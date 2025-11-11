using AlgoTrading.Application.DTOs.Trading;
using AlgoTrading.Application.Mappers;
using AlgoTrading.Application.Queries.Trading;
using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Application.Queries.Handlers;

/// <summary>
/// description(설명) : Get Order By ID Query Handler (ID로 주문 조회 쿼리 핸들러)
/// Details(상세설명) : Retrieves a single order by ID and maps to DTO (ID로 단일 주문을 조회하고 DTO로 매핑)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, Handler Pattern, Repository Pattern (CQRS 패턴, 핸들러 패턴, Repository 패턴)
/// </summary>
public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ErrorOr<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        ILogger<GetOrderByIdQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<ErrorOr<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving order: OrderId={OrderId}", request.OrderId);

            var order = await _orderRepository.GetByIdAsync(
                new OrderId(request.OrderId),
                cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order not found: OrderId={OrderId}", request.OrderId);
                return Error.NotFound("Order.NotFound", $"Order with ID {request.OrderId} not found");
            }

            // Verify account number if provided
            if (!string.IsNullOrEmpty(request.AccountNumber) && order.AccountNumber != request.AccountNumber)
            {
                _logger.LogWarning(
                    "Account mismatch: OrderAccountNumber={OrderAccountNumber}, RequestAccountNumber={RequestAccountNumber}",
                    order.AccountNumber, request.AccountNumber);
                return Error.Forbidden("Order.AccountMismatch", "You don't have permission to view this order");
            }

            var orderDto = order.ToDto();

            _logger.LogInformation("Order retrieved successfully: OrderId={OrderId}", request.OrderId);

            return orderDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve order: OrderId={OrderId}", request.OrderId);
            return Error.Failure("Order.RetrievalFailed", "An error occurred while retrieving the order");
        }
    }
}
