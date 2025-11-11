using ErrorOr;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// Interface for Portfolio Backtest Service
/// Runs backtests for multiple strategies simultaneously
/// </summary>
public interface IPortfolioBacktestService
{
    /// <summary>
    /// Run portfolio backtest for multiple strategies
    /// </summary>
    Task<ErrorOr<PortfolioBacktestResult>> RunPortfolioBacktestAsync(
        IEnumerable<Guid> strategyIds,
        DateTime startDate,
        DateTime endDate,
        decimal initialCapital,
        PortfolioAllocationMethod allocationMethod = PortfolioAllocationMethod.EqualWeight,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get portfolio correlation matrix
    /// </summary>
    Task<ErrorOr<CorrelationMatrix>> GetStrategyCorrelationAsync(
        IEnumerable<Guid> strategyIds,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Portfolio allocation method
/// </summary>
public enum PortfolioAllocationMethod
{
    /// <summary>
    /// Equal weight for all strategies
    /// </summary>
    EqualWeight,

    /// <summary>
    /// Weight based on Sharpe ratio
    /// </summary>
    SharpeWeighted,

    /// <summary>
    /// Weight based on return
    /// </summary>
    ReturnWeighted,

    /// <summary>
    /// Risk parity allocation
    /// </summary>
    RiskParity,

    /// <summary>
    /// Custom allocation
    /// </summary>
    Custom
}

/// <summary>
/// Portfolio backtest result
/// </summary>
public class PortfolioBacktestResult
{
    public Guid PortfolioId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal InitialCapital { get; set; }
    public decimal FinalCapital { get; set; }
    public List<StrategyAllocation> StrategyAllocations { get; set; } = new();
    public PerformanceMetrics PortfolioMetrics { get; set; } = null!;
    public List<EquityCurvePoint> PortfolioEquityCurve { get; set; } = new();
    public Dictionary<Guid, BacktestResult> StrategyResults { get; set; } = new();
}

/// <summary>
/// Strategy allocation in portfolio
/// </summary>
public class StrategyAllocation
{
    public Guid StrategyId { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public decimal AllocationPercent { get; set; }
    public decimal AllocatedCapital { get; set; }
    public PerformanceMetrics Metrics { get; set; } = null!;
}

/// <summary>
/// Correlation matrix
/// </summary>
public class CorrelationMatrix
{
    public List<Guid> StrategyIds { get; set; } = new();
    public Dictionary<string, decimal> Correlations { get; set; } = new();

    /// <summary>
    /// Get correlation between two strategies
    /// </summary>
    public decimal GetCorrelation(Guid strategy1, Guid strategy2)
    {
        var key = GetKey(strategy1, strategy2);
        return Correlations.TryGetValue(key, out var correlation) ? correlation : 0m;
    }

    /// <summary>
    /// Set correlation between two strategies
    /// </summary>
    public void SetCorrelation(Guid strategy1, Guid strategy2, decimal correlation)
    {
        var key = GetKey(strategy1, strategy2);
        Correlations[key] = correlation;
    }

    private string GetKey(Guid strategy1, Guid strategy2)
    {
        // Ensure consistent ordering
        var first = strategy1.CompareTo(strategy2) < 0 ? strategy1 : strategy2;
        var second = strategy1.CompareTo(strategy2) < 0 ? strategy2 : strategy1;
        return $"{first}_{second}";
    }
}
