namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 시스템 알림 및 리스크 경고의 심각도 수준
/// Details : Monitoring Context에서 발생하는 알림의 중요도를 Info(정보), Medium(주의), High(경고), Critical(긴급)로 분류합니다. Info는 일반 정보성 알림(전략 활성화 등), Medium은 주의가 필요한 상황(수익률 임계값 도달), High는 즉시 확인이 필요한 경고(손실 한도 근접), Critical은 긴급 대응이 필요한 상황(Kill Switch 발동, 마진콜)을 나타냅니다. 심각도에 따라 알림 채널(Email, SMS, Telegram, Slack)이 다르게 설정되며, Critical 알림은 모든 채널로 즉시 발송됩니다.
/// Applied technology patterns : Observer Pattern, Notification Pattern, Priority Queue Pattern
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// 정보
    /// </summary>
    Info = 1,

    /// <summary>
    /// 중간 (주의)
    /// </summary>
    Medium = 2,

    /// <summary>
    /// 높음 (경고)
    /// </summary>
    High = 3,

    /// <summary>
    /// 심각 (긴급)
    /// </summary>
    Critical = 4
}
