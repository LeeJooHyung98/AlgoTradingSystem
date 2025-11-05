namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 기술적 분석에 사용되는 지표(Indicator) 유형 정의
/// Details : 알고리즘 트레이딩 전략에서 사용하는 다양한 기술적 지표를 분류합니다. 이동평균(SMA, EMA, WMA), 모멘텀 지표(MACD, RSI, Stochastic), 추세 지표(ADX), 변동성 지표(ATR, Bollinger Bands), 거래량 지표(Volume, OBV, MFI), 가격 지표(VWAP) 등을 포함합니다. Strategy Context에서 전략 생성 시 필요한 지표를 선택하고, Market Data Context의 실시간 데이터를 기반으로 지표 값을 계산합니다. 각 지표는 Factory Pattern을 통해 생성되며, 지표별 파라미터 최적화를 지원합니다.
/// Applied technology patterns : Factory Pattern, Strategy Pattern, Calculation Engine Pattern
/// </summary>
public enum IndicatorType
{
    /// <summary>
    /// 단순 이동평균 (Simple Moving Average)
    /// </summary>
    SMA = 1,

    /// <summary>
    /// 지수 이동평균 (Exponential Moving Average)
    /// </summary>
    EMA = 2,

    /// <summary>
    /// 가중 이동평균 (Weighted Moving Average)
    /// </summary>
    WMA = 3,

    /// <summary>
    /// MACD (Moving Average Convergence Divergence)
    /// </summary>
    MACD = 4,

    /// <summary>
    /// ADX (Average Directional Index)
    /// </summary>
    ADX = 5,

    /// <summary>
    /// RSI (Relative Strength Index)
    /// </summary>
    RSI = 6,

    /// <summary>
    /// 스토캐스틱 (Stochastic Oscillator)
    /// </summary>
    Stochastic = 7,

    /// <summary>
    /// CCI (Commodity Channel Index)
    /// </summary>
    CCI = 8,

    /// <summary>
    /// 윌리엄스 %R (Williams %R)
    /// </summary>
    Williams = 9,

    /// <summary>
    /// 볼린저 밴드 (Bollinger Bands)
    /// </summary>
    BollingerBands = 10,

    /// <summary>
    /// ATR (Average True Range)
    /// </summary>
    ATR = 11,

    /// <summary>
    /// 켈트너 채널 (Keltner Channel)
    /// </summary>
    KeltnerChannel = 12,

    /// <summary>
    /// 돈치안 채널 (Donchian Channel)
    /// </summary>
    DonchianChannel = 13,

    /// <summary>
    /// 거래량 (Volume)
    /// </summary>
    Volume = 14,

    /// <summary>
    /// OBV (On-Balance Volume)
    /// </summary>
    OBV = 15,

    /// <summary>
    /// MFI (Money Flow Index)
    /// </summary>
    MFI = 16,

    /// <summary>
    /// VWAP (Volume Weighted Average Price)
    /// </summary>
    VWAP = 17,

    /// <summary>
    /// 사용자 정의 지표
    /// </summary>
    Custom = 18
}
