namespace AlgoTrading.Core.DomainEvents.Strategy;

/// <summary>
/// description : 새로운 트레이딩 전략이 생성되었을 때 발생하는 도메인 이벤트
/// Details : 전략 생성 시점에 발행되며, 다른 Bounded Context나 외부 시스템에 전략 생성을 알립니다.
///           이벤트 기반 아키텍처를 통해 느슨한 결합을 유지하며, 알림, 로깅, 감사 등에 활용됩니다.
/// Applied technology patterns : Domain Event Pattern, Event-Driven Architecture, Publish-Subscribe Pattern
/// </summary>
public class StrategyCreatedEvent : DomainEvent
{
    /// <summary>
    /// 전략 ID
    /// </summary>
    public Guid StrategyId { get; }

    /// <summary>
    /// 전략 이름
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 전략 설명
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 생성자 ID
    /// </summary>
    public string CreatedBy { get; }

    public StrategyCreatedEvent(Guid strategyId, string name, string description, string createdBy)
    {
        StrategyId = strategyId;
        Name = name;
        Description = description;
        CreatedBy = createdBy;
    }
}
