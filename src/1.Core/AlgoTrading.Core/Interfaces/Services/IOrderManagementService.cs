using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.Interfaces.Services;

/// <summary>
/// description : 주문 생성, 수정, 취소, 체결 처리 서비스 인터페이스
/// Details : Trading Context에서 주문의 생명주기를 관리하는 Application Service입니다. 전략 신호를 받아 Order 엔티티를 생성하고, 계좌 잔고와 리스크 한도를 검증한 후 브로커 API를 통해 거래소에 전송합니다. 주문 상태는 Pending → Submitted → (PartiallyFilled) → Filled 또는 Cancelled로 전이되며, 각 상태 변경 시 도메인 이벤트를 발행합니다. 체결 시 Position 엔티티를 업데이트하고 AccountBalanceUpdatedEvent를 발행하여 계좌 잔고를 차감합니다. CQRS 패턴의 CreateOrderCommand Handler에서 이 서비스를 호출합니다.
/// Applied technology patterns : Application Service Pattern, State Machine Pattern, Event-Driven Architecture
/// </summary>
public interface IOrderManagementService
{
    /// <summary>
    /// 주문 생성
    /// </summary>
    Task<Order> CreateOrderAsync(string accountNumber, string stockCode, Enums.OrderSide side, int quantity, decimal? limitPrice = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 주문 수정
    /// </summary>
    Task<Order> ModifyOrderAsync(Guid orderId, int? newQuantity = null, decimal? newLimitPrice = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 주문 취소
    /// </summary>
    Task CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 주문 검증
    /// </summary>
    Task<bool> ValidateOrderAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// 주문 상태 업데이트
    /// </summary>
    Task UpdateOrderStatusAsync(Guid orderId, Enums.OrderStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 주문 체결 처리
    /// </summary>
    Task ProcessOrderExecutionAsync(Guid orderId, int executedQuantity, decimal executedPrice, CancellationToken cancellationToken = default);
}
