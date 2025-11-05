namespace AlgoTrading.Core.DomainEvents.Trading;

/// <summary>
/// description : 주문이 전량 체결되었을 때 발생하는 도메인 이벤트
/// Details : 주문 전량 체결 시 발행되며, 포지션 업데이트, 계좌 잔고 갱신, 성과 계산 등을 트리거합니다.
///           평균 체결 가격과 수수료 정보를 포함하여 정확한 손익 계산을 지원합니다.
/// Applied technology patterns : Domain Event Pattern, Event-Driven Architecture, Saga Pattern, Eventual Consistency
/// </summary>
public class OrderFilledEvent : DomainEvent
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
    /// 체결 수량
    /// </summary>
    public int FilledQuantity { get; }

    /// <summary>
    /// 평균 체결 가격
    /// </summary>
    public decimal AveragePrice { get; }

    /// <summary>
    /// 체결 시간
    /// </summary>
    public DateTime FilledAt { get; }

    public OrderFilledEvent(Guid orderId, string accountNumber, string stockCode, int filledQuantity, decimal averagePrice, DateTime filledAt)
    {
        OrderId = orderId;
        AccountNumber = accountNumber;
        StockCode = stockCode;
        FilledQuantity = filledQuantity;
        AveragePrice = averagePrice;
        FilledAt = filledAt;
    }
}
