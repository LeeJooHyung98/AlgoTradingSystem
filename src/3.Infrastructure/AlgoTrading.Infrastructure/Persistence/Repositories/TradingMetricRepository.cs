using AlgoTrading.Core.Entities.Monitoring;
using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// description(설명) : Trading Metric repository implementation (거래 메트릭 리포지토리 구현)
/// Details(상세설명) : TradingMetric entity repository with time-series queries for TimescaleDB (TimescaleDB용 시계열 쿼리를 포함한 TradingMetric 엔티티 리포지토리)
/// Applied technology patterns(적용기술패턴) : Repository Pattern, Time-Series Pattern (리포지토리 패턴, 시계열 패턴)
/// </summary>
public class TradingMetricRepository : Repository<TradingMetric, TradingMetricId>, ITradingMetricRepository
{
    public TradingMetricRepository(AlgoTradingDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TradingMetric>> GetByAccountAndTimeRangeAsync(
        AccountId accountId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
    {
        if (accountId == null)
            throw new ArgumentNullException(nameof(accountId));

        if (startTime > endTime)
            throw new ArgumentException("Start time must be before end time");

        return await _dbSet
            .Where(m => m.AccountId == accountId
                && m.MetricTimestamp >= startTime
                && m.MetricTimestamp <= endTime)
            .OrderBy(m => m.MetricTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<TradingMetric?> GetLatestByAccountAsync(
        AccountId accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == null)
            throw new ArgumentNullException(nameof(accountId));

        return await _dbSet
            .Where(m => m.AccountId == accountId)
            .OrderByDescending(m => m.MetricTimestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
