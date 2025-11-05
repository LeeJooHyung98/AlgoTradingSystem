namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 트레이딩 전략의 유형을 분류하는 열거형
/// Details : 모멘텀, 평균회귀, 돌파, 차익거래 등 다양한 전략 유형을 정의하며, 전략 생성 및 분류에 사용됩니다.
/// Applied technology patterns : Strategy Pattern, Enum Pattern, Domain-Driven Design
/// </summary>
public enum StrategyType
{
    /// <summary>
    /// 모멘텀 전략
    /// </summary>
    Momentum = 1,

    /// <summary>
    /// 평균 회귀 전략
    /// </summary>
    MeanReversion = 2,

    /// <summary>
    /// 돌파 전략
    /// </summary>
    Breakout = 3,

    /// <summary>
    /// 차익거래 전략
    /// </summary>
    Arbitrage = 4,

    /// <summary>
    /// 페어 트레이딩 전략
    /// </summary>
    PairTrading = 5,

    /// <summary>
    /// 마켓 메이킹 전략
    /// </summary>
    MarketMaking = 6,

    /// <summary>
    /// 사용자 정의 전략
    /// </summary>
    Custom = 7
}
