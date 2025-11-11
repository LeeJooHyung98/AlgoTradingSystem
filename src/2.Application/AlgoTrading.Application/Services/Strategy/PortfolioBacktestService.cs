using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// description(설명) : Portfolio Backtest Service (포트폴리오 백테스트 서비스)
/// Details(상세설명) : Runs backtests for multiple strategies and analyzes portfolio performance
/// Applied technology patterns(적용기술패턴) : Service Pattern, Portfolio Theory
/// </summary>
public class PortfolioBacktestService : IPortfolioBacktestService
{
    private readonly ILogger<PortfolioBacktestService> _logger;
    private readonly IBacktestService _backtestService;
    private readonly IStrategyRepository _strategyRepository;

    public PortfolioBacktestService(
        ILogger<PortfolioBacktestService> logger,
        IBacktestService backtestService,
        IStrategyRepository strategyRepository)
    {
        _logger = logger;
        _backtestService = backtestService;
        _strategyRepository = strategyRepository;
    }

    /// <summary>
    /// Run portfolio backtest for multiple strategies
    /// </summary>
    public async Task<ErrorOr<PortfolioBacktestResult>> RunPortfolioBacktestAsync(
        IEnumerable<Guid> strategyIds,
        DateTime startDate,
        DateTime endDate,
        decimal initialCapital,
        PortfolioAllocationMethod allocationMethod = PortfolioAllocationMethod.EqualWeight,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var strategyIdsList = strategyIds.ToList();

            _logger.LogInformation(
                "Running portfolio backtest for {Count} strategies from {StartDate} to {EndDate}",
                strategyIdsList.Count, startDate, endDate);

            if (strategyIdsList.Count == 0)
            {
                return Error.Validation("Portfolio.NoStrategies", "At least one strategy is required for portfolio backtest");
            }

            // Run individual backtests for each strategy
            var strategyResults = new Dictionary<Guid, BacktestResult>();
            var backtestTasks = strategyIdsList.Select(async strategyId =>
            {
                var result = await _backtestService.RunBacktestAsync(
                    strategyId,
                    startDate,
                    endDate,
                    initialCapital, // Each strategy gets same initial capital for comparison
                    cancellationToken);

                return (strategyId, result);
            });

            var results = await Task.WhenAll(backtestTasks);

            foreach (var (strategyId, result) in results)
            {
                if (result.IsError)
                {
                    _logger.LogWarning("Backtest failed for strategy {StrategyId}: {Error}",
                        strategyId, result.FirstError.Description);
                    continue;
                }

                strategyResults[strategyId] = result.Value;
            }

            if (!strategyResults.Any())
            {
                return Error.Failure("Portfolio.NoResults", "All strategy backtests failed");
            }

            // Calculate portfolio allocations
            var allocations = CalculateAllocations(
                strategyResults,
                allocationMethod,
                initialCapital);

            // Combine strategy equity curves into portfolio equity curve
            var portfolioEquityCurve = CombineEquityCurves(
                strategyResults,
                allocations,
                startDate,
                endDate);

            // Calculate portfolio metrics
            var portfolioMetrics = CalculatePortfolioMetrics(
                portfolioEquityCurve,
                initialCapital,
                startDate,
                endDate);

            var finalCapital = portfolioEquityCurve.Last().Equity;

            var portfolioResult = new PortfolioBacktestResult
            {
                PortfolioId = Guid.NewGuid(),
                StartDate = startDate,
                EndDate = endDate,
                InitialCapital = initialCapital,
                FinalCapital = finalCapital,
                StrategyAllocations = allocations,
                PortfolioMetrics = portfolioMetrics,
                PortfolioEquityCurve = portfolioEquityCurve,
                StrategyResults = strategyResults
            };

            _logger.LogInformation(
                "Portfolio backtest completed. Strategies: {Count}, Total Return: {Return:P2}, Sharpe: {Sharpe:F2}",
                strategyResults.Count,
                portfolioMetrics.TotalReturn / 100,
                portfolioMetrics.SharpeRatio);

            return portfolioResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running portfolio backtest");
            return Error.Failure("Portfolio.BacktestFailed",
                $"Portfolio backtest failed: {ex.Message} | StackTrace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Get portfolio correlation matrix
    /// </summary>
    public async Task<ErrorOr<CorrelationMatrix>> GetStrategyCorrelationAsync(
        IEnumerable<Guid> strategyIds,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var strategyIdsList = strategyIds.ToList();
            var matrix = new CorrelationMatrix
            {
                StrategyIds = strategyIdsList
            };

            // Run backtests to get equity curves
            var backtestResults = new Dictionary<Guid, List<decimal>>();

            foreach (var strategyId in strategyIdsList)
            {
                var result = await _backtestService.RunBacktestAsync(
                    strategyId,
                    startDate,
                    endDate,
                    10000000m, // Fixed capital for correlation
                    cancellationToken);

                if (!result.IsError)
                {
                    var returns = CalculateReturns(result.Value.EquityCurve);
                    backtestResults[strategyId] = returns;
                }
            }

            // Calculate correlations
            foreach (var strategy1 in strategyIdsList)
            {
                foreach (var strategy2 in strategyIdsList)
                {
                    if (strategy1 == strategy2)
                    {
                        matrix.SetCorrelation(strategy1, strategy2, 1.0m);
                    }
                    else if (backtestResults.ContainsKey(strategy1) && backtestResults.ContainsKey(strategy2))
                    {
                        var correlation = CalculateCorrelation(
                            backtestResults[strategy1],
                            backtestResults[strategy2]);

                        matrix.SetCorrelation(strategy1, strategy2, correlation);
                    }
                }
            }

            return matrix;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating strategy correlation");
            return Error.Failure("Portfolio.CorrelationFailed", "Failed to calculate correlation");
        }
    }

    /// <summary>
    /// Calculate strategy allocations based on method
    /// </summary>
    private List<StrategyAllocation> CalculateAllocations(
        Dictionary<Guid, BacktestResult> strategyResults,
        PortfolioAllocationMethod method,
        decimal totalCapital)
    {
        var allocations = new List<StrategyAllocation>();

        switch (method)
        {
            case PortfolioAllocationMethod.EqualWeight:
                {
                    var equalWeight = 1.0m / strategyResults.Count;
                    foreach (var (strategyId, result) in strategyResults)
                    {
                        allocations.Add(new StrategyAllocation
                        {
                            StrategyId = strategyId,
                            StrategyName = $"Strategy {strategyId}",
                            AllocationPercent = equalWeight * 100,
                            AllocatedCapital = totalCapital * equalWeight,
                            Metrics = result.Metrics
                        });
                    }
                }
                break;

            case PortfolioAllocationMethod.SharpeWeighted:
                {
                    var totalSharpe = strategyResults.Values.Sum(r => Math.Max(0, r.Metrics.SharpeRatio));

                    // If all Sharpe ratios are 0 or negative, use equal weighting
                    if (totalSharpe == 0)
                    {
                        _logger.LogWarning("All Sharpe ratios are zero or negative, using equal weighting");
                        var equalWeight = 1m / strategyResults.Count;
                        foreach (var (strategyId, result) in strategyResults)
                        {
                            allocations.Add(new StrategyAllocation
                            {
                                StrategyId = strategyId,
                                StrategyName = $"Strategy {strategyId}",
                                AllocationPercent = equalWeight * 100,
                                AllocatedCapital = totalCapital * equalWeight,
                                Metrics = result.Metrics
                            });
                        }
                    }
                    else
                    {
                        // Use Sharpe ratio weighting
                        foreach (var (strategyId, result) in strategyResults)
                        {
                            var weight = Math.Max(0, result.Metrics.SharpeRatio) / totalSharpe;
                            allocations.Add(new StrategyAllocation
                            {
                                StrategyId = strategyId,
                                StrategyName = $"Strategy {strategyId}",
                                AllocationPercent = weight * 100,
                                AllocatedCapital = totalCapital * weight,
                                Metrics = result.Metrics
                            });
                        }
                    }
                }
                break;

            case PortfolioAllocationMethod.ReturnWeighted:
                {
                    var totalReturn = strategyResults.Values.Sum(r => Math.Max(0, r.Metrics.TotalReturn));
                    if (totalReturn == 0) totalReturn = 1;

                    foreach (var (strategyId, result) in strategyResults)
                    {
                        var weight = Math.Max(0, result.Metrics.TotalReturn) / totalReturn;
                        allocations.Add(new StrategyAllocation
                        {
                            StrategyId = strategyId,
                            StrategyName = $"Strategy {strategyId}",
                            AllocationPercent = weight * 100,
                            AllocatedCapital = totalCapital * weight,
                            Metrics = result.Metrics
                        });
                    }
                }
                break;

            default:
                // Fall back to equal weight
                goto case PortfolioAllocationMethod.EqualWeight;
        }

        return allocations;
    }

    /// <summary>
    /// Combine equity curves from multiple strategies
    /// </summary>
    private List<EquityCurvePoint> CombineEquityCurves(
        Dictionary<Guid, BacktestResult> strategyResults,
        List<StrategyAllocation> allocations,
        DateTime startDate,
        DateTime endDate)
    {
        // Get all unique timestamps
        var allTimestamps = strategyResults.Values
            .SelectMany(r => r.EquityCurve.Select(e => e.Time))
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        var portfolioEquityCurve = new List<EquityCurvePoint>();
        decimal peak = 0;

        foreach (var timestamp in allTimestamps)
        {
            decimal totalEquity = 0;

            foreach (var allocation in allocations)
            {
                if (strategyResults.TryGetValue(allocation.StrategyId, out var result))
                {
                    // Find equity at this timestamp
                    var equityPoint = result.EquityCurve
                        .Where(e => e.Time <= timestamp)
                        .OrderByDescending(e => e.Time)
                        .FirstOrDefault();

                    if (equityPoint != null && result.InitialCapital > 0)
                    {
                        var strategyEquityPercent = equityPoint.Equity / result.InitialCapital;
                        totalEquity += allocation.AllocatedCapital * strategyEquityPercent;
                    }
                    else if (equityPoint != null)
                    {
                        // If InitialCapital is 0, use allocated capital as-is
                        totalEquity += allocation.AllocatedCapital;
                    }
                }
            }

            if (totalEquity > peak) peak = totalEquity;
            var drawdown = peak - totalEquity;
            var drawdownPercent = peak > 0 ? (drawdown / peak) * 100 : 0;

            portfolioEquityCurve.Add(new EquityCurvePoint
            {
                Time = timestamp,
                Equity = totalEquity,
                Drawdown = drawdown,
                DrawdownPercent = drawdownPercent
            });
        }

        return portfolioEquityCurve;
    }

    /// <summary>
    /// Calculate portfolio-level performance metrics
    /// </summary>
    private PerformanceMetrics CalculatePortfolioMetrics(
        List<EquityCurvePoint> equityCurve,
        decimal initialCapital,
        DateTime startDate,
        DateTime endDate)
    {
        if (!equityCurve.Any())
        {
            return new PerformanceMetrics();
        }

        var finalEquity = equityCurve.Last().Equity;
        var totalReturn = ((finalEquity - initialCapital) / initialCapital) * 100;

        var days = (endDate - startDate).Days;
        var years = days / 365.25m;
        var annualizedReturn = years > 0
            ? ((decimal)Math.Pow((double)(finalEquity / initialCapital), (double)(1 / years)) - 1) * 100
            : 0m;

        // Calculate Sharpe ratio from equity curve
        var returns = new List<decimal>();
        for (int i = 1; i < equityCurve.Count; i++)
        {
            // Skip if previous equity is zero to avoid division by zero
            if (equityCurve[i - 1].Equity == 0)
                continue;

            var dailyReturn = (equityCurve[i].Equity - equityCurve[i - 1].Equity) / equityCurve[i - 1].Equity;
            returns.Add(dailyReturn);
        }

        var avgReturn = returns.Any() ? returns.Average() : 0m;
        var stdDev = returns.Count > 1
            ? (decimal)Math.Sqrt((double)returns.Sum(r => (r - avgReturn) * (r - avgReturn)) / (returns.Count - 1))
            : 0m;

        var sharpeRatio = stdDev > 0
            ? (avgReturn - 0.02m / 252) / stdDev * (decimal)Math.Sqrt(252)
            : 0m;

        var maxDrawdown = equityCurve.Max(e => e.Drawdown);
        var maxDrawdownPercent = equityCurve.Max(e => e.DrawdownPercent);

        return new PerformanceMetrics
        {
            TotalReturn = totalReturn,
            AnnualizedReturn = annualizedReturn,
            SharpeRatio = sharpeRatio,
            MaxDrawdown = maxDrawdown,
            MaxDrawdownPercent = maxDrawdownPercent,
            TotalTrades = 0, // Portfolio doesn't have individual trades
            WinningTrades = 0,
            LosingTrades = 0,
            WinRate = 0,
            ProfitFactor = 0,
            AverageWin = 0,
            AverageLoss = 0,
            LargestWin = 0,
            LargestLoss = 0
        };
    }

    /// <summary>
    /// Calculate returns from equity curve
    /// </summary>
    private List<decimal> CalculateReturns(List<EquityCurvePoint> equityCurve)
    {
        var returns = new List<decimal>();

        for (int i = 1; i < equityCurve.Count; i++)
        {
            var dailyReturn = (equityCurve[i].Equity - equityCurve[i - 1].Equity) / equityCurve[i - 1].Equity;
            returns.Add(dailyReturn);
        }

        return returns;
    }

    /// <summary>
    /// Calculate correlation between two return series
    /// </summary>
    private decimal CalculateCorrelation(List<decimal> returns1, List<decimal> returns2)
    {
        if (returns1.Count != returns2.Count || returns1.Count < 2)
            return 0m;

        var avg1 = returns1.Average();
        var avg2 = returns2.Average();

        decimal numerator = 0m;
        decimal sumSq1 = 0m;
        decimal sumSq2 = 0m;

        for (int i = 0; i < returns1.Count; i++)
        {
            var diff1 = returns1[i] - avg1;
            var diff2 = returns2[i] - avg2;

            numerator += diff1 * diff2;
            sumSq1 += diff1 * diff1;
            sumSq2 += diff2 * diff2;
        }

        var denominator = (decimal)Math.Sqrt((double)(sumSq1 * sumSq2));

        return denominator > 0 ? numerator / denominator : 0m;
    }
}
