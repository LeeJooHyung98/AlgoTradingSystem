using AlgoTrading.Application.Commands.Trading;
using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using IUnitOfWork = AlgoTrading.Core.Interfaces.IUnitOfWork;

namespace AlgoTrading.Application.Commands.Handlers;

/// <summary>
/// description(설명) : Cancel Order Command Handler (주문 취소 명령 핸들러)
/// Details(상세설명) : Handles CancelOrderCommand by canceling an existing order (기존 주문을 취소하여 CancelOrderCommand를 처리)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, Handler Pattern, Repository Pattern, UnitOfWork Pattern (CQRS 패턴, 핸들러 패턴, Repository 패턴, UnitOfWork 패턴)
/// </summary>
public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ErrorOr<bool>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Canceling order: OrderId={OrderId}", request.OrderId);

            // Retrieve order
            var order = await _orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken);

            if (order == null)
            {
                _logger.LogWarning("Order not found: OrderId={OrderId}", request.OrderId);
                return Error.NotFound("Order.NotFound", $"Order with ID {request.OrderId} not found");
            }

            // Verify account number
            if (order.AccountNumber != request.AccountNumber)
            {
                _logger.LogWarning(
                    "Account mismatch: OrderAccountNumber={OrderAccountNumber}, RequestAccountNumber={RequestAccountNumber}",
                    order.AccountNumber, request.AccountNumber);
                return Error.Forbidden("Order.AccountMismatch", "You don't have permission to cancel this order");
            }

            // Cancel order (domain logic)
            order.Cancel(request.Reason);

            // Save changes (EF Core tracks entity changes automatically)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order cancelled successfully: OrderId={OrderId}", request.OrderId);

            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel order: OrderId={OrderId}", request.OrderId);
            return Error.Validation("Order.CannotCancel", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel order: OrderId={OrderId}", request.OrderId);
            return Error.Failure("Order.CancellationFailed", "An error occurred while canceling the order");
        }
    }
}
