namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 포지션 보호를 위한 손절매 방식의 유형 분류
/// Details : Risk Management Context에서 포지션의 손실을 제한하는 손절매 전략을 정의합니다. Percentage(진입가 대비 퍼센트), Fixed(고정 금액), ATR(Average True Range 기반 변동성 손절), Support(지지선 기반), TrailingStop(추적 손절)을 지원합니다. 각 손절 유형은 시장 상황과 전략 특성에 맞게 선택되며, TrailingStop은 수익이 발생한 포지션의 이익을 보호하면서 추가 수익 기회를 제공합니다. 손절 조건 충족 시 StopLossTriggerEvent가 발행되어 자동 청산이 실행됩니다.
/// Applied technology patterns : Strategy Pattern, Guard Pattern, Event-Driven Architecture
/// </summary>
public enum StopLossType
{
    /// <summary>
    /// 퍼센트 기반
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// 고정 금액
    /// </summary>
    Fixed = 2,

    /// <summary>
    /// ATR 기반
    /// </summary>
    ATR = 3,

    /// <summary>
    /// 지지선 기반
    /// </summary>
    Support = 4,

    /// <summary>
    /// 추적 손절매 (Trailing Stop)
    /// </summary>
    TrailingStop = 5
}
