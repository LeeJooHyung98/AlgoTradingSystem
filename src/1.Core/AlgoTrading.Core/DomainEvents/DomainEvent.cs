namespace AlgoTrading.Core.DomainEvents;

/// <summary>
/// description : 도메인 이벤트 추상 기본 클래스
/// Details : 모든 구체적인 도메인 이벤트가 상속하는 베이스 클래스로, IDomainEvent 인터페이스를 구현합니다. 생성 시 자동으로 EventId(Guid.NewGuid())와 OccurredAt(DateTime.UtcNow)을 설정하여 이벤트의 유니크성과 시간 순서를 보장합니다. 불변성(Immutability)을 유지하기 위해 속성은 get-only이며, 생성자를 통해서만 값을 설정할 수 있습니다. Event Sourcing에서 이벤트를 재구성할 때는 두 번째 생성자를 사용하여 기존 EventId와 OccurredAt을 유지합니다.
/// Applied technology patterns : Template Method Pattern, Value Object Pattern (Immutability), Event Sourcing
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }

    protected DomainEvent(Guid eventId, DateTime occurredAt)
    {
        EventId = eventId;
        OccurredAt = occurredAt;
    }
}
