using AlgoTrading.Core.Entities.Strategy;
using ErrorOr;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// Interface for Strategy Optimization Service
/// Provides parameter optimization algorithms for trading strategies
/// </summary>
public interface IStrategyOptimizationService
{
    /// <summary>
    /// Optimize strategy using Grid Search
    /// Tests all combinations of parameters
    /// </summary>
    Task<ErrorOr<OptimizationResult>> GridSearchAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimize strategy using Genetic Algorithm
    /// Evolutionary algorithm to find optimal parameters
    /// </summary>
    Task<ErrorOr<OptimizationResult>> GeneticAlgorithmAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        int populationSize = 50,
        int generations = 100,
        double mutationRate = 0.1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimize strategy using Random Search
    /// Random parameter sampling for optimization
    /// </summary>
    Task<ErrorOr<OptimizationResult>> RandomSearchAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        int iterations = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Walk-Forward Analysis
    /// Tests strategy on rolling time windows
    /// </summary>
    Task<ErrorOr<WalkForwardResult>> WalkForwardAnalysisAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        int inSampleMonths = 6,
        int outOfSampleMonths = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Parameter sensitivity analysis
    /// Analyzes how each parameter affects performance
    /// </summary>
    Task<ErrorOr<SensitivityAnalysisResult>> ParameterSensitivityAsync(
        Guid strategyId,
        string parameterName,
        ParameterRange range,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimize strategy using Bayesian Optimization
    /// Uses Gaussian Process and acquisition functions for efficient parameter search
    /// </summary>
    Task<ErrorOr<OptimizationResult>> BayesianOptimizationAsync(
        Guid strategyId,
        Dictionary<string, ParameterRange> parameterRanges,
        Core.Enums.OptimizationObjective objective,
        DateTime startDate,
        DateTime endDate,
        int initialSamples = 5,
        int iterations = 50,
        Optimization.AcquisitionFunctionType acquisitionType = Optimization.AcquisitionFunctionType.ExpectedImprovement,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply optimal parameters to strategy
    /// </summary>
    Task<ErrorOr<TradingStrategy>> ApplyOptimalParametersAsync(
        Guid strategyId,
        OptimizationResult optimizationResult,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get optimization history for strategy
    /// </summary>
    Task<ErrorOr<IEnumerable<OptimizationResult>>> GetOptimizationHistoryAsync(
        Guid strategyId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Parameter range definition for optimization
/// </summary>
public class ParameterRange
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal Step { get; set; }
    public ParameterType Type { get; set; }

    public ParameterRange(decimal min, decimal max, decimal step, ParameterType type = ParameterType.Decimal)
    {
        Min = min;
        Max = max;
        Step = step;
        Type = type;
    }

    public IEnumerable<decimal> GetValues()
    {
        var values = new List<decimal>();
        for (var value = Min; value <= Max; value += Step)
        {
            values.Add(value);
        }
        return values;
    }
}

/// <summary>
/// Parameter type enumeration
/// </summary>
public enum ParameterType
{
    Integer,
    Decimal,
    Boolean
}

/// <summary>
/// Optimization result
/// </summary>
public class OptimizationResult
{
    public Guid Id { get; set; }
    public Guid StrategyId { get; set; }
    public Core.Enums.OptimizationType Type { get; set; }
    public Core.Enums.OptimizationObjective Objective { get; set; }
    public Dictionary<string, object> OptimalParameters { get; set; }
    public decimal Score { get; set; }
    public BacktestResult BacktestResult { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CompletedAt { get; set; }
    public int EvaluatedCombinations { get; set; }
    public TimeSpan Duration { get; set; }

    public OptimizationResult()
    {
        Id = Guid.NewGuid();
        OptimalParameters = new Dictionary<string, object>();
    }
}

/// <summary>
/// Walk-forward analysis result
/// </summary>
public class WalkForwardResult
{
    public Guid Id { get; set; }
    public Guid StrategyId { get; set; }
    public List<WalkForwardPeriod> Periods { get; set; }
    public Dictionary<string, object> OptimalParameters { get; set; }
    public decimal AverageScore { get; set; }
    public decimal StabilityScore { get; set; }
    public DateTime CompletedAt { get; set; }

    public WalkForwardResult()
    {
        Id = Guid.NewGuid();
        Periods = new List<WalkForwardPeriod>();
        OptimalParameters = new Dictionary<string, object>();
    }
}

/// <summary>
/// Walk-forward period
/// </summary>
public class WalkForwardPeriod
{
    public DateTime InSampleStart { get; set; }
    public DateTime InSampleEnd { get; set; }
    public DateTime OutOfSampleStart { get; set; }
    public DateTime OutOfSampleEnd { get; set; }
    public Dictionary<string, object> OptimalParameters { get; set; }
    public decimal InSampleScore { get; set; }
    public decimal OutOfSampleScore { get; set; }
}

/// <summary>
/// Sensitivity analysis result
/// </summary>
public class SensitivityAnalysisResult
{
    public Guid Id { get; set; }
    public Guid StrategyId { get; set; }
    public string ParameterName { get; set; }
    public List<ParameterScorePair> Results { get; set; }
    public decimal OptimalValue { get; set; }
    public decimal MaxScore { get; set; }
    public DateTime CompletedAt { get; set; }

    public SensitivityAnalysisResult()
    {
        Id = Guid.NewGuid();
        Results = new List<ParameterScorePair>();
    }
}

/// <summary>
/// Parameter-score pair for sensitivity analysis
/// </summary>
public class ParameterScorePair
{
    public decimal ParameterValue { get; set; }
    public decimal Score { get; set; }
    public BacktestResult BacktestResult { get; set; }
}

// Note: BacktestResult is defined in IBacktestService.cs
