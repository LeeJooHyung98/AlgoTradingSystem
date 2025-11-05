using AlgoTrading.Core.Entities.Risk;
using AlgoTrading.Core.Entities.Trading;

namespace AlgoTrading.Core.Interfaces.Services;

/// <summary>
/// description : 트레이딩 시스템의 리스크를 관리하고 모니터링하는 도메인 서비스 인터페이스
/// Details : VaR/CVaR 계산, 리스크 위반 검증, 킬 스위치 관리 등 포괄적인 리스크 관리 기능을 제공합니다.
///           실시간으로 포지션과 계좌를 모니터링하여 리스크 한도 초과 시 자동으로 대응합니다.
/// Applied technology patterns : Domain Service Pattern, Strategy Pattern, Observer Pattern, Circuit Breaker Pattern
/// </summary>
public interface IRiskManagementService
{
    /// <summary>
    /// 주문 리스크 검증
    /// </summary>
    Task<bool> ValidateOrderRiskAsync(Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포지션 리스크 계산
    /// </summary>
    Task<decimal> CalculatePositionRiskAsync(Position position, CancellationToken cancellationToken = default);

    /// <summary>
    /// VaR (Value at Risk) 계산
    /// </summary>
    Task<decimal> CalculateVaRAsync(string accountNumber, decimal confidenceLevel, int timeHorizon, CancellationToken cancellationToken = default);

    /// <summary>
    /// CVaR (Conditional Value at Risk) 계산
    /// </summary>
    Task<decimal> CalculateCVaRAsync(string accountNumber, decimal confidenceLevel, int timeHorizon, CancellationToken cancellationToken = default);

    /// <summary>
    /// 리스크 위반 검증
    /// </summary>
    Task<IEnumerable<RiskAlert>> CheckRiskViolationsAsync(Guid riskProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 킬 스위치 활성화 검증
    /// </summary>
    Task<bool> ShouldActivateKillSwitchAsync(Guid riskMonitorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 리스크 알림 생성
    /// </summary>
    Task<RiskAlert> GenerateRiskAlertAsync(Guid riskProfileId, string message, CancellationToken cancellationToken = default);
}
