namespace AlgoTrading.Core.DomainEvents.Trading;

/// <summary>
/// description : 주문이 취소되었을 때 발생하는 도메인 이벤트
/// Details : Trading Context에서 Order 엔티티의 상태가 Cancelled로 전환될 때 발행됩니다. 취소는 사용자 수동 취소, 시스템 자동 취소(예: 시장 종료, 리스크 위반), 거래소 거부 등 다양한 이유로 발생하며, CancellationReason에 사유를 기록합니다. 이벤트 구독자는 계좌 잔고의 예약 금액을 해제하고, 주문 대기 큐에서 제거하며, 사용자에게 취소 알림을 전송합니다. Risk Management Context는 취소된 주문을 리스크 계산에서 제외합니다. 부분 체결 후 취소된 경우에도 이 이벤트가 발행되며, 체결된 수량은 별도의 OrderFilledEvent로 처리됩니다.
/// Applied technology patterns : Domain Event Pattern, Compensating Action Pattern, State Transition Event
/// </summary>
public class OrderCancelledEvent : DomainEvent
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
    /// 취소 사유
    /// </summary>
    public string? CancellationReason { get; }

    public OrderCancelledEvent(Guid orderId, string accountNumber, string stockCode, string? cancellationReason = null)
    {
        OrderId = orderId;
        AccountNumber = accountNumber;
        StockCode = stockCode;
        CancellationReason = cancellationReason;
    }
}
