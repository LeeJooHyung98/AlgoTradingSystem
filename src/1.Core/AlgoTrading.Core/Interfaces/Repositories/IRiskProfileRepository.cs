using AlgoTrading.Core.Entities.Risk;
using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : RiskProfile 집합근에 대한 데이터 접근 인터페이스
/// Details : Risk Management Context의 RiskProfile 엔티티를 관리합니다. 리스크 프로필은 포지션 크기 한도, VaR 제한, 손절매 규칙, 최대 포지션 수 등 리스크 관리 정책을 정의하는 Aggregate Root입니다. 계좌 또는 전략에 할당되며, Active 상태의 프로필만 실시간 리스크 모니터링에 적용됩니다. Risk Management Service는 주문 생성 전 이 프로필을 검증하여 리스크 위반을 사전 방지하고, 위반 발생 시 RiskLimitViolatedEvent를 발행합니다.
/// Applied technology patterns : Repository Pattern, Policy Pattern, Aggregate Root Pattern
/// </summary>
public interface IRiskProfileRepository : IRepository<RiskProfile>
{
    /// <summary>
    /// 계좌번호로 리스크 프로필 조회
    /// </summary>
    Task<RiskProfile?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 이름으로 리스크 프로필 조회
    /// </summary>
    Task<RiskProfile?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 상태로 리스크 프로필 목록 조회
    /// </summary>
    Task<IEnumerable<RiskProfile>> GetByStatusAsync(RiskProfileStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 활성화된 리스크 프로필 목록 조회
    /// </summary>
    Task<IEnumerable<RiskProfile>> GetActiveProfilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 ID로 리스크 프로필 목록 조회
    /// </summary>
    Task<IEnumerable<RiskProfile>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);
}
