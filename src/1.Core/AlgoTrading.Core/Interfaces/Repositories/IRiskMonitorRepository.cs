using AlgoTrading.Core.Entities.Risk;
using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : RiskMonitor 엔티티(실시간 리스크 감시 인스턴스)에 대한 데이터 접근 인터페이스
/// Details : Monitoring Context에서 실시간 리스크 모니터링 인스턴스를 관리합니다. RiskMonitor는 특정 계좌나 RiskProfile에 대한 모니터링 세션을 나타내며, 모니터링 상태(Initializing/Running/Paused/Stopped), 마지막 점검 시간, 위반 횟수 등을 추적합니다. Running 상태의 모니터만 Background Service에서 실시간 점검을 수행하며, 리스크 위반 발생 시 RiskLimitViolatedEvent를 발행하고 알림을 전송합니다. 계좌당 하나의 RiskMonitor 인스턴스가 존재하며, 상태 전이는 State Pattern을 따릅니다.
/// Applied technology patterns : Repository Pattern, State Pattern, Real-time Monitoring Pattern
/// </summary>
public interface IRiskMonitorRepository : IRepository<RiskMonitor>
{
    /// <summary>
    /// 리스크 프로필 ID로 리스크 모니터 조회
    /// </summary>
    Task<RiskMonitor?> GetByRiskProfileIdAsync(Guid riskProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 계좌번호로 리스크 모니터 조회
    /// </summary>
    Task<RiskMonitor?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 모니터링 상태로 리스크 모니터 목록 조회
    /// </summary>
    Task<IEnumerable<RiskMonitor>> GetByStatusAsync(Enums.MonitoringStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 실행 중인 리스크 모니터 목록 조회
    /// </summary>
    Task<IEnumerable<RiskMonitor>> GetRunningMonitorsAsync(CancellationToken cancellationToken = default);
}
