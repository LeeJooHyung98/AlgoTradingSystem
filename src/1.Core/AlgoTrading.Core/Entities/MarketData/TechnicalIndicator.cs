using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.MarketData;

/// <summary>
/// description : 기술적 지표 엔티티 (Technical Indicator Entity)
/// Details : 주식의 기술적 분석 지표(RSI, MACD, 이동평균, 볼린저밴드 등)를 저장하고 관리하는 Entity입니다. 다양한 지표 타입과 파라미터를 지원하며, 신호 해석(매수/매도/중립)을 자동으로 계산합니다. Strategy Context에서 매매 신호 생성에 활용됩니다.
/// Applied technology patterns : DDD Entity Pattern, Factory Pattern (지표별 생성 메서드), Strategy Pattern (지표 계산), Value Object Pattern (StockCode), Market Data Context
/// </summary>
public sealed class TechnicalIndicator : Entity<TechnicalIndicatorId>
{
    /// <summary>
    /// Stock code
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// Indicator type
    /// </summary>
    public IndicatorType Type { get; private set; }

    /// <summary>
    /// Time interval
    /// </summary>
    public CandleInterval Interval { get; private set; }

    /// <summary>
    /// Calculation timestamp
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Indicator value
    /// </summary>
    public decimal Value { get; private set; }

    /// <summary>
    /// Additional indicator values (for multi-line indicators like MACD)
    /// </summary>
    public Dictionary<string, decimal> AdditionalValues { get; private set; }

    /// <summary>
    /// Indicator parameters (e.g., period=14 for RSI)
    /// </summary>
    public Dictionary<string, string> Parameters { get; private set; }

    /// <summary>
    /// Signal interpretation (Bullish/Bearish/Neutral)
    /// </summary>
    public SignalInterpretation? Signal { get; private set; }

    private TechnicalIndicator()
    {
        AdditionalValues = new Dictionary<string, decimal>();
        Parameters = new Dictionary<string, string>();
    }

    private TechnicalIndicator(
        TechnicalIndicatorId id,
        StockCode stockCode,
        IndicatorType type,
        CandleInterval interval,
        DateTime timestamp,
        decimal value,
        Dictionary<string, string> parameters)
    {
        Id = id;
        StockCode = stockCode;
        Type = type;
        Interval = interval;
        Timestamp = timestamp;
        Value = value;
        Parameters = parameters ?? new Dictionary<string, string>();
        AdditionalValues = new Dictionary<string, decimal>();
    }

    /// <summary>
    /// Create a simple technical indicator
    /// </summary>
    public static TechnicalIndicator Create(
        StockCode stockCode,
        IndicatorType type,
        CandleInterval interval,
        DateTime timestamp,
        decimal value,
        Dictionary<string, string>? parameters = null)
    {
        var id = new TechnicalIndicatorId(Guid.NewGuid());
        return new TechnicalIndicator(id, stockCode, type, interval, timestamp, value, parameters ?? new Dictionary<string, string>());
    }

    /// <summary>
    /// Create RSI indicator
    /// </summary>
    public static TechnicalIndicator CreateRSI(
        StockCode stockCode,
        CandleInterval interval,
        DateTime timestamp,
        decimal rsiValue,
        int period = 14)
    {
        var parameters = new Dictionary<string, string> { { "period", period.ToString() } };
        var indicator = Create(stockCode, IndicatorType.RSI, interval, timestamp, rsiValue, parameters);

        // Interpret signal
        if (rsiValue < 30)
            indicator.Signal = SignalInterpretation.Bullish; // Oversold
        else if (rsiValue > 70)
            indicator.Signal = SignalInterpretation.Bearish; // Overbought
        else
            indicator.Signal = SignalInterpretation.Neutral;

        return indicator;
    }

    /// <summary>
    /// Create MACD indicator
    /// </summary>
    public static TechnicalIndicator CreateMACD(
        StockCode stockCode,
        CandleInterval interval,
        DateTime timestamp,
        decimal macdLine,
        decimal signalLine,
        decimal histogram)
    {
        var parameters = new Dictionary<string, string>
        {
            { "fast", "12" },
            { "slow", "26" },
            { "signal", "9" }
        };

        var indicator = Create(stockCode, IndicatorType.MACD, interval, timestamp, macdLine, parameters);
        indicator.AdditionalValues["signalLine"] = signalLine;
        indicator.AdditionalValues["histogram"] = histogram;

        // Interpret signal
        if (histogram > 0 && macdLine > signalLine)
            indicator.Signal = SignalInterpretation.Bullish;
        else if (histogram < 0 && macdLine < signalLine)
            indicator.Signal = SignalInterpretation.Bearish;
        else
            indicator.Signal = SignalInterpretation.Neutral;

        return indicator;
    }

