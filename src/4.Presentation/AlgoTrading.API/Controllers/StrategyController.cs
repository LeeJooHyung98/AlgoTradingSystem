using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTrading.API.Controllers;

/// <summary>
/// Strategy API Controller
/// Provides endpoints for managing trading strategies
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StrategyController : ControllerBase
{
    private readonly ILogger<StrategyController> _logger;
    private readonly IStrategyService _strategyService;
    private readonly ISignalGenerationService _signalGenerationService;
    private readonly IEnumerable<ITradingStrategy> _tradingStrategies;

    public StrategyController(
        ILogger<StrategyController> logger,
        IStrategyService strategyService,
        ISignalGenerationService signalGenerationService,
        IEnumerable<ITradingStrategy> tradingStrategies)
    {
        _logger = logger;
        _strategyService = strategyService;
        _signalGenerationService = signalGenerationService;
        _tradingStrategies = tradingStrategies;
    }

    /// <summary>
    /// Get all available trading strategies
    /// </summary>
    /// <returns>List of available strategy types</returns>
    [HttpGet("available")]
    [ProducesResponseType(typeof(List<StrategyInfoDto>), StatusCodes.Status200OK)]
    public IActionResult GetAvailableStrategies()
    {
        _logger.LogInformation("Getting available trading strategies");

        var strategies = _tradingStrategies.Select(s => new StrategyInfoDto
        {
            Name = s.Name,
            Description = s.Description,
            StrategyType = s.StrategyType.ToString(),
            DefaultParameters = s.GetDefaultParameters()
        }).ToList();

        return Ok(strategies);
    }

    /// <summary>
    /// Create a new trading strategy
    /// </summary>
    /// <param name="request">Strategy creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created strategy</returns>
    [HttpPost]
    [ProducesResponseType(typeof(StrategyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStrategy(
        [FromBody] CreateStrategyRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating strategy: {Name}", request.Name);

        var result = await _strategyService.CreateStrategyAsync(
            request.Name,
            request.Description,
            request.StrategyType,
            request.CreatedBy,
            cancellationToken);

        if (result.IsError)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Strategy Creation Failed",
                Detail = result.FirstError.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        var strategy = result.Value;
        var dto = new StrategyDto
        {
            Id = strategy.Id.Value,
            Name = strategy.Name,
            Description = strategy.Description,
            StrategyType = strategy.Type.ToString(),
            Status = strategy.Status.ToString(),
            CreatedAt = strategy.CreatedAt
        };

        return CreatedAtAction(nameof(GetStrategyById), new { id = strategy.Id.Value }, dto);
    }

    /// <summary>
    /// Get strategy by ID
    /// </summary>
    /// <param name="id">Strategy ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Strategy details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StrategyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStrategyById(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting strategy {StrategyId}", id);

        var result = await _strategyService.GetStrategyByIdAsync(id, cancellationToken);

        if (result.IsError)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Strategy Not Found",
                Detail = result.FirstError.Description,
                Status = StatusCodes.Status404NotFound
            });
        }

        var strategy = result.Value;
        var dto = new StrategyDto
        {
            Id = strategy.Id.Value,
            Name = strategy.Name,
            Description = strategy.Description,
            StrategyType = strategy.Type.ToString(),
            Status = strategy.Status.ToString(),
            CreatedAt = strategy.CreatedAt,
            RuleCount = strategy.Rules.Count
        };

        return Ok(dto);
    }

    /// <summary>
    /// Activate a strategy
    /// </summary>
    /// <param name="id">Strategy ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activated strategy</returns>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(StrategyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateStrategy(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating strategy {StrategyId}", id);

        var result = await _strategyService.ActivateStrategyAsync(id, cancellationToken);

        if (result.IsError)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Strategy Activation Failed",
                Detail = result.FirstError.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        var strategy = result.Value;
        var dto = new StrategyDto
        {
            Id = strategy.Id.Value,
            Name = strategy.Name,
            Description = strategy.Description,
            StrategyType = strategy.Type.ToString(),
            Status = strategy.Status.ToString(),
            CreatedAt = strategy.CreatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Deactivate a strategy
    /// </summary>
    /// <param name="id">Strategy ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deactivated strategy</returns>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(StrategyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateStrategy(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating strategy {StrategyId}", id);

        var result = await _strategyService.DeactivateStrategyAsync(id, cancellationToken);

        if (result.IsError)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Strategy Deactivation Failed",
                Detail = result.FirstError.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        var strategy = result.Value;
        var dto = new StrategyDto
        {
            Id = strategy.Id.Value,
            Name = strategy.Name,
            Description = strategy.Description,
            StrategyType = strategy.Type.ToString(),
            Status = strategy.Status.ToString(),
            CreatedAt = strategy.CreatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// Generate trading signal for a strategy
    /// </summary>
    /// <param name="id">Strategy ID</param>
    /// <param name="request">Signal generation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated signal</returns>
    [HttpPost("{id:guid}/signal")]
    [ProducesResponseType(typeof(SignalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateSignal(
        Guid id,
        [FromBody] GenerateSignalRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating signal for strategy {StrategyId}, stock {StockCode}",
            id, request.StockCode);

        var result = await _signalGenerationService.GenerateSignalAsync(
            id,
            request.StockCode,
            cancellationToken);

        if (result.IsError)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Signal Generation Failed",
                Detail = result.FirstError.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        var signal = result.Value;
        var dto = new SignalDto
        {
            HasSignal = true,
            SignalType = signal.Type.ToString(),
            SignalStrength = signal.Strength.ToString(),
            Price = signal.TargetPrice ?? 0,
            Reason = $"Signal generated for {request.StockCode}",
            GeneratedAt = signal.GeneratedAt,
            StopLoss = signal.StopLoss,
            TakeProfit = signal.TakeProfit
        };

        return Ok(dto);
    }
}

/// <summary>
/// Strategy information DTO
/// </summary>
public class StrategyInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StrategyType { get; set; } = string.Empty;
    public Dictionary<string, object> DefaultParameters { get; set; } = new();
}

/// <summary>
/// Strategy DTO
/// </summary>
public class StrategyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StrategyType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int RuleCount { get; set; }
}

/// <summary>
/// Create strategy request
/// </summary>
public class CreateStrategyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StrategyType StrategyType { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Generate signal request
/// </summary>
public class GenerateSignalRequest
{
    public string StockCode { get; set; } = string.Empty;
}

/// <summary>
/// Signal DTO
/// </summary>
public class SignalDto
{
    public bool HasSignal { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public string SignalStrength { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
}
