using AlgoTrading.Application.Services.Trading;
using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Core.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using IUnitOfWork = AlgoTrading.Core.Interfaces.IUnitOfWork;

namespace AlgoTrading.API.Controllers;

/// <summary>
/// Order API Controller
/// Provides endpoints for order management and execution
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderController(
        ILogger<OrderController> logger,
        IOrderService orderService,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _orderService = orderService;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Create market order
    /// </summary>
    /// <param name="request">Market order request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order</returns>
    [HttpPost("market")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMarketOrder(
        [FromBody] CreateMarketOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating market order: {StockCode} {Side} {Quantity}",
                request.StockCode, request.Side, request.Quantity);

            // Create order entity
            var order = Order.CreateMarketOrder(
                new StockCode(request.StockCode),
                request.Side,
                request.Quantity,
                request.AccountNumber,
                OrderSource.API,
                request.StrategyId);

            // Save order
            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Submit order
            var submitResult = await _orderService.SubmitOrderAsync(order.Id.Value, cancellationToken);

            if (submitResult.IsError)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Order Submission Failed",
                    Detail = submitResult.FirstError.Description,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var dto = MapToDto(order);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id.Value }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create market order");
            return BadRequest(new ProblemDetails
            {
                Title = "Order Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Create limit order
    /// </summary>
    /// <param name="request">Limit order request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order</returns>
    [HttpPost("limit")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLimitOrder(
        [FromBody] CreateLimitOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating limit order: {StockCode} {Side} {Quantity} @ {Price}",
                request.StockCode, request.Side, request.Quantity, request.LimitPrice);

            // Create order entity
            var order = Order.CreateLimitOrder(
                new StockCode(request.StockCode),
                request.Side,
                request.Quantity,
                request.LimitPrice,
                request.AccountNumber,
                OrderSource.API,
                request.StrategyId);

            // Save order
            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Submit order
            var submitResult = await _orderService.SubmitOrderAsync(order.Id.Value, cancellationToken);

            if (submitResult.IsError)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Order Submission Failed",
                    Detail = submitResult.FirstError.Description,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var dto = MapToDto(order);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id.Value }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create limit order");
            return BadRequest(new ProblemDetails
            {
                Title = "Order Creation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting order {OrderId}", id);

        var order = await _orderRepository.GetByIdAsync(new OrderId(id), cancellationToken);

        if (order == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Order Not Found",
                Detail = $"Order with ID {id} not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        var dto = MapToDto(order);
        return Ok(dto);
    }

    /// <summary>
    /// Get orders by account number
    /// </summary>
    /// <param name="accountNumber">Account number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of orders</returns>
    [HttpGet("account/{accountNumber}")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByAccount(
        string accountNumber,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting orders for account {AccountNumber}", accountNumber);

        var orders = await _orderRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);

        var dtos = orders.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Cancel order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Cancellation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order</returns>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Cancelling order {OrderId}", id);

            var order = await _orderRepository.GetByIdAsync(new OrderId(id), cancellationToken);

            if (order == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Order Not Found",
                    Detail = $"Order with ID {id} not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            order.Cancel(request.Reason ?? "Cancelled by user");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(order);
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel order {OrderId}", id);
            return BadRequest(new ProblemDetails
            {
                Title = "Order Cancellation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Get order statistics for account
    /// </summary>
    /// <param name="accountNumber">Account number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order statistics</returns>
    [HttpGet("statistics/{accountNumber}")]
    [ProducesResponseType(typeof(OrderStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderStatistics(
        string accountNumber,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting order statistics for account {AccountNumber}", accountNumber);

        var result = await _orderService.GetOrderStatisticsAsync(accountNumber, cancellationToken: cancellationToken);

        if (result.IsError)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Failed to Get Statistics",
                Detail = result.FirstError.Description,
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Map Order entity to DTO
    /// </summary>
    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id.Value,
            StockCode = order.StockCode.Value,
            Side = order.Side.ToString(),
            Type = order.Type.ToString(),
            Quantity = order.Quantity,
            FilledQuantity = order.FilledQuantity,
            RemainingQuantity = order.RemainingQuantity,
            LimitPrice = order.LimitPrice,
            StopPrice = order.StopPrice,
            AverageFillPrice = order.AverageFillPrice,
            Status = order.Status.ToString(),
            AccountNumber = order.AccountNumber,
            Source = order.Source.ToString(),
            StrategyId = order.StrategyId,
            SubmittedAt = order.SubmittedAt,
            AcceptedAt = order.AcceptedAt,
            CompletedAt = order.CompletedAt,
            CancelledAt = order.CancelledAt,
            BrokerOrderId = order.BrokerOrderId,
            CancellationReason = order.CancellationReason,
            RejectionReason = order.RejectionReason
        };
    }
}

/// <summary>
/// Create market order request
/// </summary>
public class CreateMarketOrderRequest
{
    public string StockCode { get; set; } = string.Empty;
    public OrderSide Side { get; set; }
    public int Quantity { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public Guid? StrategyId { get; set; }
}

/// <summary>
/// Create limit order request
/// </summary>
public class CreateLimitOrderRequest
{
    public string StockCode { get; set; } = string.Empty;
    public OrderSide Side { get; set; }
    public int Quantity { get; set; }
    public decimal LimitPrice { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public Guid? StrategyId { get; set; }
}

/// <summary>
/// Cancel order request
/// </summary>
public class CancelOrderRequest
{
    public string? Reason { get; set; }
}

/// <summary>
/// Order DTO
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int FilledQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public decimal? AverageFillPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Guid? StrategyId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? BrokerOrderId { get; set; }
    public string? CancellationReason { get; set; }
    public string? RejectionReason { get; set; }
}

/// <summary>
/// Order statistics DTO
/// </summary>
public class OrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int AcceptedOrders { get; set; }
    public int FilledOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int RejectedOrders { get; set; }
    public decimal TotalOrderValue { get; set; }
    public decimal AverageFillRate { get; set; }
}
