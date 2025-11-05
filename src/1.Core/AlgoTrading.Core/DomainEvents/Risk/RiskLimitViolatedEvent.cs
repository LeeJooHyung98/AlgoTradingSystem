using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.DomainEvents.Risk;

/// <summary>
/// description : 리스크 한도가 위반되었을 때 발생하는 도메인 이벤트
/// Details : VaR 초과, 포지션 크기 초과, 일일 손실 한도 초과 등 리스크 위반 시 발행됩니다.
///           긴급 알림 전송, 주문 차단, 포지션 청산 등 즉각적인 리스크 대응 조치를 트리거합니다.
/// Applied technology patterns : Domain Event Pattern, Circuit Breaker Pattern, Observer Pattern, Alert Pattern
/// </summary>
public class RiskLimitViolatedEvent : DomainEvent
{
    /// <summary>
    /// 리스크 프로필 ID
    /// </summary>
    public Guid RiskProfileId { get; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string AccountNumber { get; }

    /// <summary>
    /// 위반 유형
    /// </summary>
    public RiskViolationType ViolationType { get; }

    /// <summary>
    /// 현재 값
    /// </summary>
    public decimal CurrentValue { get; }

    /// <summary>
    /// 한도 값
    /// </summary>
    public decimal LimitValue { get; }

    /// <summary>
    /// 위반 메시지
    /// </summary>
    public string Message { get; }

    public RiskLimitViolatedEvent(Guid riskProfileId, string accountNumber, RiskViolationType violationType, decimal currentValue, decimal limitValue, string message)
    {
        RiskProfileId = riskProfileId;
        AccountNumber = accountNumber;
        ViolationType = violationType;
        CurrentValue = currentValue;
        LimitValue = limitValue;
        Message = message;
    }
}