    /// <summary>
    /// Create Moving Average indicator
    /// </summary>
    public static TechnicalIndicator CreateMovingAverage(
        StockCode stockCode,
        CandleInterval interval,
        DateTime timestamp,
        decimal maValue,
        int period,
        MovingAverageType maType = MovingAverageType.Simple)
    {
        var parameters = new Dictionary<string, string>
        {
            { "period", period.ToString() },
            { "type", maType.ToString() }
        };

        return Create(stockCode, IndicatorType.MovingAverage, interval, timestamp, maValue, parameters);
    }

    /// <summary>
    /// Create Bollinger Bands indicator
    /// </summary>
    public static TechnicalIndicator CreateBollingerBands(
        StockCode stockCode,
        CandleInterval interval,
        DateTime timestamp,
        decimal middleBand,
        decimal upperBand,
        decimal lowerBand,
        int period = 20,
        decimal stdDev = 2)
    {
        var parameters = new Dictionary<string, string>
        {
            { "period", period.ToString() },
            { "stdDev", stdDev.ToString() }
        };

        var indicator = Create(stockCode, IndicatorType.BollingerBands, interval, timestamp, middleBand, parameters);
        indicator.AdditionalValues["upper"] = upperBand;
        indicator.AdditionalValues["lower"] = lowerBand;

        return indicator;
    }

    /// <summary>
    /// Add an additional value to the indicator
    /// </summary>
    public void AddValue(string key, decimal value)
    {
        AdditionalValues[key] = value;
        MarkAsModified();
    }

    /// <summary>
    /// Set signal interpretation
    /// </summary>
    public void SetSignal(SignalInterpretation signal)
    {
        Signal = signal;
        MarkAsModified();
    }

    /// <summary>
    /// Check if indicator suggests bullish signal
    /// </summary>
    public bool IsBullish() => Signal == SignalInterpretation.Bullish;

    /// <summary>
    /// Check if indicator suggests bearish signal
    /// </summary>
    public bool IsBearish() => Signal == SignalInterpretation.Bearish;

    /// <summary>
    /// Check if indicator is neutral
    /// </summary>
    public bool IsNeutral() => Signal == SignalInterpretation.Neutral || Signal == null;
}

/// <summary>
/// description : 기술적 지표 고유 식별자 (Technical Indicator ID)
/// Details : TechnicalIndicator Entity의 고유 식별자로 GUID 기반 Value Object입니다.
/// Applied technology patterns : DDD Identity Pattern, Value Object Pattern
/// </summary>
public sealed class TechnicalIndicatorId : GuidId
{
    public TechnicalIndicatorId(Guid value) : base(value) { }

    public static TechnicalIndicatorId New() => new(Guid.NewGuid());
}

/// <summary>
/// Indicator Type enumeration
/// </summary>
public enum IndicatorType
{
    RSI = 1,                    // Relative Strength Index
    MACD = 2,                   // Moving Average Convergence Divergence
    MovingAverage = 3,          // Simple/Exponential Moving Average
    BollingerBands = 4,         // Bollinger Bands
    Stochastic = 5,             // Stochastic Oscillator
    ATR = 6,                    // Average True Range
    ADX = 7,                    // Average Directional Index
    CCI = 8,                    // Commodity Channel Index
    Williams = 9,               // Williams %R
    MFI = 10,                   // Money Flow Index
    OBV = 11,                   // On Balance Volume
    IchimokuCloud = 12,         // Ichimoku Cloud
    ParabolicSAR = 13,          // Parabolic SAR
    Momentum = 14,              // Momentum
    ROC = 15                    // Rate of Change
}

/// <summary>
/// Moving Average Type
/// </summary>
public enum MovingAverageType
{
    Simple = 1,                 // SMA
    Exponential = 2,            // EMA
    Weighted = 3,               // WMA
    Hull = 4                    // HMA
}

/// <summary>
/// Signal Interpretation
/// </summary>
public enum SignalInterpretation
{
    Bullish = 1,                // Buy signal
    Bearish = 2,                // Sell signal
    Neutral = 3                 // No clear signal
}
