using System.Linq.Expressions;
using AlgoTrading.Core.Entities;
using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Interfaces;

/// <summary>
/// description(설명) : Generic repository interface (제네릭 리포지토리 인터페이스)
/// Details(상세설명) : Base repository pattern with common CRUD operations (공통 CRUD 작업을 포함한 기본 리포지토리 패턴)
/// Applied technology patterns(적용기술패턴) : Repository Pattern, Generic Pattern (리포지토리 패턴, 제네릭 패턴)
/// </summary>
public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : GuidId
{
    /// <summary>
    /// Get entity by ID (ID로 엔티티 조회)
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities (모든 엔티티 조회)
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Find entities by predicate (조건으로 엔티티 조회)
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add entity (엔티티 추가)
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities (여러 엔티티 추가)
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity (엔티티 업데이트)
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Delete entity (엔티티 삭제)
    /// </summary>
    void Remove(TEntity entity);

    /// <summary>
    /// Delete multiple entities (여러 엔티티 삭제)
    /// </summary>
    void RemoveRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Check if entity exists (엔티티 존재 여부 확인)
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count entities (엔티티 개수)
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Count entities by predicate (조건으로 엔티티 개수)
    /// </summary>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}
