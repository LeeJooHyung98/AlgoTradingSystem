using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.MarketData;

/// <summary>
/// description : 실시간 시장 가격 데이터 엔티티 (Real-time Market Price Data Entity)
/// Details : 주식의 실시간 시장 데이터(현재가, 시고저, 거래량, 호가 등)를 관리하는 Entity입니다. Kiwoom OpenAPI를 통해 수신한 실시간 데이터를 저장하며, 가격 변동률, 호가 스프레드, 상한가/하한가 도달 여부 등을 계산합니다. 장 상태(장전, 거래중, 장후 등)도 추적합니다.
/// Applied technology patterns : DDD Entity Pattern, Value Object Pattern (StockCode, Money), Real-time Data Pattern, Market Data Context (Bounded Context)
/// </summary>
public sealed class PriceData : Entity<PriceDataId>
{
    /// <summary>
    /// Stock code
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// Current price
    /// </summary>
    public decimal CurrentPrice { get; private set; }

    /// <summary>
    /// Opening price
    /// </summary>
    public decimal OpenPrice { get; private set; }

    /// <summary>
    /// High price
    /// </summary>
    public decimal HighPrice { get; private set; }

    /// <summary>
    /// Low price
    /// </summary>
    public decimal LowPrice { get; private set; }

    /// <summary>
    /// Previous close price
    /// </summary>
    public decimal PreviousClosePrice { get; private set; }

    /// <summary>
    /// Trading volume
    /// </summary>
    public long Volume { get; private set; }

    /// <summary>
    /// Trading value (amount)
    /// </summary>
    public Money TradingValue { get; private set; }

    /// <summary>
    /// Change amount from previous close
    /// </summary>
    public decimal ChangeAmount { get; private set; }

    /// <summary>
    /// Change rate from previous close (percentage)
    /// </summary>
    public decimal ChangeRate { get; private set; }

    /// <summary>
    /// Best bid price
    /// </summary>
    public decimal? BidPrice { get; private set; }

    /// <summary>
    /// Best ask price
    /// </summary>
    public decimal? AskPrice { get; private set; }

    /// <summary>
    /// Bid quantity
    /// </summary>
    public long? BidQuantity { get; private set; }

    /// <summary>
    /// Ask quantity
    /// </summary>
    public long? AskQuantity { get; private set; }

    /// <summary>
    /// Market status
    /// </summary>
    public MarketStatus MarketStatus { get; private set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; private set; }

    private PriceData() { } // EF Core

    private PriceData(
        PriceDataId id,
        StockCode stockCode,
        decimal currentPrice,
        decimal openPrice,
        decimal highPrice,
        decimal lowPrice,
        decimal previousClosePrice,
        long volume,
        Money tradingValue,
        DateTime timestamp)
    {
        Id = id;
        StockCode = stockCode;
        CurrentPrice = currentPrice;
        OpenPrice = openPrice;
        HighPrice = highPrice;
        LowPrice = lowPrice;
        PreviousClosePrice = previousClosePrice;
        Volume = volume;
        TradingValue = tradingValue;
        Timestamp = timestamp;
        MarketStatus = MarketStatus.Trading;

        CalculateChange();
    }

    /// <summary>
    /// Create new price data
    /// </summary>
    public static PriceData Create(
        StockCode stockCode,
        decimal currentPrice,
        decimal openPrice,
        decimal highPrice,
        decimal lowPrice,
        decimal previousClosePrice,
        long volume,
        Money tradingValue,
        DateTime timestamp)
    {
        var id = new PriceDataId(Guid.NewGuid());
        return new PriceData(
            id,
            stockCode,
            currentPrice,
            openPrice,
            highPrice,
            lowPrice,
            previousClosePrice,
            volume,
            tradingValue,
            timestamp);
    }

    /// <summary>
    /// Update current price
    /// </summary>
    public void UpdatePrice(decimal newPrice, long additionalVolume, Money additionalValue, DateTime timestamp)
    {
        CurrentPrice = newPrice;
        Volume += additionalVolume;
        TradingValue += additionalValue;
        Timestamp = timestamp;

        // Update high/low
        if (newPrice > HighPrice)
            HighPrice = newPrice;
        if (newPrice < LowPrice)
            LowPrice = newPrice;

        CalculateChange();
        MarkAsModified();
    }

    /// <summary>
    /// Update order book (bid/ask)
    /// </summary>
    public void UpdateOrderBook(decimal? bidPrice, long? bidQuantity, decimal? askPrice, long? askQuantity)
    {
        BidPrice = bidPrice;
        BidQuantity = bidQuantity;
        AskPrice = askPrice;
        AskQuantity = askQuantity;
        MarkAsModified();
    }

    /// <summary>
    /// Update market status
    /// </summary>
    public void UpdateMarketStatus(MarketStatus status)
    {
        MarketStatus = status;
        MarkAsModified();
    }

    /// <summary>
    /// Calculate change from previous close
    /// </summary>
    private void CalculateChange()
    {
        ChangeAmount = CurrentPrice - PreviousClosePrice;
        ChangeRate = PreviousClosePrice != 0
            ? (ChangeAmount / PreviousClosePrice) * 100
            : 0;
    }

    /// <summary>
    /// Check if price is at upper limit
    /// </summary>
    public bool IsUpperLimit(decimal upperLimit)
    {
        return Math.Abs(CurrentPrice - upperLimit) < 0.01m;
    }

    /// <summary>
    /// Check if price is at lower limit
    /// </summary>
    public bool IsLowerLimit(decimal lowerLimit)
    {
        return Math.Abs(CurrentPrice - lowerLimit) < 0.01m;
    }

    /// <summary>
    /// Get spread (ask - bid)
    /// </summary>
    public decimal? GetSpread()
    {
        if (AskPrice.HasValue && BidPrice.HasValue)
            return AskPrice.Value - BidPrice.Value;
        return null;
    }

    /// <summary>
    /// Get mid price (average of bid and ask)
    /// </summary>
    public decimal? GetMidPrice()
    {
        if (AskPrice.HasValue && BidPrice.HasValue)
            return (AskPrice.Value + BidPrice.Value) / 2;
        return null;
    }
}

/// <summary>
/// description : 가격 데이터 고유 식별자 (Price Data ID)
/// Details : PriceData Entity의 고유 식별자로 GUID 기반 Value Object입니다.
/// Applied technology patterns : DDD Identity Pattern, Value Object Pattern
/// </summary>
public sealed class PriceDataId : GuidId
{
    public PriceDataId(Guid value) : base(value) { }

    public static PriceDataId New() => new(Guid.NewGuid());
}

/// <summary>
/// Market Status enumeration
/// </summary>
public enum MarketStatus
{
    PreOpen = 1,        // 장 시작 전
    Opening = 2,        // 개시
    Trading = 3,        // 거래 중
    Closing = 4,        // 마감
    AfterHours = 5,     // 장 마감 후
    Halted = 6,         // 거래 정지
    Closed = 7          // 휴장
}
