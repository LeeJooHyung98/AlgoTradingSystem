using AlgoTrading.Application.Services.Strategy;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTrading.API.Controllers;

/// <summary>
/// Portfolio Backtest API Controller
/// Provides endpoints for running portfolio backtests with multiple strategies
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PortfolioBacktestController : ControllerBase
{
    private readonly ILogger<PortfolioBacktestController> _logger;
    private readonly IPortfolioBacktestService _portfolioBacktestService;

    public PortfolioBacktestController(
        ILogger<PortfolioBacktestController> logger,
        IPortfolioBacktestService portfolioBacktestService)
    {
        _logger = logger;
        _portfolioBacktestService = portfolioBacktestService;
    }

    /// <summary>
    /// Run a portfolio backtest with multiple strategies
    /// </summary>
    /// <param name="request">Portfolio backtest request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Portfolio backtest result with combined performance metrics</returns>
    [HttpPost("run")]
    [ProducesResponseType(typeof(PortfolioBacktestResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunPortfolioBacktest(
        [FromBody] RunPortfolioBacktestRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Running portfolio backtest for {Count} strategies from {StartDate} to {EndDate}",
            request.StrategyIds.Count, request.StartDate, request.EndDate);

        var result = await _portfolioBacktestService.RunPortfolioBacktestAsync(
            request.StrategyIds,
            request.StartDate,
            request.EndDate,
            request.InitialCapital,
            request.AllocationMethod,
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
                ErrorOr.ErrorType.Validation => BadRequest(new ProblemDetails
                {
                    Title = "Validation Error",
                    Detail = error.Description,
                    Status = StatusCodes.Status400BadRequest
                }),
                _ => BadRequest(new ProblemDetails
                {
                    Title = "Portfolio Backtest Failed",
                    Detail = error.Description,
                    Status = StatusCodes.Status400BadRequest
                })
            };
        }

        var portfolioResult = result.Value;
        var dto = MapToDto(portfolioResult);

        return Ok(dto);
    }

    /// <summary>
    /// Get correlation matrix for multiple strategies
    /// </summary>
    /// <param name="request">Correlation request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Correlation matrix showing relationships between strategies</returns>
    [HttpPost("correlation")]
    [ProducesResponseType(typeof(CorrelationMatrixDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStrategyCorrelation(
        [FromBody] GetCorrelationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Calculating correlation for {Count} strategies from {StartDate} to {EndDate}",
            request.StrategyIds.Count, request.StartDate, request.EndDate);

        var result = await _portfolioBacktestService.GetStrategyCorrelationAsync(
            request.StrategyIds,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        if (result.IsError)
        {
            var error = result.FirstError;
            return BadRequest(new ProblemDetails
            {
                Title = "Correlation Calculation Failed",
                Detail = error.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        var correlationMatrix = result.Value;
        var dto = MapCorrelationToDto(correlationMatrix);

        return Ok(dto);
    }

    /// <summary>
    /// Get allocation suggestions based on strategy performance
    /// </summary>
    /// <param name="portfolioId">Portfolio backtest ID</param>
    /// <returns>Strategy allocation percentages and metrics</returns>
    [HttpGet("{portfolioId:guid}/allocations")]
    [ProducesResponseType(typeof(List<StrategyAllocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetStrategyAllocations(Guid portfolioId)
    {
        // TODO: Implement allocation retrieval from storage
        // For now, return empty list
        _logger.LogInformation("Retrieving allocations for portfolio {PortfolioId}", portfolioId);
        return Ok(new List<StrategyAllocationDto>());
    }

    /// <summary>
    /// Get portfolio equity curve
    /// </summary>
    /// <param name="portfolioId">Portfolio backtest ID</param>
    /// <returns>Portfolio equity curve data points</returns>
    [HttpGet("{portfolioId:guid}/equity-curve")]
    [ProducesResponseType(typeof(List<EquityCurvePointDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public IActionResult GetPortfolioEquityCurve(Guid portfolioId)
    {
        // TODO: Implement equity curve retrieval from storage
        _logger.LogInformation("Retrieving equity curve for portfolio {PortfolioId}", portfolioId);
        return Ok(new List<EquityCurvePointDto>());
    }

    private PortfolioBacktestResultDto MapToDto(PortfolioBacktestResult result)
    {
        return new PortfolioBacktestResultDto
        {
            PortfolioId = result.PortfolioId,
            StartDate = result.StartDate,
            EndDate = result.EndDate,
            InitialCapital = result.InitialCapital,
            FinalCapital = result.FinalCapital,
            TotalReturn = result.PortfolioMetrics.TotalReturn,
            AnnualizedReturn = result.PortfolioMetrics.AnnualizedReturn,
            SharpeRatio = result.PortfolioMetrics.SharpeRatio,
            MaxDrawdown = result.PortfolioMetrics.MaxDrawdown,
            MaxDrawdownPercent = result.PortfolioMetrics.MaxDrawdownPercent,
            StrategyAllocations = result.StrategyAllocations.Select(a => new StrategyAllocationDto
            {
                StrategyId = a.StrategyId,
                StrategyName = a.StrategyName,
                AllocationPercent = a.AllocationPercent,
                AllocatedCapital = a.AllocatedCapital,
                TotalReturn = a.Metrics.TotalReturn,
                SharpeRatio = a.Metrics.SharpeRatio,
                MaxDrawdownPercent = a.Metrics.MaxDrawdownPercent,
                WinRate = a.Metrics.WinRate
            }).ToList(),
            EquityCurvePoints = result.PortfolioEquityCurve.Count,
            StrategyCount = result.StrategyResults.Count
        };
    }

    private CorrelationMatrixDto MapCorrelationToDto(CorrelationMatrix matrix)
    {
        var correlations = new Dictionary<string, Dictionary<string, decimal>>();

        foreach (var strategy1 in matrix.StrategyIds)
        {
            var row = new Dictionary<string, decimal>();
            foreach (var strategy2 in matrix.StrategyIds)
            {
                row[strategy2.ToString()] = matrix.GetCorrelation(strategy1, strategy2);
            }
            correlations[strategy1.ToString()] = row;
        }

        return new CorrelationMatrixDto
        {
            StrategyIds = matrix.StrategyIds,
            Correlations = correlations,
            AverageCorrelation = CalculateAverageCorrelation(matrix)
        };
    }

    private decimal CalculateAverageCorrelation(CorrelationMatrix matrix)
    {
        var correlationValues = new List<decimal>();

        for (int i = 0; i < matrix.StrategyIds.Count; i++)
        {
            for (int j = i + 1; j < matrix.StrategyIds.Count; j++)
            {
                var correlation = matrix.GetCorrelation(
                    matrix.StrategyIds[i],
                    matrix.StrategyIds[j]);
                correlationValues.Add(correlation);
            }
        }

        return correlationValues.Any() ? correlationValues.Average() : 0m;
    }
}

/// <summary>
/// Run portfolio backtest request
/// </summary>
public class RunPortfolioBacktestRequest
{
    /// <summary>
    /// List of strategy IDs to include in portfolio
    /// </summary>
    public List<Guid> StrategyIds { get; set; } = new();

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

    /// <summary>
    /// Portfolio allocation method
    /// </summary>
    public PortfolioAllocationMethod AllocationMethod { get; set; } = PortfolioAllocationMethod.EqualWeight;
}

/// <summary>
/// Get correlation request
/// </summary>
public class GetCorrelationRequest
{
    /// <summary>
    /// List of strategy IDs to analyze
    /// </summary>
    public List<Guid> StrategyIds { get; set; } = new();

    /// <summary>
    /// Analysis start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Analysis end date
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Portfolio backtest result DTO
/// </summary>
public class PortfolioBacktestResultDto
{
    public Guid PortfolioId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal InitialCapital { get; set; }
    public decimal FinalCapital { get; set; }
    public decimal TotalReturn { get; set; }
    public decimal AnnualizedReturn { get; set; }
    public decimal SharpeRatio { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal MaxDrawdownPercent { get; set; }
    public List<StrategyAllocationDto> StrategyAllocations { get; set; } = new();
    public int EquityCurvePoints { get; set; }
    public int StrategyCount { get; set; }
}

/// <summary>
/// Strategy allocation DTO
/// </summary>
public class StrategyAllocationDto
{
    public Guid StrategyId { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public decimal AllocationPercent { get; set; }
    public decimal AllocatedCapital { get; set; }
    public decimal TotalReturn { get; set; }
    public decimal SharpeRatio { get; set; }
    public decimal MaxDrawdownPercent { get; set; }
    public decimal WinRate { get; set; }
}

/// <summary>
/// Correlation matrix DTO
/// </summary>
public class CorrelationMatrixDto
{
    public List<Guid> StrategyIds { get; set; } = new();
    public Dictionary<string, Dictionary<string, decimal>> Correlations { get; set; } = new();
    public decimal AverageCorrelation { get; set; }
}
