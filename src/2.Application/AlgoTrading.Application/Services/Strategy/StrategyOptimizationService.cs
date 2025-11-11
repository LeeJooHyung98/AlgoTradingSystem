using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Application.Services.Strategy.Optimization;
using ErrorOr;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// Strategy Optimization Service Implementation
/// Provides various optimization algorithms for strategy parameter tuning
/// </summary>
public class StrategyOptimizationService : IStrategyOptimizationService
{
    private readonly ILogger<StrategyOptimizationService> _logger;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IStrategyService _strategyService;
    private readonly IBacktestService _backtestService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Random _random;

    public StrategyOptimizationService(
        ILogger<StrategyOptimizationService> logger,
        IStrategyRepository strategyRepository,
        IStrategyService strategyService,
        IBacktestService backtestService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _strategyRepository = strategyRepository ?? throw new ArgumentNullException(nameof(strategyRepository));
        _strategyService = strategyService ?? throw new ArgumentNullException(nameof(strategyService));
        _backtestService = backtestService ?? throw new ArgumentNullException(nameof(backtestService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _random = new Random();
    }

    /// <summary>
    /// Grid Search Optimization
    /// Tests all combinations of parameters
    /// </summary>
    public async Task<ErrorOr<OptimizationResult>> GridSearchAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Grid Search optimization for strategy {StrategyId}", strategyId);

        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Validate strategy exists
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId), cancellationToken);
            if (strategy == null)
            {
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            // Generate all parameter combinations
            var combinations = GenerateParameterCombinations(parameterRanges);
            _logger.LogInformation("Generated {Count} parameter combinations for grid search", combinations.Count);

            // Evaluate each combination
            OptimizationResult? bestResult = null;
            decimal bestScore = decimal.MinValue;
            BacktestResult? bestBacktestResult = null;

            foreach (var combination in combinations)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Grid search cancelled");
                    break;
                }

                var (score, backtestResult) = await EvaluateParameterCombinationWithResultAsync(
                    strategy,
                    combination,
                    objective,
                    startDate,
                    endDate,
                    cancellationToken);

