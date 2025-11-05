namespace AlgoTrading.Core.DomainEvents.Risk;

/// <summary>
/// description : 긴급 킬 스위치가 활성화되었을 때 발생하는 도메인 이벤트
/// Details : 심각한 손실이나 시스템 이상 발생 시 모든 거래를 즉시 중단시키는 킬 스위치 활성화를 알립니다.
///           모든 활성 주문 취소, 포지션 청산, 거래 중단 등 최우선 순위의 보호 조치를 실행합니다.
/// Applied technology patterns : Circuit Breaker Pattern, Emergency Stop Pattern, Domain Event Pattern, Observer Pattern
/// </summary>
public class KillSwitchActivatedEvent : DomainEvent
{
    /// <summary>
    /// 리스크 모니터 ID
    /// </summary>
    public Guid RiskMonitorId { get; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string AccountNumber { get; }

    /// <summary>
    /// 활성화 사유
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// 현재 손실 금액
    /// </summary>
    public decimal CurrentLoss { get; }

    /// <summary>
    /// 손실 한도
    /// </summary>
    public decimal LossLimit { get; }

    public KillSwitchActivatedEvent(Guid riskMonitorId, string accountNumber, string reason, decimal currentLoss, decimal lossLimit)
    {
        RiskMonitorId = riskMonitorId;
        AccountNumber = accountNumber;
        Reason = reason;
        CurrentLoss = currentLoss;
        LossLimit = lossLimit;
    }
}
