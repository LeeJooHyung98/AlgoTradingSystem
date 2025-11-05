using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.DomainEvents.Trading;

/// <summary>
/// description : 새로운 주문이 생성되었을 때 발생하는 도메인 이벤트
/// Details : 주문 생성 직후 발행되며, 브로커 전송, 리스크 검증, 포지션 추적, 알림 등의 워크플로우를 시작합니다.
///           CQRS 패턴의 Command 측면에서 이벤트 소싱과 함께 사용될 수 있습니다.
/// Applied technology patterns : Domain Event Pattern, CQRS Pattern, Event Sourcing, Saga Pattern
/// </summary>
public class OrderCreatedEvent : DomainEvent
{
    /// <summary>
    /// 주문 ID
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string AccountNumber { get; }

    /// <summary>
    /// 종목 코드
    /// </summary>
    public string StockCode { get; }

    /// <summary>
    /// 주문 방향
    /// </summary>
    public OrderSide Side { get; }

    /// <summary>
    /// 주문 수량
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// 주문 가격
    /// </summary>
    public decimal? LimitPrice { get; }

    public OrderCreatedEvent(Guid orderId, string accountNumber, string stockCode, OrderSide side, int quantity, decimal? limitPrice = null)
    {
        OrderId = orderId;
        AccountNumber = accountNumber;
        StockCode = stockCode;
        Side = side;
        Quantity = quantity;
        LimitPrice = limitPrice;
    }
}
