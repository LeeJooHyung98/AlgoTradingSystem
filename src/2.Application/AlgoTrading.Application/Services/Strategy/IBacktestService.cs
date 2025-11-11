using ErrorOr;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// description($Ö) : Backtest Service Interface (1L§∏ D§ x0òt§)
/// Details(¡8$Ö) : Interface for executing backtests on trading strategies (∏t) µX 1L§∏ ‰âD \ x0òt§)
/// Applied technology patterns(©0 (4) : Service Pattern, Strategy Pattern, DDD Service (D§ (4, µ (4, DDD D§)
/// </summary>
public interface IBacktestService
{
    /// <summary>
    /// Run backtest for a trading strategy (∏t) µ 1L§∏ ‰â)
    /// </summary>
    /// <param name="strategyId">Strategy ID</param>
    /// <param name="startDate">Backtest start date</param>
    /// <param name="endDate">Backtest end date</param>
    /// <param name="initialCapital">Initial capital amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Backtest result with performance metrics</returns>
    Task<ErrorOr<BacktestResult>> RunBacktestAsync(
        Guid strategyId,
        DateTime startDate,
        DateTime endDate,
        decimal initialCapital,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Run walk-forward backtest (ÃlÏÃ‹ 1L§∏ ‰â)
    /// </summary>
    /// <param name="strategyId">Strategy ID</param>
    /// <param name="startDate">Backtest start date</param>
    /// <param name="endDate">Backtest end date</param>
    /// <param name="initialCapital">Initial capital amount</param>
    /// <param name="inSamplePeriodDays">In-sample period in days</param>
    /// <param name="outOfSamplePeriodDays">Out-of-sample period in days</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Walk-forward backtest result</returns>
    Task<ErrorOr<WalkForwardBacktestResult>> RunWalkForwardBacktestAsync(
        Guid strategyId,
        DateTime startDate,
        DateTime endDate,
        decimal initialCapital,
        int inSamplePeriodDays,
        int outOfSamplePeriodDays,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate performance metrics from trades (pò ¥Ì<\Ä0 1¸ ¿\ ƒ∞)
    /// </summary>
    /// <param name="trades">List of completed trades</param>
    /// <param name="initialCapital">Initial capital amount</param>
    /// <returns>Performance metrics</returns>
    PerformanceMetrics CalculatePerformanceMetrics(
        IEnumerable<BacktestTrade> trades,
        decimal initialCapital);
}

/// <summary>
/// Backtest result (1L§∏ ∞¸)
/// </summary>
public sealed record BacktestResult
{
    public Guid BacktestId { get; init; }
    public Guid StrategyId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal InitialCapital { get; init; }
    public decimal FinalCapital { get; init; }
    public PerformanceMetrics Metrics { get; init; } = null!;
    public List<BacktestTrade> Trades { get; init; } = new();
    public List<EquityCurvePoint> EquityCurve { get; init; } = new();
}

/// <summary>
/// Walk-forward backtest result (ÃlÏÃ‹ 1L§∏ ∞¸)
/// </summary>
public sealed record WalkForwardBacktestResult
{
    public Guid BacktestId { get; init; }
    public Guid StrategyId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal InitialCapital { get; init; }
    public decimal FinalCapital { get; init; }
    public PerformanceMetrics OverallMetrics { get; init; } = null!;
    public List<WalkForwardPeriodResult> PeriodResults { get; init; } = new();
}

/// <summary>
/// Walk-forward period result (ÃlÏÃ‹ 0 ∞¸)
/// </summary>
public sealed record WalkForwardPeriodResult
{
    public DateTime InSampleStart { get; init; }
    public DateTime InSampleEnd { get; init; }
    public DateTime OutOfSampleStart { get; init; }
    public DateTime OutOfSampleEnd { get; init; }
    public PerformanceMetrics InSampleMetrics { get; init; } = null!;
    public PerformanceMetrics OutOfSampleMetrics { get; init; } = null!;
}

/// <summary>
/// Performance metrics (1¸ ¿\)
/// </summary>
public sealed record PerformanceMetrics
{
    public decimal TotalReturn { get; init; }
    public decimal AnnualizedReturn { get; init; }
    public decimal SharpeRatio { get; init; }
    public decimal MaxDrawdown { get; init; }
    public decimal MaxDrawdownPercent { get; init; }
    public int TotalTrades { get; init; }
    public int WinningTrades { get; init; }
    public int LosingTrades { get; init; }
    public decimal WinRate { get; init; }
    public decimal ProfitFactor { get; init; }
    public decimal AverageWin { get; init; }
    public decimal AverageLoss { get; init; }
    public decimal LargestWin { get; init; }
    public decimal LargestLoss { get; init; }
}

/// <summary>
/// Backtest trade (1L§∏ pò)
/// </summary>
public sealed record BacktestTrade
{
    public DateTime EntryTime { get; init; }
    public DateTime ExitTime { get; init; }
    public decimal EntryPrice { get; init; }
    public decimal ExitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal RealizedPL { get; init; }
    public decimal RealizedPLPercent { get; init; }
    public string StockCode { get; init; } = string.Empty;
}

/// <summary>
/// Equity curve point (ê∞ ·  Ïx∏)
/// </summary>
public sealed record EquityCurvePoint
{
    public DateTime Time { get; init; }
    public decimal Equity { get; init; }
    public decimal Drawdown { get; init; }
    public decimal DrawdownPercent { get; init; }
}
