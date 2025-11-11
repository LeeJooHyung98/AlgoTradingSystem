using AlgoTrading.Application.Commands.Trading;
using AlgoTrading.Application.Queries.Trading;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlgoTrading.API.Controllers;

/// <summary>
/// description(설명) : Orders API Controller (주문 API 컨트롤러)
/// Details(상세설명) : RESTful API endpoints for order management (주문 관리를 위한 RESTful API 엔드포인트)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, RESTful API, MediatR (CQRS 패턴, RESTful API, MediatR)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new order (새로운 주문 생성)
    /// </summary>
    /// <param name="command">Order creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order ID of the created order</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating order for stock {StockCode}", command.StockCode);

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            orderId => CreatedAtAction(nameof(GetOrder), new { id = orderId }, orderId),
            errors => BadRequest(errors));
    }

    /// <summary>
    /// Get order by ID (ID로 주문 조회)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrder(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting order {OrderId}", id);

        var query = new GetOrderByIdQuery { OrderId = id };
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            order => order != null ? Ok(order) : NotFound(),
            errors => BadRequest(errors));
    }

    /// <summary>
    /// Get orders by account (계좌별 주문 목록 조회)
    /// </summary>
    /// <param name="accountNumber">Account number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of orders</returns>
    [HttpGet("account/{accountNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrdersByAccount(
        [FromRoute] string accountNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting orders for account {AccountNumber}", accountNumber);

        var query = new GetActiveOrdersQuery { AccountNumber = accountNumber };
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            orders => Ok(orders),
            errors => BadRequest(errors));
    }

    /// <summary>
    /// Cancel an order (주문 취소)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelOrder(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling order {OrderId}", id);

        var command = new CancelOrderCommand
        {
            OrderId = id,
            AccountNumber = string.Empty // TODO: Get from authentication context
        };

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(new { message = "Order cancelled successfully" }),
            errors => BadRequest(errors));
    }

    // TODO: Implement UpdateOrder endpoint when UpdateOrderCommand is created in Application layer
}
