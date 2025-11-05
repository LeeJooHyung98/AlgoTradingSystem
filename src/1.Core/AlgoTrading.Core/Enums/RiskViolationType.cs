namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 리스크 관리 규칙 위반의 유형 분류
/// Details : Risk Management Context에서 탐지 가능한 리스크 위반 사항을 정의합니다. PositionSizeExceeded(단일 포지션 크기 초과), MaxPositionsExceeded(최대 포지션 수 초과), ExposureRatioExceeded(총 노출 비율 초과), DailyLossExceeded(일일 손실 한도 초과), VaRExceeded(VaR 한도 초과), MissingStopLoss(손절매 미설정), LeverageExceeded(레버리지 한도 초과), MarginInsufficient(증거금 부족)를 포함합니다. 각 위반 유형에 따라 자동 대응 로직이 실행되며(예: 강제 청산, 신규 주문 차단), RiskLimitViolatedEvent가 발행됩니다.
/// Applied technology patterns : Rule Engine Pattern, Event-Driven Architecture, Guard Pattern
/// </summary>
public enum RiskViolationType
{
    /// <summary>
    /// 포지션 크기 초과
    /// </summary>
    PositionSizeExceeded = 1,

    /// <summary>
    /// 최대 포지션 수 초과
    /// </summary>
    MaxPositionsExceeded = 2,

    /// <summary>
    /// 노출 비율 초과
    /// </summary>
    ExposureRatioExceeded = 3,

    /// <summary>
    /// 일일 손실 한도 초과
    /// </summary>
    DailyLossExceeded = 4,

    /// <summary>
    /// VaR (Value at Risk) 초과
    /// </summary>
    VaRExceeded = 5,

    /// <summary>
    /// 손절매 누락
    /// </summary>
    MissingStopLoss = 6,

    /// <summary>
    /// 레버리지 한도 초과
    /// </summary>
    LeverageExceeded = 7,

    /// <summary>
    /// 마진 부족
    /// </summary>
    MarginInsufficient = 8
}