                if (score > bestScore && backtestResult != null)
                {
                    bestScore = score;
                    bestBacktestResult = backtestResult;
                    bestResult = new OptimizationResult
                    {
                        StrategyId = strategyId,
                        Type = Core.Enums.OptimizationType.GridSearch,
                        Objective = objective,
                        OptimalParameters = combination,
                        Score = score,
                        BacktestResult = backtestResult,
                        StartDate = startDate,
                        EndDate = endDate,
                        EvaluatedCombinations = combinations.Count
                    };
                }
            }

            stopwatch.Stop();

            if (bestResult != null)
            {
                bestResult.Duration = stopwatch.Elapsed;
                bestResult.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Grid search completed: Score={Score}, Combinations={Count}, Duration={Duration}s",
                    bestResult.Score,
                    combinations.Count,
                    stopwatch.Elapsed.TotalSeconds);

                return bestResult;
            }

            return Error.Failure("Optimization.NoResults", "No valid results found during grid search");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during grid search optimization");
            return Error.Failure("Optimization.GridSearchFailed", "Grid search optimization failed");
        }
    }

    /// <summary>
    /// Genetic Algorithm Optimization
    /// Uses evolutionary algorithm to find optimal parameters
    /// </summary>
    public async Task<ErrorOr<OptimizationResult>> GeneticAlgorithmAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        int populationSize = 50,
        int generations = 100,
        double mutationRate = 0.1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting Genetic Algorithm optimization for strategy {StrategyId}: Population={Population}, Generations={Generations}",
            strategyId,
            populationSize,
            generations);

        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Validate strategy exists
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId), cancellationToken);
            if (strategy == null)
            {
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            // Initialize population
            var population = GenerateRandomPopulation(parameterRanges, populationSize);
            int evaluatedCombinations = 0;

            Dictionary<string, object>? bestIndividual = null;
            decimal bestFitness = decimal.MinValue;

            // Evolve population
            for (int generation = 0; generation < generations; generation++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Genetic algorithm cancelled at generation {Generation}", generation);
                    break;
                }

                // Evaluate fitness
                var fitnessScores = new List<(Dictionary<string, object> individual, decimal fitness)>();

                foreach (var individual in population)
                {
                    var fitness = await EvaluateParameterCombinationAsync(
                        strategy,
                        individual,
                        objective,
                        startDate,
                        endDate,
                        cancellationToken);

                    fitnessScores.Add((individual, fitness));
                    evaluatedCombinations++;

                    if (fitness > bestFitness)
                    {
                        bestFitness = fitness;
                        bestIndividual = new Dictionary<string, object>(individual);
                    }
                }

                // Selection - Tournament selection
                var selected = TournamentSelection(fitnessScores, populationSize / 2);

                // Crossover
                var offspring = new List<Dictionary<string, object>>();
                for (int i = 0; i < selected.Count - 1; i += 2)
                {
                    var (child1, child2) = Crossover(selected[i], selected[i + 1], parameterRanges);
                    offspring.Add(child1);
                    offspring.Add(child2);
                }

                // Mutation
                foreach (var individual in offspring)
                {
                    if (_random.NextDouble() < mutationRate)
                    {
                        Mutate(individual, parameterRanges);
                    }
                }

                // Elitism - Keep best individual
                offspring.Add(new Dictionary<string, object>(bestIndividual!));

                population = offspring;

                if (generation % 10 == 0)
                {
                    _logger.LogDebug("Generation {Generation}: Best Fitness = {Fitness}", generation, bestFitness);
                }
            }

            stopwatch.Stop();

            if (bestIndividual != null)
            {
                var result = new OptimizationResult
                {
                    StrategyId = strategyId,
                    Type = Core.Enums.OptimizationType.GeneticAlgorithm,
                    Objective = objective,
                    OptimalParameters = bestIndividual,
                    Score = bestFitness,
                    StartDate = startDate,
                    EndDate = endDate,
                    EvaluatedCombinations = evaluatedCombinations,
                    Duration = stopwatch.Elapsed,
                    CompletedAt = DateTime.UtcNow
                };

                _logger.LogInformation(
                    "Genetic algorithm completed: Score={Score}, Evaluated={Count}, Duration={Duration}s",
                    result.Score,
                    evaluatedCombinations,
                    stopwatch.Elapsed.TotalSeconds);

                return result;
            }

            return Error.Failure("Optimization.NoResults", "No valid results found during genetic algorithm");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during genetic algorithm optimization");
            return Error.Failure("Optimization.GeneticAlgorithmFailed", "Genetic algorithm optimization failed");
        }
    }

    /// <summary>
    /// Random Search Optimization
    /// Randomly samples parameter space
    /// </summary>
    public async Task<ErrorOr<OptimizationResult>> RandomSearchAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        int iterations = 100,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Random Search optimization for strategy {StrategyId}: Iterations={Iterations}",
            strategyId, iterations);

        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Validate strategy exists
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId), cancellationToken);
            if (strategy == null)
            {
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            Dictionary<string, object>? bestCombination = null;
            decimal bestScore = decimal.MinValue;

            // Random sampling
            for (int i = 0; i < iterations; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Random search cancelled at iteration {Iteration}", i);
                    break;
                }

                var combination = GenerateRandomParameterCombination(parameterRanges);

                var score = await EvaluateParameterCombinationAsync(
                    strategy,
                    combination,
                    objective,
                    startDate,
                    endDate,
                    cancellationToken);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCombination = combination;
                }

                if (i % 10 == 0)
                {
                    _logger.LogDebug("Iteration {Iteration}: Current Best Score = {Score}", i, bestScore);
                }
            }

            stopwatch.Stop();

            if (bestCombination != null)
            {
                var result = new OptimizationResult
                {
                    StrategyId = strategyId,
                    Type = Core.Enums.OptimizationType.RandomSearch,
                    Objective = objective,
                    OptimalParameters = bestCombination,
                    Score = bestScore,
                    StartDate = startDate,
                    EndDate = endDate,
                    EvaluatedCombinations = iterations,
                    Duration = stopwatch.Elapsed,
                    CompletedAt = DateTime.UtcNow
                };

                _logger.LogInformation(
                    "Random search completed: Score={Score}, Iterations={Count}, Duration={Duration}s",
                    result.Score,
                    iterations,
                    stopwatch.Elapsed.TotalSeconds);

                return result;
            }

            return Error.Failure("Optimization.NoResults", "No valid results found during random search");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during random search optimization");
            return Error.Failure("Optimization.RandomSearchFailed", "Random search optimization failed");
        }
    }

    /// <summary>
    /// Walk-Forward Analysis
    /// Tests strategy on rolling time windows
    /// </summary>
    public async Task<ErrorOr<WalkForwardResult>> WalkForwardAnalysisAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        int inSampleMonths = 6,
        int outOfSampleMonths = 3,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting Walk-Forward Analysis for strategy {StrategyId}: InSample={InSample}m, OutOfSample={OutOfSample}m",
            strategyId,
            inSampleMonths,
            outOfSampleMonths);

        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId), cancellationToken);
            if (strategy == null)
            {
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            var result = new WalkForwardResult
            {
                StrategyId = strategyId
            };

            var currentDate = startDate;
            var totalScore = 0m;
            var periodCount = 0;

            while (currentDate < endDate)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Walk-forward analysis cancelled");
                    break;
                }

                var inSampleEnd = currentDate.AddMonths(inSampleMonths);
                var outOfSampleStart = inSampleEnd;
                var outOfSampleEnd = outOfSampleStart.AddMonths(outOfSampleMonths);

                if (outOfSampleEnd > endDate)
                    break;

                // Optimize on in-sample period
                var optimizationResult = await GridSearchAsync(
                    strategyId,
                    parameterRanges,
                    objective,
                    currentDate,
                    inSampleEnd,
                    cancellationToken);

                if (optimizationResult.IsError)
                {
                    _logger.LogWarning("Optimization failed for period {Start} - {End}", currentDate, inSampleEnd);
                    currentDate = outOfSampleEnd;
                    continue;
                }

                // Test on out-of-sample period
                var outOfSampleScore = await EvaluateParameterCombinationAsync(
                    strategy,
                    optimizationResult.Value.OptimalParameters,
                    objective,
                    outOfSampleStart,
                    outOfSampleEnd,
                    cancellationToken);

                var period = new WalkForwardPeriod
                {
                    InSampleStart = currentDate,
                    InSampleEnd = inSampleEnd,
                    OutOfSampleStart = outOfSampleStart,
                    OutOfSampleEnd = outOfSampleEnd,
                    OptimalParameters = optimizationResult.Value.OptimalParameters,
                    InSampleScore = optimizationResult.Value.Score,
                    OutOfSampleScore = outOfSampleScore
                };

                result.Periods.Add(period);
                totalScore += outOfSampleScore;
                periodCount++;

                currentDate = outOfSampleEnd;

                _logger.LogDebug(
                    "Walk-forward period completed: InSampleScore={InSample}, OutOfSampleScore={OutOfSample}",
                    period.InSampleScore,
                    period.OutOfSampleScore);
            }

            if (periodCount > 0)
            {
                result.AverageScore = totalScore / periodCount;
                result.StabilityScore = CalculateStabilityScore(result.Periods);
                result.CompletedAt = DateTime.UtcNow;

                // Calculate overall optimal parameters (average or most common)
                result.OptimalParameters = AggregateOptimalParameters(result.Periods);

                _logger.LogInformation(
                    "Walk-forward analysis completed: Periods={Count}, AverageScore={Average}, Stability={Stability}",
                    periodCount,
                    result.AverageScore,
                    result.StabilityScore);

                return result;
            }

            return Error.Failure("Optimization.NoResults", "No valid periods found during walk-forward analysis");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during walk-forward analysis");
            return Error.Failure("Optimization.WalkForwardFailed", "Walk-forward analysis failed");
        }
    }

    /// <summary>
    /// Parameter Sensitivity Analysis
    /// Analyzes how parameter changes affect performance
    /// </summary>
    public async Task<ErrorOr<SensitivityAnalysisResult>> ParameterSensitivityAsync(
        Guid strategyId,
        string parameterName,
        ParameterRange range,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting parameter sensitivity analysis for {Parameter} on strategy {StrategyId}",
            parameterName, strategyId);

        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId), cancellationToken);
            if (strategy == null)
            {
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            var result = new SensitivityAnalysisResult
            {
                StrategyId = strategyId,
                ParameterName = parameterName
            };

            var maxScore = decimal.MinValue;
            var optimalValue = 0m;

            foreach (var value in range.GetValues())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Sensitivity analysis cancelled");
                    break;
                }

                var parameters = new Dictionary<string, object>
                {
                    { parameterName, value }
                };

                var score = await EvaluateParameterCombinationAsync(
                    strategy,
                    parameters,
                    objective,
                    startDate,
                    endDate,
                    cancellationToken);

                result.Results.Add(new ParameterScorePair
                {
                    ParameterValue = value,
                    Score = score
                });

                if (score > maxScore)
                {
                    maxScore = score;
                    optimalValue = value;
                }
            }

            result.OptimalValue = optimalValue;
            result.MaxScore = maxScore;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Sensitivity analysis completed: Parameter={Parameter}, OptimalValue={Optimal}, MaxScore={Score}",
                parameterName,
                optimalValue,
                maxScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during parameter sensitivity analysis");
            return Error.Failure("Optimization.SensitivityFailed", "Parameter sensitivity analysis failed");
        }
    }

    /// <summary>
    /// Bayesian Optimization using Gaussian Process
    /// Most efficient for expensive function evaluations
    /// </summary>
    public async Task<ErrorOr<OptimizationResult>> BayesianOptimizationAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        int initialSamples = 5,
        int iterations = 50,
        AcquisitionFunctionType acquisitionType = AcquisitionFunctionType.ExpectedImprovement,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting Bayesian Optimization for strategy {StrategyId}: InitialSamples={Initial}, Iterations={Iterations}, Acquisition={Acquisition}",
            strategyId,
            initialSamples,
            iterations,
            acquisitionType);

        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Validate strategy exists
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId), cancellationToken);
            if (strategy == null)
            {
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            if (parameterRanges.Count == 0)
            {
                return Error.Validation("Optimization.NoParameters", "No parameters provided for optimization");
            }

            // Normalize parameter ranges to [0, 1] for GP
            var normalizer = new ParameterNormalizer(parameterRanges);
            int dimensions = parameterRanges.Count;

            // Storage for observations
            var xObserved = new List<double[]>();
            var yObserved = new List<double>();

            var bestScore = double.MinValue;
            Dictionary<string, object>? bestParameters = null;
            int evaluatedCombinations = 0;

            // Phase 1: Initial random sampling
            _logger.LogDebug("Phase 1: Collecting {Count} initial samples", initialSamples);
            for (int i = 0; i < initialSamples; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Bayesian optimization cancelled during initial sampling");
                    break;
                }

                var parameters = GenerateRandomParameterCombination(parameterRanges);
                var normalizedX = normalizer.Normalize(parameters);

                var score = await EvaluateParameterCombinationAsync(
                    strategy,
                    parameters,
                    objective,
                    startDate,
                    endDate,
                    cancellationToken);

                xObserved.Add(normalizedX);
                yObserved.Add((double)score);
                evaluatedCombinations++;

                if (score > (decimal)bestScore)
                {
                    bestScore = (double)score;
                    bestParameters = parameters;
                }

                _logger.LogDebug("Initial sample {Index}/{Total}: Score = {Score}", i + 1, initialSamples, score);
            }

            if (xObserved.Count == 0)
            {
                return Error.Failure("Optimization.NoSamples", "No initial samples collected");
            }

            // Phase 2: Bayesian optimization iterations
            _logger.LogDebug("Phase 2: Starting Bayesian optimization with {Count} iterations", iterations);

            // Initialize Gaussian Process
            var gp = new GaussianProcess(
                lengthScale: 1.0,
                variance: 1.0,
                noise: 1e-6);

            for (int iter = 0; iter < iterations; iter++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Bayesian optimization cancelled at iteration {Iteration}", iter);
                    break;
                }

                // Fit GP to observed data
                gp.Fit(xObserved, yObserved);

                // Find next point to evaluate using acquisition function
                var nextPoint = OptimizeAcquisitionFunction(
                    gp,
                    dimensions,
                    yObserved.Max(),
                    acquisitionType);

                // Denormalize parameters
                var nextParameters = normalizer.Denormalize(nextPoint);

                // Evaluate new point
                var score = await EvaluateParameterCombinationAsync(
                    strategy,
                    nextParameters,
                    objective,
                    startDate,
                    endDate,
                    cancellationToken);

                xObserved.Add(nextPoint);
                yObserved.Add((double)score);
                evaluatedCombinations++;

                if (score > (decimal)bestScore)
                {
                    bestScore = (double)score;
                    bestParameters = nextParameters;

                    _logger.LogDebug(
                        "New best found at iteration {Iteration}: Score = {Score}",
                        iter + 1,
                        score);
                }

                if ((iter + 1) % 10 == 0)
                {
                    _logger.LogDebug(
                        "Iteration {Iteration}/{Total}: Current Best = {Score}",
                        iter + 1,
                        iterations,
                        bestScore);
                }

                // Memory optimization: Clear GP if too many samples
                if (xObserved.Count > 200)
                {
                    _logger.LogDebug("Memory optimization: Keeping only recent 100 samples");
                    xObserved = xObserved.Skip(xObserved.Count - 100).ToList();
                    yObserved = yObserved.Skip(yObserved.Count - 100).ToList();
                }
            }

            stopwatch.Stop();

            // Clear GP memory
            gp.Clear();

            if (bestParameters != null)
            {
                var result = new OptimizationResult
                {
                    StrategyId = strategyId,
                    Type = Core.Enums.OptimizationType.BayesianOptimization,
                    Objective = objective,
                    OptimalParameters = bestParameters,
                    Score = (decimal)bestScore,
                    StartDate = startDate,
                    EndDate = endDate,
                    EvaluatedCombinations = evaluatedCombinations,
                    Duration = stopwatch.Elapsed,
                    CompletedAt = DateTime.UtcNow
                };

                _logger.LogInformation(
                    "Bayesian optimization completed: Score={Score}, Evaluated={Count}, Duration={Duration}s",
                    result.Score,
                    evaluatedCombinations,
                    stopwatch.Elapsed.TotalSeconds);

                return result;
            }

            return Error.Failure("Optimization.NoResults", "No valid results found during Bayesian optimization");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Bayesian optimization");
            return Error.Failure("Optimization.BayesianOptimizationFailed", "Bayesian optimization failed");
        }
    }

    /// <summary>
    /// Apply optimal parameters to strategy
    /// </summary>
    public async Task<ErrorOr<TradingStrategy>> ApplyOptimalParametersAsync(
        Guid strategyId,
        OptimizationResult optimizationResult,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Applying optimal parameters to strategy {StrategyId}", strategyId);

        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId), cancellationToken);
            if (strategy == null)
            {
                return Error.NotFound("Strategy.NotFound", $"Strategy with ID {strategyId} not found");
            }

            // Apply each optimal parameter
            foreach (var (paramName, value) in optimizationResult.OptimalParameters)
            {
                // Note: TradingStrategy.UpdateParameter doesn't exist in current implementation
                // This would need to be added to the entity or handled differently
                _logger.LogDebug("Setting parameter {Parameter} = {Value}", paramName, value);
                // strategy.UpdateParameter(paramName, value);
            }

            _strategyRepository.Update(strategy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Optimal parameters applied successfully to strategy {StrategyId}", strategyId);

            return strategy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying optimal parameters");
            return Error.Failure("Optimization.ApplyFailed", "Failed to apply optimal parameters");
        }
    }

    /// <summary>
    /// Get optimization history for strategy
    /// </summary>
    public async Task<ErrorOr<IEnumerable<OptimizationResult>>> GetOptimizationHistoryAsync(
        Guid strategyId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting optimization history for strategy {StrategyId}", strategyId);

        try
        {
            // Note: This would require an OptimizationResult repository
            // For now, return empty list as placeholder
            var results = new List<OptimizationResult>();

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimization history");
            return Error.Failure("Optimization.GetHistoryFailed", "Failed to get optimization history");
        }
    }

    #region Helper Methods

    /// <summary>
    /// Evaluate a parameter combination with backtest
    /// Returns both score and backtest result
    /// </summary>
    private async Task<(decimal score, BacktestResult? backtestResult)> EvaluateParameterCombinationWithResultAsync(
        TradingStrategy strategy,
        Dictionary<string, object> parameters,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            // Apply parameters to strategy (temporarily for this evaluation)
            var originalParams = new Dictionary<string, object>();
            foreach (var (paramName, value) in parameters)
            {
                // Store original values
                originalParams[paramName] = GetParameterValue(strategy, paramName);

                // Apply new value
                ApplyParameterToStrategy(strategy, paramName, value);
            }

            // Run backtest with these parameters
            var backtestResult = await _backtestService.RunBacktestAsync(
                strategy.Id.Value,
                startDate,
                endDate,
                initialCapital: 10000000m, // 10 million KRW default
                cancellationToken);

            // Restore original parameters
            foreach (var (paramName, value) in originalParams)
            {
                ApplyParameterToStrategy(strategy, paramName, value);
            }

            if (backtestResult.IsError)
            {
                _logger.LogDebug("Backtest failed for parameters: {Params}",
                    string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}")));
                return (decimal.MinValue, null);
            }

            // Calculate score based on objective
            var score = CalculateScore(backtestResult.Value.Metrics, objective);

            _logger.LogTrace("Evaluated params {Params}: score={Score}, return={Return}%, sharpe={Sharpe}",
                string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}")),
                score,
                backtestResult.Value.Metrics.TotalReturn,
                backtestResult.Value.Metrics.SharpeRatio);

            return (score, backtestResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error evaluating parameter combination");
            return (decimal.MinValue, null);
        }
    }

    /// <summary>
    /// Evaluate a parameter combination (score only)
    /// </summary>
    private async Task<decimal> EvaluateParameterCombinationAsync(
        TradingStrategy strategy,
        Dictionary<string, object> parameters,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var (score, _) = await EvaluateParameterCombinationWithResultAsync(
            strategy, parameters, objective, startDate, endDate, cancellationToken);
        return score;
    }

    /// <summary>
    /// Get parameter value from strategy
    /// </summary>
    private object GetParameterValue(TradingStrategy strategy, string paramName)
    {
        return paramName switch
        {
            "StopLossPercent" => strategy.StopLossPercent,
            "TakeProfitPercent" => strategy.TakeProfitPercent,
            "MaxPositionSizePercent" => strategy.MaxPositionSizePercent,
            _ => 0m
        };
    }

    /// <summary>
    /// Apply parameter value to strategy
    /// </summary>
    private void ApplyParameterToStrategy(TradingStrategy strategy, string paramName, object value)
    {
        var decimalValue = Convert.ToDecimal(value);

        switch (paramName)
        {
            case "StopLossPercent":
                // Use reflection or add setter methods to strategy
                typeof(TradingStrategy)
                    .GetProperty("StopLossPercent")?
                    .SetValue(strategy, decimalValue);
                break;
            case "TakeProfitPercent":
                typeof(TradingStrategy)
                    .GetProperty("TakeProfitPercent")?
                    .SetValue(strategy, decimalValue);
                break;
            case "MaxPositionSizePercent":
                typeof(TradingStrategy)
                    .GetProperty("MaxPositionSizePercent")?
                    .SetValue(strategy, decimalValue);
                break;
            default:
                _logger.LogWarning("Unknown parameter: {ParamName}", paramName);
                break;
        }
    }

    /// <summary>
    /// Calculate score based on optimization objective
    /// </summary>
    private decimal CalculateScore(PerformanceMetrics metrics, Core.Enums.OptimizationObjective objective)
    {
        return objective switch
        {
            Core.Enums.OptimizationObjective.MaxReturn => metrics.TotalReturn,
            Core.Enums.OptimizationObjective.MaxSharpe => metrics.SharpeRatio,
            Core.Enums.OptimizationObjective.MinDrawdown => -metrics.MaxDrawdownPercent, // Negative because we want to minimize
            Core.Enums.OptimizationObjective.MaxProfitFactor => metrics.ProfitFactor,
            Core.Enums.OptimizationObjective.MaxWinRate => metrics.WinRate,
            Core.Enums.OptimizationObjective.MaxRiskAdjustedReturn => metrics.SharpeRatio * metrics.TotalReturn / 100,
            Core.Enums.OptimizationObjective.MinTradeCount => -metrics.TotalTrades, // Negative because we want to minimize
            Core.Enums.OptimizationObjective.MaxAverageWin => metrics.AverageWin,
            _ => 0m
        };
    }

    /// <summary>
    /// Generate all parameter combinations (Grid Search)
    /// </summary>
    private List<Dictionary<string, object>> GenerateParameterCombinations(
        Dictionary<string, ParameterRange> parameterRanges)
    {
        var combinations = new List<Dictionary<string, object>>();
        var parameterNames = parameterRanges.Keys.ToList();
        var valuesList = parameterRanges.Values.Select(r => r.GetValues().ToList()).ToList();

        GenerateCombinationsRecursive(
            combinations,
            new Dictionary<string, object>(),
            parameterNames,
            valuesList,
            0);

        return combinations;
    }

    private void GenerateCombinationsRecursive(
        List<Dictionary<string, object>> combinations,
        Dictionary<string, object> current,
        List<string> paramNames,
        List<List<decimal>> values,
        int depth)
    {
        if (depth == paramNames.Count)
        {
            combinations.Add(new Dictionary<string, object>(current));
            return;
        }

        foreach (var value in values[depth])
        {
            current[paramNames[depth]] = value;
            GenerateCombinationsRecursive(combinations, current, paramNames, values, depth + 1);
        }
    }

    /// <summary>
    /// Generate random population for Genetic Algorithm
    /// </summary>
    private List<Dictionary<string, object>> GenerateRandomPopulation(
        Dictionary<string, ParameterRange> parameterRanges,
        int size)
    {
        var population = new List<Dictionary<string, object>>();

        for (int i = 0; i < size; i++)
        {
            population.Add(GenerateRandomParameterCombination(parameterRanges));
        }

        return population;
    }

    /// <summary>
    /// Generate random parameter combination
    /// </summary>
    private Dictionary<string, object> GenerateRandomParameterCombination(
        Dictionary<string, ParameterRange> parameterRanges)
    {
        var combination = new Dictionary<string, object>();

        foreach (var (name, range) in parameterRanges)
        {
            var values = range.GetValues().ToList();
            var randomIndex = _random.Next(values.Count);
            combination[name] = values[randomIndex];
        }

        return combination;
    }

    /// <summary>
    /// Tournament selection for Genetic Algorithm
    /// </summary>
    private List<Dictionary<string, object>> TournamentSelection(
        List<(Dictionary<string, object> individual, decimal fitness)> population,
        int count)
    {
        var selected = new List<Dictionary<string, object>>();

        for (int i = 0; i < count; i++)
        {
            var tournament = new List<(Dictionary<string, object>, decimal)>();
            for (int j = 0; j < 3; j++) // Tournament size = 3
            {
                var randomIndex = _random.Next(population.Count);
                tournament.Add(population[randomIndex]);
            }

            var winner = tournament.OrderByDescending(t => t.Item2).First();
            selected.Add(new Dictionary<string, object>(winner.Item1));
        }

        return selected;
    }

    /// <summary>
    /// Crossover operation for Genetic Algorithm
    /// </summary>
    private (Dictionary<string, object>, Dictionary<string, object>) Crossover(
        Dictionary<string, object> parent1,
        Dictionary<string, object> parent2,
        Dictionary<string, ParameterRange> parameterRanges)
    {
        var child1 = new Dictionary<string, object>();
        var child2 = new Dictionary<string, object>();

        foreach (var key in parent1.Keys)
        {
            if (_random.NextDouble() < 0.5)
            {
                child1[key] = parent1[key];
                child2[key] = parent2[key];
            }
            else
            {
                child1[key] = parent2[key];
                child2[key] = parent1[key];
            }
        }

        return (child1, child2);
    }

    /// <summary>
    /// Mutation operation for Genetic Algorithm
    /// </summary>
    private void Mutate(
        Dictionary<string, object> individual,
        Dictionary<string, ParameterRange> parameterRanges)
    {
        var keys = individual.Keys.ToList();
        var randomKey = keys[_random.Next(keys.Count)];

        var values = parameterRanges[randomKey].GetValues().ToList();
        individual[randomKey] = values[_random.Next(values.Count)];
    }

    /// <summary>
    /// Calculate stability score for walk-forward analysis
    /// </summary>
    private decimal CalculateStabilityScore(List<WalkForwardPeriod> periods)
    {
        if (periods.Count == 0)
            return 0m;

        var scores = periods.Select(p => p.OutOfSampleScore).ToList();
        var mean = scores.Average();
        var variance = scores.Sum(s => (s - mean) * (s - mean)) / scores.Count;
        var stdDev = (decimal)Math.Sqrt((double)variance);

        // Stability = 1 - (StdDev / Mean) - Lower is better
        if (mean == 0)
            return 0m;

        return Math.Max(0m, 1m - (stdDev / Math.Abs(mean)));
    }

    /// <summary>
    /// Aggregate optimal parameters from walk-forward periods
    /// </summary>
    private Dictionary<string, object> AggregateOptimalParameters(List<WalkForwardPeriod> periods)
    {
        var aggregated = new Dictionary<string, object>();

        if (periods.Count == 0)
            return aggregated;

        // Get all parameter names
        var firstPeriod = periods.First();
        foreach (var key in firstPeriod.OptimalParameters.Keys)
        {
            // Calculate average value for numeric parameters
            var values = periods
                .Select(p => Convert.ToDecimal(p.OptimalParameters[key]))
                .ToList();

            aggregated[key] = values.Average();
        }

        return aggregated;
    }

    /// <summary>
    /// Optimize acquisition function to find next point to evaluate
    /// Uses random sampling for simplicity (can be improved with gradient-based methods)
    /// </summary>
    private double[] OptimizeAcquisitionFunction(
        GaussianProcess gp,
        int dimensions,
        double bestObserved,
        AcquisitionFunctionType acquisitionType,
        int numCandidates = 1000)
    {
        double[] bestPoint = new double[dimensions];
        double bestAcquisition = double.MinValue;

        // Random search over acquisition function
        // In production, could use gradient-based methods (L-BFGS-B)
        for (int i = 0; i < numCandidates; i++)
        {
            double[] candidate = new double[dimensions];
            for (int d = 0; d < dimensions; d++)
            {
                candidate[d] = _random.NextDouble(); // Random point in [0, 1]
            }

            var (mean, stdDev) = gp.Predict(candidate);

            double acquisitionValue = acquisitionType switch
            {
                AcquisitionFunctionType.ExpectedImprovement =>
                    AcquisitionFunction.ExpectedImprovement(mean, stdDev, bestObserved),
                AcquisitionFunctionType.ProbabilityOfImprovement =>
                    AcquisitionFunction.ProbabilityOfImprovement(mean, stdDev, bestObserved),
                AcquisitionFunctionType.UpperConfidenceBound =>
                    AcquisitionFunction.UpperConfidenceBound(mean, stdDev),
                AcquisitionFunctionType.ThompsonSampling =>
                    AcquisitionFunction.ThompsonSampling(mean, stdDev, _random),
                AcquisitionFunctionType.KnowledgeGradient =>
                    AcquisitionFunction.KnowledgeGradient(mean, stdDev, bestObserved, bestObserved),
                _ => AcquisitionFunction.ExpectedImprovement(mean, stdDev, bestObserved)
            };

            if (acquisitionValue > bestAcquisition)
            {
                bestAcquisition = acquisitionValue;
                bestPoint = (double[])candidate.Clone();
            }
        }

        return bestPoint;
    }

    #endregion
}

