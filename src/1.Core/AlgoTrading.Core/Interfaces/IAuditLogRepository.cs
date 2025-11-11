using AlgoTrading.Core.Entities.Audit;
using AlgoTrading.Core.Entities.Users;

namespace AlgoTrading.Core.Interfaces;

/// <summary>
/// description(설명) : Audit Log repository interface (감사 로그 리포지토리 인터페이스)
/// Details(상세설명) : Repository for AuditLog entity with time-series and user queries (시계열 및 사용자 쿼리를 포함한 AuditLog 엔티티 리포지토리)
/// Applied technology patterns(적용기술패턴) : Repository Pattern, Time-Series Pattern (리포지토리 패턴, 시계열 패턴)
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog, AuditLogId>
{
    /// <summary>
    /// Get logs by user and time range (사용자 및 시간 범위로 로그 조회)
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByUserAndTimeRangeAsync(
        UserId userId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get logs by entity (엔티티로 로그 조회)
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get logs by action type (작업 타입으로 로그 조회)
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByActionTypeAsync(
        ActionType actionType,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);
}
