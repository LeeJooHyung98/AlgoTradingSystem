using AlgoTrading.Core.Entities.Strategy;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : Backtest 집합근(백테스트 결과)에 대한 데이터 접근 인터페이스
/// Details : Strategy Context의 Backtest 엔티티를 관리하며, 전략의 과거 성과를 시뮬레이션한 결과를 저장합니다. 백테스트는 시작/종료 날짜, 초기 자본, 총 수익률, Sharpe Ratio, Max Drawdown 등의 성과 지표를 포함하는 Aggregate Root입니다. 전략별 최신 백테스트를 조회하여 전략 비교와 파라미터 최적화에 활용하며, 완료된 백테스트만 필터링하여 분석 대상을 명확히 합니다. Backtesting Service는 Walk-Forward Analysis를 수행할 때 여러 백테스트를 생성하고 이 리포지토리를 통해 저장합니다.
/// Applied technology patterns : Repository Pattern, Aggregate Root Pattern, Performance Metrics Pattern
/// </summary>
public interface IBacktestRepository : IRepository<Backtest>
{
    /// <summary>
    /// 전략 ID로 백테스트 목록 조회
    /// </summary>
    Task<IEnumerable<Backtest>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 이름으로 백테스트 조회
    /// </summary>
    Task<Backtest?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 날짜 범위로 백테스트 목록 조회
    /// </summary>
    Task<IEnumerable<Backtest>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 완료된 백테스트 목록 조회
    /// </summary>
    Task<IEnumerable<Backtest>> GetCompletedBacktestsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략의 최신 백테스트 조회
    /// </summary>
    Task<Backtest?> GetLatestByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);
}
