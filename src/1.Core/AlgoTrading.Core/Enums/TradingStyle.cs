namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 거래 보유 기간에 따른 트레이딩 스타일 분류
/// Details : 투자 시간 지평(time horizon)에 따라 트레이딩 스타일을 정의합니다. Scalping(초단타), DayTrading(당일매매), SwingTrading(단기), PositionTrading(중기), LongTermInvesting(장기)로 구분하여 각 스타일에 맞는 전략 파라미터, 리스크 관리, 수수료 최적화를 적용합니다. Strategy Context에서 전략의 TradingStyle에 따라 적절한 TimeFrame과 Stop-Loss 설정을 자동으로 조정합니다.
/// Applied technology patterns : Strategy Pattern, Configuration Pattern
/// </summary>
public enum TradingStyle
{
    /// <summary>
    /// 스캘핑 (초단타 매매)
    /// </summary>
    Scalping = 1,

    /// <summary>
    /// 데이 트레이딩 (당일 매매)
    /// </summary>
    DayTrading = 2,

    /// <summary>
    /// 스윙 트레이딩 (단기 매매)
    /// </summary>
    SwingTrading = 3,

    /// <summary>
    /// 포지션 트레이딩 (중기 매매)
    /// </summary>
    PositionTrading = 4,

    /// <summary>
    /// 장기 투자
    /// </summary>
    LongTermInvesting = 5
}
