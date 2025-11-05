using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.DomainEvents.Risk;

/// <summary>
/// description : 리스크 모니터링 중 경고 조건이 감지되어 알림이 생성되었을 때 발생하는 도메인 이벤트
/// Details : Monitoring Context에서 RiskMonitor가 리스크 위반 또는 주의 상황을 감지할 때 발행됩니다. RiskAlertType(위반 유형), AlertSeverity(Info/Medium/High/Critical), Message(상세 메시지)를 포함하여 알림 정보를 전달합니다. 이벤트 구독자는 Severity에 따라 적절한 알림 채널(Email, SMS, Telegram 등)을 선택하여 사용자에게 경고를 전송하고, Critical 알림의 경우 자동 대응 로직(신규 주문 차단, 포지션 강제 청산)을 실행할 수 있습니다. 알림은 데이터베이스에 기록되어 감사 추적과 리스크 분석에 활용됩니다.
/// Applied technology patterns : Domain Event Pattern, Alert Notification Pattern, Monitoring Pattern
/// </summary>
public class RiskAlertGeneratedEvent : DomainEvent
{
    /// <summary>
    /// 알림 ID
    /// </summary>
    public Guid AlertId { get; }

    /// <summary>
    /// 리스크 프로필 ID
    /// </summary>
    public Guid RiskProfileId { get; }

    /// <summary>
    /// 알림 유형
    /// </summary>
    public RiskAlertType AlertType { get; }

    /// <summary>
    /// 알림 심각도
    /// </summary>
    public AlertSeverity Severity { get; }

    /// <summary>
    /// 알림 메시지
    /// </summary>
    public string Message { get; }

    public RiskAlertGeneratedEvent(Guid alertId, Guid riskProfileId, RiskAlertType alertType, AlertSeverity severity, string message)
    {
        AlertId = alertId;
        RiskProfileId = riskProfileId;
        AlertType = alertType;
        Severity = severity;
        Message = message;
    }
}
