using AlgoTrading.Application.Commands.Trading;
using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Core.ValueObjects;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using IUnitOfWork = AlgoTrading.Core.Interfaces.IUnitOfWork;

namespace AlgoTrading.Application.Commands.Handlers;

/// <summary>
/// description(설명) : Close Position Command Handler (포지션 청산 명령 핸들러)
/// Details(상세설명) : Handles ClosePositionCommand by creating an exit order for the position (포지션에 대한 청산 주문을 생성하여 ClosePositionCommand를 처리)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, Handler Pattern, Repository Pattern, UnitOfWork Pattern (CQRS 패턴, 핸들러 패턴, Repository 패턴, UnitOfWork 패턴)
/// </summary>
public sealed class ClosePositionCommandHandler : IRequestHandler<ClosePositionCommand, ErrorOr<Guid>>
{
    private readonly IPositionRepository _positionRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClosePositionCommandHandler> _logger;

    public ClosePositionCommandHandler(
        IPositionRepository positionRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<ClosePositionCommandHandler> logger)
    {
        _positionRepository = positionRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(ClosePositionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Closing position: PositionId={PositionId}", request.PositionId);

            // Retrieve position
            var position = await _positionRepository.GetByIdAsync(new PositionId(request.PositionId), cancellationToken);

            if (position == null)
            {
                _logger.LogWarning("Position not found: PositionId={PositionId}", request.PositionId);
                return Error.NotFound("Position.NotFound", $"Position with ID {request.PositionId} not found");
            }

            // Verify account number
            if (position.AccountNumber != request.AccountNumber)
            {
                _logger.LogWarning(
                    "Account mismatch: PositionAccountNumber={PositionAccountNumber}, RequestAccountNumber={RequestAccountNumber}",
                    position.AccountNumber, request.AccountNumber);
                return Error.Forbidden("Position.AccountMismatch", "You don't have permission to close this position");
            }

            // Determine quantity to close
            var quantityToClose = request.Quantity ?? position.Quantity;

            if (quantityToClose > position.Quantity)
            {
                _logger.LogWarning(
                    "Quantity to close exceeds position quantity: RequestQuantity={RequestQuantity}, PositionQuantity={PositionQuantity}",
                    quantityToClose, position.Quantity);
                return Error.Validation("Position.InvalidQuantity",
                    $"Cannot close {quantityToClose} shares when position only has {position.Quantity} shares");
            }

            // Determine order side (opposite of position side)
            var orderSide = position.Side == PositionSide.Long ? OrderSide.Sell : OrderSide.Buy;

            // Create exit order
            Order exitOrder = request.OrderType switch
            {
                1 => Order.CreateMarketOrder( // Market order
                    position.StockCode,
                    orderSide,
                    quantityToClose,
                    request.AccountNumber,
                    OrderSource.Manual,
                    position.StrategyId),

                2 when request.LimitPrice.HasValue => Order.CreateLimitOrder( // Limit order
                    position.StockCode,
                    orderSide,
                    quantityToClose,
                    request.LimitPrice.Value,
                    request.AccountNumber,
                    OrderSource.Manual,
                    position.StrategyId),

                _ => throw new ArgumentException($"Invalid order type: {request.OrderType}")
            };

            // Add exit order
            await _orderRepository.AddAsync(exitOrder, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Position close order created successfully: OrderId={OrderId}, PositionId={PositionId}, Quantity={Quantity}",
                exitOrder.Id.Value, request.PositionId, quantityToClose);

            return exitOrder.Id.Value;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid position close request");
            return Error.Validation("Position.Invalid", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close position: PositionId={PositionId}", request.PositionId);
            return Error.Failure("Position.CloseFailed", "An error occurred while closing the position");
        }
    }
}
