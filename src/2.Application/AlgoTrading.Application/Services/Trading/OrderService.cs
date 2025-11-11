using AlgoTrading.Application.DTOs.Trading;
using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using IUnitOfWork = AlgoTrading.Core.Interfaces.IUnitOfWork;

namespace AlgoTrading.Application.Services.Trading;

/// <summary>
/// description(설명) : Order Service Implementation (주문 서비스 구현)
/// Details(상세설명) : Implements business logic for order management (주문 관리 비즈니스 로직 구현)
/// Applied technology patterns(적용기술패턴) : Service Pattern, DDD Application Service (서비스 패턴, DDD 애플리케이션 서비스)
/// </summary>
public sealed class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> SubmitOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(new OrderId(orderId), cancellationToken);

            if (order == null)
            {
                return Error.NotFound("Order.NotFound", $"Order with ID {orderId} not found");
            }

            // Submit order (domain logic)
            order.Submit();

            // TODO: Integrate with broker API to submit order
            // var brokerOrderId = await _brokerService.SubmitOrderAsync(order);
            // order.Accept(brokerOrderId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order submitted successfully: OrderId={OrderId}", orderId);

            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot submit order: OrderId={OrderId}", orderId);
            return Error.Validation("Order.CannotSubmit", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit order: OrderId={OrderId}", orderId);
            return Error.Failure("Order.SubmissionFailed", "An error occurred while submitting the order");
        }
    }

    public async Task<ErrorOr<bool>> ProcessOrderExecutionAsync(
        Guid orderId,
        int filledQuantity,
        decimal fillPrice,
        DateTime fillTime,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(new OrderId(orderId), cancellationToken);

            if (order == null)
            {
                return Error.NotFound("Order.NotFound", $"Order with ID {orderId} not found");
            }

            // Fill order (domain logic)
            order.Fill(filledQuantity, fillPrice, fillTime);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Order execution processed: OrderId={OrderId}, FilledQuantity={FilledQuantity}, FillPrice={FillPrice}",
                orderId, filledQuantity, fillPrice);

            return true;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid order execution: OrderId={OrderId}", orderId);
            return Error.Validation("Order.InvalidExecution", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order execution: OrderId={OrderId}", orderId);
            return Error.Failure("Order.ExecutionProcessingFailed", "An error occurred while processing order execution");
        }
    }

    public async Task<ErrorOr<OrderStatisticsDto>> GetOrderStatisticsAsync(
        string accountNumber,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);

            // Apply date filter
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.SubmittedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.SubmittedAt <= endDate.Value);
            }

            var ordersList = orders.ToList();

            var statistics = new OrderStatisticsDto
            {
                TotalOrders = ordersList.Count,
                PendingOrders = ordersList.Count(o => o.Status == OrderStatus.PendingSubmit || o.Status == OrderStatus.Submitted),
                AcceptedOrders = ordersList.Count(o => o.Status == OrderStatus.Accepted),
                FilledOrders = ordersList.Count(o => o.Status == OrderStatus.Filled),
                CancelledOrders = ordersList.Count(o => o.Status == OrderStatus.Cancelled),
                RejectedOrders = ordersList.Count(o => o.Status == OrderStatus.Rejected),
                TotalOrderValue = ordersList.Sum(o => o.GetEstimatedValue().Amount),
                AverageFillRate = ordersList.Any() ? ordersList.Average(o => o.GetFillRate()) : 0
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get order statistics: AccountNumber={AccountNumber}", accountNumber);
            return Error.Failure("Order.StatisticsFailed", "An error occurred while retrieving order statistics");
        }
    }
}
