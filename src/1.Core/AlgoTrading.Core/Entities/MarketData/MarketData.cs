using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.MarketData;

/// <summary>
/// description(설명) : Market Data entity for real-time stock price data (실시간 주식 가격 데이터를 위한 시장 데이터 엔티티)
/// Details(상세설명) : Time-series data with OHLC prices, volume, stored in TimescaleDB Hypertable (TimescaleDB 하이퍼테이블에 저장되는 OHLC 가격, 거래량을 포함한 시계열 데이터)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, Time-Series Pattern (엔티티 패턴, 시계열 패턴)
/// </summary>
public sealed class MarketData : Entity<MarketDataId>
{
    /// <summary>
    /// 종목 코드
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// 시가 (장 시작가)
    /// </summary>
    public Money OpenPrice { get; private set; }

    /// <summary>
    /// 고가 (당일 최고가)
    /// </summary>
    public Money HighPrice { get; private set; }

    /// <summary>
    /// 저가 (당일 최저가)
    /// </summary>
    public Money LowPrice { get; private set; }

    /// <summary>
    /// 종가 (현재가 또는 장 마감가)
    /// </summary>
    public Money ClosePrice { get; private set; }

    /// <summary>
    /// 거래량 (주)
    /// </summary>
    public long Volume { get; private set; }

    /// <summary>
    /// 거래 횟수
    /// </summary>
    public int? TradeCount { get; private set; }

    /// <summary>
    /// 전일 대비 변동 금액
    /// </summary>
    public Money? ChangeAmount { get; private set; }

    /// <summary>
    /// 전일 대비 변동률 (%)
    /// </summary>
    public decimal? ChangePercent { get; private set; }

    /// <summary>
    /// 시장 상태 (장 시작 전, 장중, 장 마감 등)
    /// </summary>
    public MarketStatus MarketStatus { get; private set; }

    /// <summary>
    /// 데이터 시각 (TimescaleDB 시계열 기준)
    /// </summary>
    public DateTime DataTimestamp { get; private set; }

    private MarketData() { }

    private MarketData(
        MarketDataId id,
        StockCode stockCode,
        Money openPrice,
        Money highPrice,
        Money lowPrice,
        Money closePrice,
        long volume,
        MarketStatus marketStatus,
        DateTime dataTimestamp)
    {
        Id = id;
        StockCode = stockCode;
        OpenPrice = openPrice;
        HighPrice = highPrice;
        LowPrice = lowPrice;
        ClosePrice = closePrice;
        Volume = volume;
        MarketStatus = marketStatus;
        DataTimestamp = dataTimestamp;
    }

    /// <summary>
    /// description(설명) : Create real-time market data (실시간 시장 데이터 생성)
    /// Details(상세설명) : Factory method to create market data snapshot (시장 데이터 스냅샷을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static MarketData Create(
        StockCode stockCode,
        Money openPrice,
        Money highPrice,
        Money lowPrice,
        Money closePrice,
        long volume,
        MarketStatus marketStatus,
        DateTime dataTimestamp)
    {
        if (volume < 0)
            throw new ArgumentException("Volume cannot be negative", nameof(volume));

        if (highPrice.Amount < lowPrice.Amount)
            throw new ArgumentException("High price cannot be lower than low price");

        if (highPrice.Amount < openPrice.Amount || highPrice.Amount < closePrice.Amount)
            throw new ArgumentException("High price must be the highest among OHLC prices");

        if (lowPrice.Amount > openPrice.Amount || lowPrice.Amount > closePrice.Amount)
            throw new ArgumentException("Low price must be the lowest among OHLC prices");

        var id = MarketDataId.New();
        return new MarketData(
            id,
            stockCode,
            openPrice,
            highPrice,
            lowPrice,
            closePrice,
            volume,
            marketStatus,
            dataTimestamp);
    }

    /// <summary>
    /// description(설명) : Update additional market metrics (추가 시장 지표 업데이트)
    /// Details(상세설명) : Updates trade count, change amount, and change percent (거래 횟수, 변동 금액, 변동률 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateMetrics(int? tradeCount, Money? changeAmount, decimal? changePercent)
    {
        TradeCount = tradeCount;
        ChangeAmount = changeAmount;
        ChangePercent = changePercent;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update market status (시장 상태 업데이트)
    /// Details(상세설명) : Updates the market status (e.g., from PreMarket to Trading) (시장 상태 업데이트, 예: 장 시작 전 → 장중)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void UpdateMarketStatus(MarketStatus newStatus)
    {
        MarketStatus = newStatus;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Calculate price change from previous close (전일 종가 대비 변동 계산)
    /// Details(상세설명) : Calculates the price change and change percent from previous close price (전일 종가 대비 가격 변동 및 등락률 계산)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void CalculateChange(Money previousClosePrice)
    {
        if (previousClosePrice.Amount == 0)
            throw new ArgumentException("Previous close price cannot be zero", nameof(previousClosePrice));

        var change = ClosePrice.Amount - previousClosePrice.Amount;
        ChangePercent = (change / previousClosePrice.Amount) * 100;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if data is intraday (장중 데이터인지 확인)
    /// Details(상세설명) : Returns true if market status indicates active trading hours (시장 상태가 거래 시간임을 나타내면 true 반환)
    /// </summary>
    public bool IsIntraday()
    {
        return MarketStatus == MarketStatus.Trading;
    }
}

/// <summary>
/// description(설명) : Market Data ID value object (시장 데이터 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for MarketData (MarketData의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class MarketDataId : GuidId
{
    public MarketDataId(Guid value) : base(value) { }

    public static MarketDataId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Market Status enumeration (시장 상태 열거형)
/// Details(상세설명) : Represents the current market trading status (현재 시장 거래 상태를 나타냄)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum MarketStatus
{
    PreOpen = 1,    // 장 시작 전 (PRE_OPEN)
    Opening = 2,    // 시가 결정 (OPENING)
    Trading = 3,    // 정규장 (TRADING)
    Closing = 4,    // 종가 결정 (CLOSING)
    Closed = 5,     // 장 마감 (CLOSED)
    Halted = 6      // 거래 정지 (HALTED)
}
