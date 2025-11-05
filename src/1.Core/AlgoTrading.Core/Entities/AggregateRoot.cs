using AlgoTrading.Core.DomainEvents;

namespace AlgoTrading.Core.Entities;

/// <summary>
/// description : DDD Aggregate Root 기본 클래스
/// Details : DDD의 Aggregate Root 패턴을 구현하며, Aggregate 경계 내의 일관성(Consistency)과 불변식(Invariants)을 보장합니다. Domain Event를 관리하고 발행하는 기능을 제공하여 Event-Driven Architecture를 지원합니다. Aggregate 내부의 모든 엔티티는 반드시 Aggregate Root를 통해서만 접근 가능합니다.
/// Applied technology patterns : DDD Aggregate Root Pattern, Domain Events Pattern, Event Sourcing Support, Bounded Context Pattern
/// </summary>
/// <typeparam name="TId">Type of the aggregate root ID</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Domain events raised by this aggregate
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() : base()
    {
    }

    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Add a domain event to be published
    /// </summary>
    /// <param name="domainEvent">Domain event to add</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove a specific domain event
    /// </summary>
    /// <param name="domainEvent">Domain event to remove</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clear all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Check if there are any domain events
    /// </summary>
    public bool HasDomainEvents => _domainEvents.Any();
}
