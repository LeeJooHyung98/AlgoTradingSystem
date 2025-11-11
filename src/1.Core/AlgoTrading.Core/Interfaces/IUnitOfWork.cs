namespace AlgoTrading.Core.Interfaces;

/// <summary>
/// description(설명) : Unit of Work interface (작업 단위 인터페이스)
/// Details(상세설명) : Manages database transactions and coordinates repository changes (데이터베이스 트랜잭션 관리 및 리포지토리 변경 조정)
/// Applied technology patterns(적용기술패턴) : Unit of Work Pattern (작업 단위 패턴)
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Save all changes to database (데이터베이스에 모든 변경사항 저장)
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin database transaction (데이터베이스 트랜잭션 시작)
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit transaction (트랜잭션 커밋)
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback transaction (트랜잭션 롤백)
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
