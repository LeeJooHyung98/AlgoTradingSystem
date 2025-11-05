using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : 트레이딩 전략(TradingStrategy) 엔티티에 대한 데이터 액세스를 담당하는 리포지토리 인터페이스
/// Details : 전략 생성, 조회, 수정 기능과 함께 전략 규칙, 신호, 백테스트 결과를 포함한
///           복잡한 Aggregate Root 조회를 지원합니다. Strategy Context의 핵심 인터페이스입니다.
/// Applied technology patterns : Repository Pattern, Aggregate Root Pattern, Lazy Loading, Domain-Driven Design
/// </summary>
public interface IStrategyRepository : IRepository<TradingStrategy>
{
    /// <summary>
    /// 이름으로 전략 조회
    /// </summary>
    Task<TradingStrategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 상태로 전략 목록 조회
    /// </summary>
    Task<IEnumerable<TradingStrategy>> GetByStatusAsync(Enums.StrategyStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 유형으로 전략 목록 조회
    /// </summary>
    Task<IEnumerable<TradingStrategy>> GetByTypeAsync(Enums.StrategyType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// 활성화된 전략 목록 조회
    /// </summary>
    Task<IEnumerable<TradingStrategy>> GetActiveStrategiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 사용자 ID로 전략 목록 조회
    /// </summary>
    Task<IEnumerable<TradingStrategy>> GetByCreatedByAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 규칙 포함하여 조회
    /// </summary>
    Task<TradingStrategy?> GetWithRulesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 신호 포함하여 조회
    /// </summary>
    Task<TradingStrategy?> GetWithSignalsAsync(Guid id, CancellationToken cancellationToken = default);
}
