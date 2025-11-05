namespace AlgoTrading.Core.DomainEvents;

/// <summary>
/// description : 도메인 이벤트 기본 인터페이스
/// Details : Domain-Driven Design의 핵심 패턴으로, 도메인에서 발생한 중요한 사건을 표현하는 불변 객체입니다. 모든 도메인 이벤트는 EventId(고유 식별자)와 OccurredAt(발생 시각)을 가지며, 과거형 명사로 명명됩니다(예: OrderFilledEvent, SignalGeneratedEvent). Aggregate Root가 상태를 변경할 때 이벤트를 발행하고, Application Layer의 Handler가 이벤트를 구독하여 사이드 이펙트(알림 전송, 캐시 무효화, 다른 Aggregate 업데이트)를 처리합니다. Event Sourcing에서는 이벤트 스트림이 엔티티의 전체 히스토리를 나타냅니다.
/// Applied technology patterns : Domain Event Pattern (DDD), Event-Driven Architecture, Marker Interface Pattern
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    DateTime OccurredAt { get; }
}
