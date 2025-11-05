namespace AlgoTrading.Core.DomainEvents.Strategy;

/// <summary>
/// description : 트레이딩 전략이 활성화(Active 상태로 전환)되었을 때 발생하는 도메인 이벤트
/// Details : Strategy Context에서 전략의 상태가 Testing 또는 Inactive에서 Active로 전환될 때 TradingStrategy 엔티티가 발행합니다. 전략 활성화는 실시간 매매 신호 생성을 시작하는 중요한 상태 전이이며, 이벤트 구독자는 Strategy Execution Service를 시작하고, Risk Monitor를 활성화하며, 사용자에게 알림을 전송합니다. Market Data Service는 이 이벤트를 받아 해당 전략이 모니터링하는 종목들의 실시간 데이터 구독을 시작합니다. 전략 이름과 활성화 시각을 포함하여 감사 추적(Audit Trail)을 제공합니다.
/// Applied technology patterns : Domain Event Pattern, State Transition Event, Pub-Sub Pattern
/// </summary>
public class StrategyActivatedEvent : DomainEvent
{
    /// <summary>
    /// 전략 ID
    /// </summary>
    public Guid StrategyId { get; }

    /// <summary>
    /// 전략 이름
    /// </summary>
    public string StrategyName { get; }

    /// <summary>
    /// 활성화 시간
    /// </summary>
    public DateTime ActivatedAt { get; }

    public StrategyActivatedEvent(Guid strategyId, string strategyName, DateTime activatedAt)
    {
        StrategyId = strategyId;
        StrategyName = strategyName;
        ActivatedAt = activatedAt;
    }
}
