using AlgoTrading.Application.DTOs.Trading;
using AlgoTrading.Application.Mappers;
using AlgoTrading.Application.Queries.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Application.Queries.Handlers;

/// <summary>
/// description(설명) : Get Active Orders Query Handler (활성 주문 조회 쿼리 핸들러)
/// Details(상세설명) : Retrieves all active orders for an account with optional filters (계좌의 모든 활성 주문을 필터와 함께 조회)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, Handler Pattern, Repository Pattern (CQRS 패턴, 핸들러 패턴, Repository 패턴)
/// </summary>
public sealed class GetActiveOrdersQueryHandler : IRequestHandler<GetActiveOrdersQuery, ErrorOr<IEnumerable<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetActiveOrdersQueryHandler> _logger;

    public GetActiveOrdersQueryHandler(
        IOrderRepository orderRepository,
        ILogger<GetActiveOrdersQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<ErrorOr<IEnumerable<OrderDto>>> Handle(GetActiveOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Retrieving active orders: AccountNumber={AccountNumber}, StockCode={StockCode}, StrategyId={StrategyId}",
                request.AccountNumber, request.StockCode, request.StrategyId);

            // Get orders by account number
            var orders = await _orderRepository.GetByAccountNumberAsync(request.AccountNumber, cancellationToken);

            // Filter for active orders only
            var activeOrders = orders.Where(o => o.IsActive());

            // Apply stock code filter if provided
            if (!string.IsNullOrEmpty(request.StockCode))
            {
                activeOrders = activeOrders.Where(o => o.StockCode.Value == request.StockCode);
            }

            // Apply strategy ID filter if provided
            if (request.StrategyId.HasValue)
            {
                activeOrders = activeOrders.Where(o => o.StrategyId == request.StrategyId.Value);
            }

            var orderDtos = activeOrders
                .Select(o => o.ToDto())
                .OrderByDescending(o => o.SubmittedAt)
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} active orders for account {AccountNumber}",
                orderDtos.Count, request.AccountNumber);

            return orderDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active orders: AccountNumber={AccountNumber}", request.AccountNumber);
            return Error.Failure("Orders.RetrievalFailed", "An error occurred while retrieving active orders");
        }
    }
}
