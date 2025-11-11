using AlgoTrading.Application.Queries.Trading;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTrading.API.Controllers;

/// <summary>
/// description($�) : Trades API Controller (p� API �d�)
/// Details(�8$�) : RESTful API endpoints for trade history (p� t%D \ RESTful API ���x�)
/// Applied technology patterns(�0 (4) : CQRS Pattern, RESTful API, MediatR (CQRS (4, RESTful API, MediatR)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TradesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TradesController> _logger;

    public TradesController(IMediator mediator, ILogger<TradesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get trade history (p� t% p�)
    /// </summary>
    /// <param name="accountNumber">Account number</param>
    /// <param name="startDate">Start date (optional)</param>
    /// <param name="endDate">End date (optional)</param>
    /// <param name="stockCode">Stock code filter (optional)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of trades</returns>
    [HttpGet("account/{accountNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTradeHistory(
        [FromRoute] string accountNumber,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? stockCode = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting trade history for account {AccountNumber}, page {PageNumber}",
            accountNumber,
            pageNumber);

        var query = new GetTradeHistoryQuery
        {
            AccountNumber = accountNumber,
            StartDate = startDate,
            EndDate = endDate,
            StockCode = stockCode,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            trades => Ok(trades),
            errors => BadRequest(errors));
    }

    /// <summary>
    /// Get trade performance metrics (p� 1� �\ p�)
    /// </summary>
    /// <param name="accountNumber">Account number</param>
    /// <param name="startDate">Start date (optional)</param>
    /// <param name="endDate">End date (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trade performance metrics</returns>
    [HttpGet("account/{accountNumber}/metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTradeMetrics(
        [FromRoute] string accountNumber,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting trade metrics for account {AccountNumber}",
            accountNumber);

        var query = new GetTradeHistoryQuery
        {
            AccountNumber = accountNumber,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            trades =>
            {
                var winningTrades = trades.Where(t => t.IsWinner).ToList();
                var losingTrades = trades.Where(t => t.IsLoser).ToList();

                var metrics = new
                {
                    TotalTrades = trades.Count(),
                    WinningTrades = winningTrades.Count,
                    LosingTrades = losingTrades.Count,
                    WinRate = trades.Count() > 0
                        ? (decimal)winningTrades.Count / trades.Count() * 100
                        : 0,
                    TotalProfit = winningTrades.Sum(t => t.RealizedPL),
                    TotalLoss = losingTrades.Sum(t => t.RealizedPL),
                    NetProfit = trades.Sum(t => t.RealizedPL),
                    AverageProfit = winningTrades.Any()
                        ? winningTrades.Average(t => t.RealizedPL)
                        : 0,
                    AverageLoss = losingTrades.Any()
                        ? losingTrades.Average(t => t.RealizedPL)
                        : 0,
                    ProfitFactor = losingTrades.Sum(t => Math.Abs(t.RealizedPL)) > 0
                        ? winningTrades.Sum(t => t.RealizedPL) / losingTrades.Sum(t => Math.Abs(t.RealizedPL))
                        : 0,
                    AverageDurationMinutes = trades.Any()
                        ? trades.Average(t => (t.ExitTime - t.EntryTime).TotalMinutes)
                        : 0
                };

                return Ok(metrics);
            },
            errors => BadRequest(errors));
    }

    /// <summary>
    /// Get trades by stock (��� p� t% p�)
    /// </summary>
    /// <param name="stockCode">Stock code</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of trades for specific stock</returns>
    [HttpGet("stock/{stockCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTradesByStock(
        [FromRoute] string stockCode,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting trades for stock {StockCode}, page {PageNumber}",
            stockCode,
            pageNumber);

        var query = new GetTradeHistoryQuery
        {
            AccountNumber = string.Empty, // TODO: Get from authentication context or make AccountNumber optional
            StockCode = stockCode,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            trades => Ok(trades),
            errors => BadRequest(errors));
    }
}
