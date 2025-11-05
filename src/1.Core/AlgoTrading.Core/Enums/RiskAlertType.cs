namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 리스크 관리 시스템에서 발생하는 알림 유형을 정의하는 열거형
/// Details : VaR 초과, 낙폭 초과, 손절매 실행 등 다양한 리스크 알림 상황을 분류하며, 알림 시스템과 연동됩니다.
/// Applied technology patterns : Enum Pattern, Observer Pattern, Domain Event Pattern
/// </summary>
public enum RiskAlertType
{
    /// <summary>
    /// 고위험
    /// </summary>
    HighRisk = 1,

    /// <summary>
    /// VaR 초과
    /// </summary>
    VaRExceeded = 2,

    /// <summary>
    /// 낙폭 초과
    /// </summary>
    DrawdownExceeded = 3,

    /// <summary>
    /// 높은 노출도
    /// </summary>
    HighExposure = 4,

    /// <summary>
    /// 낮은 마진
    /// </summary>
    LowMargin = 5,

    /// <summary>
    /// 낮은 잔고
    /// </summary>
    LowBalance = 6,

    /// <summary>
    /// 손절매 실행됨
    /// </summary>
    StopLossTriggered = 7,

    /// <summary>
    /// 익절매 실행됨
    /// </summary>
    TakeProfitTriggered = 8
}
