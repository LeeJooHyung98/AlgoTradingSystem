using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.MarketData;

/// <summary>
/// description : 캔들(OHLC) 데이터 엔티티 (Candle Entity)
/// Details : 특정 시간 간격(1분, 5분, 1시간, 1일 등)의 OHLC(시가, 고가, 저가, 종가) 데이터를 관리하는 Entity입니다. 거래량, 거래대금, VWAP(거래량 가중 평균 가격)를 포함하며, 캔들 패턴 분석 메서드(Doji, Hammer 등)를 제공합니다. TimescaleDB에 최적화된 시계열 데이터 저장을 지원합니다.
/// Applied technology patterns : DDD Entity Pattern, Value Object Pattern (StockCode, Money), Time-Series Data Pattern, Technical Analysis Pattern, Market Data Context
/// </summary>
public sealed class Candle : Entity<CandleId>
{
    /// <summary>
    /// Stock code
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// Time interval (1m, 5m, 15m, 1h, 1d, etc.)
    /// </summary>
    public CandleInterval Interval { get; private set; }

    /// <summary>
    /// Candle start timestamp
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// Opening price
    /// </summary>
    public decimal Open { get; private set; }

    /// <summary>
    /// Highest price
    /// </summary>
    public decimal High { get; private set; }

    /// <summary>
    /// Lowest price
    /// </summary>
    public decimal Low { get; private set; }

    /// <summary>
    /// Closing price
    /// </summary>
    public decimal Close { get; private set; }

    /// <summary>
    /// Trading volume
    /// </summary>
    public long Volume { get; private set; }

    /// <summary>
    /// Trading value (amount)
    /// </summary>
    public Money Value { get; private set; }

    /// <summary>
    /// Volume Weighted Average Price
    /// </summary>
    public decimal VWAP { get; private set; }

    /// <summary>
    /// Number of trades
    /// </summary>
    public int TradeCount { get; private set; }

    /// <summary>
    /// Whether the candle is closed (complete)
    /// </summary>
    public bool IsClosed { get; private set; }

    private Candle() { } // EF Core

    private Candle(
        CandleId id,
        StockCode stockCode,
        CandleInterval interval,
        DateTime timestamp,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume,
        Money value)
    {
        Id = id;
        StockCode = stockCode;
        Interval = interval;
        Timestamp = timestamp;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
        Value = value;
        IsClosed = false;
        TradeCount = 0;

        CalculateVWAP();
    }

    /// <summary>
    /// Create a new candle
    /// </summary>
    public static Candle Create(
        StockCode stockCode,
        CandleInterval interval,
        DateTime timestamp,
        decimal openPrice)
    {
        var id = new CandleId(Guid.NewGuid());
        var money = new Money(0);

        return new Candle(
            id,
            stockCode,
            interval,
            timestamp,
            openPrice,
            openPrice,
            openPrice,
            openPrice,
            0,
            money);
    }

    /// <summary>
    /// Update candle with new tick
    /// </summary>
    public void UpdateWithTick(decimal price, long volume, Money value)
    {
        if (IsClosed)
            throw new InvalidOperationException("Cannot update a closed candle");

        // Update OHLC
        if (price > High)
            High = price;
        if (price < Low)
            Low = price;
        Close = price;

        // Update volume and value
        Volume += volume;
        Value += value;
        TradeCount++;

        CalculateVWAP();
        MarkAsModified();
    }

    /// <summary>
    /// Close the candle (mark as complete)
    /// </summary>
    public void CloseCandle()
    {
        IsClosed = true;
        MarkAsModified();
    }

    /// <summary>
    /// Calculate Volume Weighted Average Price
    /// </summary>
    private void CalculateVWAP()
    {
        VWAP = Volume > 0 ? Value.Amount / Volume : Close;
    }

    /// <summary>
    /// Get candle body size (Close - Open)
    /// </summary>
    public decimal GetBodySize()
    {
        return Close - Open;
    }

    /// <summary>
    /// Get candle body size as percentage
    /// </summary>
    public decimal GetBodySizePercent()
    {
        return Open != 0 ? ((Close - Open) / Open) * 100 : 0;
    }

    /// <summary>
    /// Get upper wick/shadow length (High - Max(Open, Close))
    /// </summary>
    public decimal GetUpperWickLength()
    {
        return High - Math.Max(Open, Close);
    }

    /// <summary>
    /// Get lower wick/shadow length (Min(Open, Close) - Low)
    /// </summary>
    public decimal GetLowerWickLength()
    {
        return Math.Min(Open, Close) - Low;
    }

    /// <summary>
    /// Get total candle range (High - Low)
    /// </summary>
    public decimal GetRange()
    {
        return High - Low;
    }

    /// <summary>
    /// Check if it's a bullish candle (Close > Open)
    /// </summary>
    public bool IsBullish()
    {
        return Close > Open;
    }

    /// <summary>
    /// Check if it's a bearish candle (Close < Open)
    /// </summary>
    public bool IsBearish()
    {
        return Close < Open;
    }

    /// <summary>
    /// Check if it's a doji candle (Close ≈ Open)
    /// </summary>
    public bool IsDoji(decimal threshold = 0.001m)
    {
        return Math.Abs(Close - Open) / Open < threshold;
    }

    /// <summary>
    /// Check if it's a hammer pattern
    /// </summary>
    public bool IsHammer()
    {
        var body = Math.Abs(GetBodySize());
        var lowerWick = GetLowerWickLength();
        var upperWick = GetUpperWickLength();

        return lowerWick >= 2 * body && upperWick <= body / 2;
    }

    /// <summary>
    /// Check if it's an inverted hammer pattern
    /// </summary>
    public bool IsInvertedHammer()
    {
        var body = Math.Abs(GetBodySize());
        var lowerWick = GetLowerWickLength();
        var upperWick = GetUpperWickLength();

        return upperWick >= 2 * body && lowerWick <= body / 2;
    }
}

/// <summary>
/// description : 캔들 고유 식별자 (Candle ID)
/// Details : Candle Entity의 고유 식별자로 GUID 기반 Value Object입니다.
/// Applied technology patterns : DDD Identity Pattern, Value Object Pattern
/// </summary>
public sealed class CandleId : GuidId
{
    public CandleId(Guid value) : base(value) { }

    public static CandleId New() => new(Guid.NewGuid());
}

/// <summary>
/// Candle Interval enumeration
/// </summary>
public enum CandleInterval
{
    Tick = 0,           // 틱
    OneMinute = 1,      // 1분봉
    ThreeMinutes = 3,   // 3분봉
    FiveMinutes = 5,    // 5분봉
    TenMinutes = 10,    // 10분봉
    FifteenMinutes = 15,// 15분봉
    ThirtyMinutes = 30, // 30분봉
    OneHour = 60,       // 1시간봉
    FourHours = 240,    // 4시간봉
    OneDay = 1440,      // 일봉
    OneWeek = 10080,    // 주봉
    OneMonth = 43200    // 월봉
}
