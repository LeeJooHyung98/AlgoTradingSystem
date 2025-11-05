namespace AlgoTrading.Core.Entities;

/// <summary>
/// description : 모든 엔티티의 기본 클래스 (Base Entity class)
/// Details : DDD(Domain-Driven Design)에서 Entity의 기본 구현을 제공하며, 고유 식별자(Identity)와 타임스탬프(CreatedAt, LastModifiedAt)를 관리합니다. 모든 엔티티는 ID를 통해 비교되며, 값이 아닌 식별자로 동일성(Identity)을 판단합니다. Generic type parameter를 통해 다양한 ID 타입을 지원합니다.
/// Applied technology patterns : DDD Entity Pattern, Identity Pattern, Temporal Pattern (생성/수정 시간 추적)
/// </summary>
/// <typeparam name="TId">Type of the entity ID</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Entity creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? LastModifiedAt { get; protected set; }

    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Equality comparison by ID
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Hash code based on ID
    /// </summary>
    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }

    /// <summary>
    /// Equality operator
    /// </summary>
    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    /// <summary>
    /// Inequality operator
    /// </summary>
    public static bool operator !=(Entity<TId>? a, Entity<TId>? b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Mark entity as modified
    /// </summary>
    protected void MarkAsModified()
    {
        LastModifiedAt = DateTime.UtcNow;
    }
}
