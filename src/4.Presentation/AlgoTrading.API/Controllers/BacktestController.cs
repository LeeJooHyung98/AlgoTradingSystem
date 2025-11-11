using AlgoTrading.Application.Services.Strategy;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTrading.API.Controllers;

/// <summary>
/// Backtest API Controller
/// Provides endpoints for running backtests and retrieving results
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BacktestController : ControllerBase
{
    private readonly ILogger<BacktestController> _logger;
    private readonly IBacktestService _backtestService;

    public BacktestController(
        ILogger<BacktestController> _logger,
        IBacktestService backtestService)
    {
        this._logger = _logger;
        _backtestService = backtestService;
    }

    /// <summary>
    /// Run a backtest for a strategy
    /// </summary>
    /// <param name="request">Backtest request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Backtest result with performance metrics</returns>
    [HttpPost("run")]
    [ProducesResponseType(typeof(BacktestResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunBacktest(
        [FromBody] RunBacktestRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running backtest for strategy {StrategyId}", request.StrategyId);

        var result = await _backtestService.RunBacktestAsync(
            request.StrategyId,
            request.StartDate,
            request.EndDate,
            request.InitialCapital,
            cancellationToken);

        if (result.IsError)
        {
            var error = result.FirstError;
            return error.Type switch
            {
                ErrorOr.ErrorType.NotFound => NotFound(new ProblemDetails
                {
                    Title = "Strategy Not Found",
                    Detail = error.Description,
                    Status = StatusCodes.Status404NotFound
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Title = "Backtest Failed",
                    Detail = error.Description,
                    Status = StatusCodes.Status400BadRequest
                })
            };
        }

        var backtestResult = result.Value;
        var dto = new BacktestResultDto
        {
            BacktestId = backtestResult.BacktestId,
            StrategyId = backtestResult.StrategyId,
            StartDate = backtestResult.StartDate,
            EndDate = backtestResult.EndDate,
            InitialCapital = backtestResult.InitialCapital,
            FinalCapital = backtestResult.FinalCapital,
            Metrics = MapMetrics(backtestResult.Metrics),
            TradeCount = backtestResult.Trades.Count,
            EquityCurvePoints = backtestResult.EquityCurve.Count
        };

        return Ok(dto);
    }

    /// <summary>
    /// Get backtest trades
    /// </summary>
    /// <param name="backtestId">Backtest ID</param>
    /// <returns>List of trades</returns>
    [HttpGet("{backtestId:guid}/trades")]
    [ProducesResponseType(typeof(List<TradeDto>), StatusCodes.Status200OK)]
    public IActionResult GetBacktestTrades(Guid backtestId)
    {
        // TODO: Implement trade retrieval from storage
        return Ok(new List<TradeDto>());
    }

    /// <summary>
    /// Get backtest equity curve
    /// </summary>
    /// <param name="backtestId">Backtest ID</param>
    /// <returns>Equity curve data points</returns>
    [HttpGet("{backtestId:guid}/equity-curve")]
    [ProducesResponseType(typeof(List<EquityCurvePointDto>), StatusCodes.Status200OK)]
    public IActionResult GetEquityCurve(Guid backtestId)
    {
        // TODO: Implement equity curve retrieval from storage
        return Ok(new List<EquityCurvePointDto>());
    }

    /// <summary>
    /// Get backtest performance metrics
    /// </summary>
    /// <param name="backtestId">Backtest ID</param>
    /// <returns>Performance metrics</returns>
    [HttpGet("{backtestId:guid}/metrics")]
    [ProducesResponseType(typeof(PerformanceMetricsDto), StatusCodes.Status200OK)]
    public IActionResult GetPerformanceMetrics(Guid backtestId)
    {
        // TODO: Implement metrics retrieval from storage
        return Ok(new PerformanceMetricsDto());
    }

    private PerformanceMetricsDto MapMetrics(PerformanceMetrics metrics)
    {
        return new PerformanceMetricsDto
        {
            TotalReturn = metrics.TotalReturn,
            AnnualizedReturn = metrics.AnnualizedReturn,
            SharpeRatio = metrics.SharpeRatio,
            MaxDrawdown = metrics.MaxDrawdown,
            MaxDrawdownPercent = metrics.MaxDrawdownPercent,
            TotalTrades = metrics.TotalTrades,
            WinningTrades = metrics.WinningTrades,
            LosingTrades = metrics.LosingTrades,
            WinRate = metrics.WinRate,
            ProfitFactor = metrics.ProfitFactor,
            AverageWin = metrics.AverageWin,
            AverageLoss = metrics.AverageLoss,
            LargestWin = metrics.LargestWin,
            LargestLoss = metrics.LargestLoss
        };
    }
}

/// <summary>
/// Run backtest request
/// </summary>
public class RunBacktestRequest
{
    /// <summary>
    /// Strategy ID to backtest
    /// </summary>
    public Guid StrategyId { get; set; }

    /// <summary>
    /// Backtest start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Backtest end date
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Initial capital amount
    /// </summary>
    public decimal InitialCapital { get; set; } = 10000000m; // 10M KRW default
}

/// <summary>
/// Backtest result DTO
/// </summary>
public class BacktestResultDto
{
    public Guid BacktestId { get; set; }
    public Guid StrategyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal InitialCapital { get; set; }
    public decimal FinalCapital { get; set; }
    public PerformanceMetricsDto Metrics { get; set; } = null!;
    public int TradeCount { get; set; }
    public int EquityCurvePoints { get; set; }
}

/// <summary>
/// Performance metrics DTO
/// </summary>
public class PerformanceMetricsDto
{
    public decimal TotalReturn { get; set; }
    public decimal AnnualizedReturn { get; set; }
    public decimal SharpeRatio { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal MaxDrawdownPercent { get; set; }
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public decimal WinRate { get; set; }
    public decimal ProfitFactor { get; set; }
    public decimal AverageWin { get; set; }
    public decimal AverageLoss { get; set; }
    public decimal LargestWin { get; set; }
    public decimal LargestLoss { get; set; }
}

/// <summary>
/// Trade DTO
/// </summary>
public class TradeDto
{
    public string StockCode { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }
    public DateTime ExitTime { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal RealizedPL { get; set; }
    public decimal RealizedPLPercent { get; set; }
}

/// <summary>
/// Equity curve point DTO
/// </summary>
public class EquityCurvePointDto
{
    public DateTime Time { get; set; }
    public decimal Equity { get; set; }
    public decimal Drawdown { get; set; }
    public decimal DrawdownPercent { get; set; }
}
