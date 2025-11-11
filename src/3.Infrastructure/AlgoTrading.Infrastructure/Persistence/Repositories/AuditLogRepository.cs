using AlgoTrading.Core.Entities.Audit;
using AlgoTrading.Core.Entities.Users;
using AlgoTrading.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// description(설명) : Audit Log repository implementation (감사 로그 리포지토리 구현)
/// Details(상세설명) : AuditLog entity repository with time-series and filtering queries for TimescaleDB (TimescaleDB용 시계열 및 필터링 쿼리를 포함한 AuditLog 엔티티 리포지토리)
/// Applied technology patterns(적용기술패턴) : Repository Pattern, Time-Series Pattern (리포지토리 패턴, 시계열 패턴)
/// </summary>
public class AuditLogRepository : Repository<AuditLog, AuditLogId>, IAuditLogRepository
{
    public AuditLogRepository(AlgoTradingDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserAndTimeRangeAsync(
        UserId userId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        if (userId == null)
            throw new ArgumentNullException(nameof(userId));

        if (startTime > endTime)
            throw new ArgumentException("Start time must be before end time");

        return await _dbSet
            .Where(l => l.UserId == userId
                && l.LogTimestamp >= startTime
                && l.LogTimestamp <= endTime)
            .OrderByDescending(l => l.LogTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty", nameof(entityType));

        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));

        return await _dbSet
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.LogTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByActionTypeAsync(
        ActionType actionType,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        if (startTime > endTime)
            throw new ArgumentException("Start time must be before end time");

        return await _dbSet
            .Where(l => l.Action == actionType
                && l.LogTimestamp >= startTime
                && l.LogTimestamp <= endTime)
            .OrderByDescending(l => l.LogTimestamp)
            .ToListAsync(cancellationToken);
    }
}