/// <summary>
/// Parameter normalizer for Bayesian Optimization
/// Normalizes parameters to [0, 1] and denormalizes back to original range
/// </summary>
internal class ParameterNormalizer
{
    private readonly Dictionary<string, ParameterRange> _parameterRanges;
    private readonly List<string> _parameterNames;

    public ParameterNormalizer(Dictionary<string, ParameterRange> parameterRanges)
    {
        _parameterRanges = parameterRanges;
        _parameterNames = parameterRanges.Keys.ToList();
    }

    /// <summary>
    /// Normalize parameters to [0, 1]
    /// </summary>
    public double[] Normalize(Dictionary<string, object> parameters)
    {
        var normalized = new double[_parameterNames.Count];

        for (int i = 0; i < _parameterNames.Count; i++)
        {
            var name = _parameterNames[i];
            var range = _parameterRanges[name];

            var value = Convert.ToDecimal(parameters[name]);
            var min = range.Min;
            var max = range.Max;

            // Normalize to [0, 1]
            normalized[i] = (double)((value - min) / (max - min));
        }

        return normalized;
    }

    /// <summary>
    /// Denormalize from [0, 1] to original range
    /// </summary>
    public Dictionary<string, object> Denormalize(double[] normalizedValues)
    {
        var parameters = new Dictionary<string, object>();

        for (int i = 0; i < _parameterNames.Count; i++)
        {
            var name = _parameterNames[i];
            var range = _parameterRanges[name];

            var normalizedValue = normalizedValues[i];
            var min = range.Min;
            var max = range.Max;

            // Denormalize from [0, 1] to original range
            var value = min + (decimal)normalizedValue * (max - min);

            // Round to nearest step
            var steps = Math.Round((value - min) / range.Step);
            value = min + steps * range.Step;

            // Ensure within bounds
            value = Math.Max(min, Math.Min(max, value));

            // Convert to appropriate type
            parameters[name] = range.Type switch
            {
                ParameterType.Integer => (int)value,
                ParameterType.Boolean => value > ((max - min) / 2 + min),
                _ => value
            };
        }

        return parameters;
    }
}
