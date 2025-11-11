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
/// description(설명) : Create Order Command Handler (주문 생성 명령 핸들러)
/// Details(상세설명) : Handles CreateOrderCommand by creating Order entity and persisting through repository (Repository를 통해 Order 엔티티를 생성하고 저장하여 CreateOrderCommand를 처리)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, Handler Pattern, Repository Pattern, UnitOfWork Pattern (CQRS 패턴, 핸들러 패턴, Repository 패턴, UnitOfWork 패턴)
/// </summary>
public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ErrorOr<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating order: StockCode={StockCode}, Side={Side}, Type={Type}, Quantity={Quantity}",
                request.StockCode, request.Side, request.Type, request.Quantity);

            // Create StockCode value object
            var stockCode = new StockCode(request.StockCode);

            // Create Order entity based on order type
            Order order = request.Type switch
            {
                1 => Order.CreateMarketOrder( // Market Order
                    stockCode,
                    (OrderSide)request.Side,
                    request.Quantity,
                    request.AccountNumber,
                    (OrderSource)request.Source,
                    request.StrategyId),

                2 when request.LimitPrice.HasValue => Order.CreateLimitOrder( // Limit Order
                    stockCode,
                    (OrderSide)request.Side,
                    request.Quantity,
                    request.LimitPrice.Value,
                    request.AccountNumber,
                    (OrderSource)request.Source,
                    request.StrategyId),

                3 when request.StopPrice.HasValue => Order.CreateStopOrder( // Stop Order
                    stockCode,
                    (OrderSide)request.Side,
                    request.Quantity,
                    request.StopPrice.Value,
                    request.AccountNumber,
                    (OrderSource)request.Source,
                    request.StrategyId),

                _ => throw new ArgumentException($"Invalid order type or missing price: Type={request.Type}")
            };

            // Add order to repository
            await _orderRepository.AddAsync(order, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order created successfully: OrderId={OrderId}", order.Id.Value);

            return order.Id.Value;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid order creation request");
            return Error.Validation("Order.Invalid", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order");
            return Error.Failure("Order.CreationFailed", "An error occurred while creating the order");
        }
    }
}
