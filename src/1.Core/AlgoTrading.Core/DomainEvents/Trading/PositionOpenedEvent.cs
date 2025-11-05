namespace AlgoTrading.Core.DomainEvents.Trading;

/// <summary>
/// description : 새로운 포지션이 열렸을 때(첫 매수 체결) 발생하는 도메인 이벤트
/// Details : Trading Context에서 주문이 체결되어 새로운 Position 엔티티가 생성될 때 발행됩니다. 포지션 열기는 특정 종목에 대한 첫 번째 매수 주문 체결을 의미하며, EntryPrice(진입가)와 Quantity(수량)를 포함합니다. 이벤트 구독자는 포트폴리오의 포지션 목록을 업데이트하고, Risk Management Context에 새 포지션을 등록하여 리스크 한도를 재계산하며, 사용자에게 포지션 개시 알림을 전송합니다. StrategyId가 포함되어 있어 어떤 전략이 포지션을 생성했는지 추적할 수 있으며, 전략별 성과 분석에 활용됩니다.
/// Applied technology patterns : Domain Event Pattern, Position Tracking Pattern, Event-Driven Architecture
/// </summary>
public class PositionOpenedEvent : DomainEvent
{
    /// <summary>
    /// 포지션 ID
    /// </summary>
    public Guid PositionId { get; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string AccountNumber { get; }

    /// <summary>
    /// 종목 코드
    /// </summary>
    public string StockCode { get; }

    /// <summary>
    /// 포지션 수량
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// 진입 가격
    /// </summary>
    public decimal EntryPrice { get; }

    /// <summary>
    /// 전략 ID
    /// </summary>
    public Guid? StrategyId { get; }

    public PositionOpenedEvent(Guid positionId, string accountNumber, string stockCode, int quantity, decimal entryPrice, Guid? strategyId = null)
    {
        PositionId = positionId;
        AccountNumber = accountNumber;
        StockCode = stockCode;
        Quantity = quantity;
        EntryPrice = entryPrice;
        StrategyId = strategyId;
    }
}
