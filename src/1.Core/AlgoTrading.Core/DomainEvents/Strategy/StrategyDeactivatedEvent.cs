namespace AlgoTrading.Core.DomainEvents.Strategy;

/// <summary>
/// description : 트레이딩 전략이 비활성화(Inactive 상태로 전환)되었을 때 발생하는 도메인 이벤트
/// Details : Strategy Context에서 전략의 상태가 Active에서 Inactive로 전환될 때 발행됩니다. 비활성화는 사용자 수동 중지, 리스크 위반 자동 중지, 성과 악화 등 다양한 이유로 발생하며, Reason 속성에 비활성화 사유를 기록합니다. 이벤트 구독자는 Strategy Execution Service를 중지하고, 모든 열린 포지션에 대해 청산 신호를 발생시킬지 결정하며, 실시간 데이터 구독을 해제합니다. Risk Management Context는 이 이벤트를 수신하여 해당 전략의 리스크 모니터링을 일시 중지합니다.
/// Applied technology patterns : Domain Event Pattern, State Transition Event, Audit Trail Pattern
/// </summary>
public class StrategyDeactivatedEvent : DomainEvent
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
    /// 비활성화 시간
    /// </summary>
    public DateTime DeactivatedAt { get; }

    /// <summary>
    /// 비활성화 이유
    /// </summary>
    public string? Reason { get; }

    public StrategyDeactivatedEvent(Guid strategyId, string strategyName, DateTime deactivatedAt, string? reason = null)
    {
        StrategyId = strategyId;
        StrategyName = strategyName;
        DeactivatedAt = deactivatedAt;
        Reason = reason;
    }
}
