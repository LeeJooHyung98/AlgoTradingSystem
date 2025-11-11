using AlgoTrading.Core.Entities.Monitoring;
using AlgoTrading.Core.Entities.Portfolio;

namespace AlgoTrading.Core.Interfaces;

/// <summary>
/// description(설명) : Trading Metric repository interface (거래 메트릭 리포지토리 인터페이스)
/// Details(상세설명) : Repository for TradingMetric entity with time-series queries (시계열 쿼리를 포함한 TradingMetric 엔티티 리포지토리)
/// Applied technology patterns(적용기술패턴) : Repository Pattern, Time-Series Pattern (리포지토리 패턴, 시계열 패턴)
/// </summary>
public interface ITradingMetricRepository : IRepository<TradingMetric, TradingMetricId>
{
    /// <summary>
    /// Get metrics by account and time range (계좌 및 시간 범위로 메트릭 조회)
    /// </summary>
    Task<IReadOnlyList<TradingMetric>> GetByAccountAndTimeRangeAsync(
        AccountId accountId,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest metric for account (계좌의 최신 메트릭 조회)
    /// </summary>
    Task<TradingMetric?> GetLatestByAccountAsync(
        AccountId accountId,
        CancellationToken cancellationToken = default);
}
