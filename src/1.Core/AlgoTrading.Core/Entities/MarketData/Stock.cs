using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.MarketData;

/// <summary>
/// description : 주식 종목 정보 Aggregate Root (Stock Entity)
/// Details : 한국 주식시장의 거래 가능한 종목 정보를 관리하는 Aggregate Root입니다. 종목코드(StockCode), 종목명, 상장/거래 상태, 가격 제한폭(상한가/하한가), 호가단위 등의 정보를 포함합니다. 종목의 생명주기(상장, 거래정지, 상장폐지)를 관리하며, Domain Event를 통해 상태 변경을 알립니다.
/// Applied technology patterns : DDD Aggregate Root Pattern, Value Object Pattern (StockCode, Money), State Machine Pattern (종목 상태 관리), Domain Events Pattern
/// </summary>
public sealed class Stock : AggregateRoot<StockId>
{
    /// <summary>
    /// Stock code (e.g., "005930" for Samsung Electronics)
    /// </summary>
    public StockCode Code { get; private set; }

    /// <summary>
    /// Stock name (e.g., "삼성전자")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Market where the stock is traded
    /// </summary>
    public string Market { get; private set; }

    /// <summary>
    /// Industry sector
    /// </summary>
    public string Sector { get; private set; }

    /// <summary>
    /// Stock status (Active, Suspended, Delisted)
    /// </summary>
    public StockStatus Status { get; private set; }

    /// <summary>
    /// Whether the stock is tradeable
    /// </summary>
    public bool IsTradeable { get; private set; }

    /// <summary>
    /// Listing date
    /// </summary>
    public DateTime ListingDate { get; private set; }

    /// <summary>
    /// Total shares outstanding
    /// </summary>
    public long SharesOutstanding { get; private set; }

    /// <summary>
    /// Market capitalization
    /// </summary>
    public Money? MarketCap { get; private set; }

    /// <summary>
    /// Previous closing price
    /// </summary>
    public decimal? PreviousClosePrice { get; private set; }

    /// <summary>
    /// Upper price limit (상한가)
    /// </summary>
    public decimal? UpperPriceLimit { get; private set; }

    /// <summary>
    /// Lower price limit (하한가)
    /// </summary>
    public decimal? LowerPriceLimit { get; private set; }

    /// <summary>
    /// Tick size (호가단위)
    /// </summary>
    public decimal TickSize { get; private set; }

    private Stock() { } // EF Core

    private Stock(
        StockId id,
        StockCode code,
        string name,
        string market,
        string sector,
        DateTime listingDate)
    {
        Id = id;
        Code = code;
        Name = name;
        Market = market;
        Sector = sector;
        ListingDate = listingDate;
        Status = StockStatus.Active;
        IsTradeable = true;
        TickSize = 1;
    }

    /// <summary>
    /// Create a new Stock
    /// </summary>
    public static Stock Create(
        StockCode code,
        string name,
        string market,
        string sector,
        DateTime listingDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Stock name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(market))
            throw new ArgumentException("Market cannot be empty", nameof(market));

        var stockId = new StockId(Guid.NewGuid());
        return new Stock(stockId, code, name, market, sector, listingDate);
    }

    /// <summary>
    /// Update price limits
    /// </summary>
    public void UpdatePriceLimits(decimal previousClose, decimal upperLimit, decimal lowerLimit)
    {
        if (upperLimit <= previousClose)
            throw new ArgumentException("Upper limit must be greater than previous close");

        if (lowerLimit >= previousClose)
            throw new ArgumentException("Lower limit must be less than previous close");

        PreviousClosePrice = previousClose;
        UpperPriceLimit = upperLimit;
        LowerPriceLimit = lowerLimit;
        MarkAsModified();
    }

    /// <summary>
    /// Update market capitalization
    /// </summary>
    public void UpdateMarketCap(Money marketCap)
    {
        MarketCap = marketCap;
        MarkAsModified();
    }

    /// <summary>
    /// Update shares outstanding
    /// </summary>
    public void UpdateSharesOutstanding(long shares)
    {
        if (shares <= 0)
            throw new ArgumentException("Shares outstanding must be positive", nameof(shares));

        SharesOutstanding = shares;
        MarkAsModified();
    }

    /// <summary>
    /// Suspend trading
    /// </summary>
    public void Suspend(string reason)
    {
        Status = StockStatus.Suspended;
        IsTradeable = false;
        MarkAsModified();
    }

    /// <summary>
    /// Resume trading
    /// </summary>
    public void Resume()
    {
        Status = StockStatus.Active;
        IsTradeable = true;
        MarkAsModified();
    }

    /// <summary>
    /// Delist the stock
    /// </summary>
    public void Delist(DateTime delistingDate)
    {
        Status = StockStatus.Delisted;
        IsTradeable = false;
        MarkAsModified();
    }

    /// <summary>
    /// Update tick size based on price range
    /// </summary>
    public void UpdateTickSize(decimal currentPrice)
    {
        // Korean market tick size rules
        TickSize = currentPrice switch
        {
            < 1000 => 1,
            < 5000 => 5,
            < 10000 => 10,
            < 50000 => 50,
            < 100000 => 100,
            < 500000 => 500,
            _ => 1000
        };
        MarkAsModified();
    }
}

/// <summary>
/// description : 주식 종목 고유 식별자 (Stock ID)
/// Details : Stock Entity의 고유 식별자로 GUID 기반 Value Object입니다.
/// Applied technology patterns : DDD Identity Pattern, Value Object Pattern
/// </summary>
public sealed class StockId : GuidId
{
    public StockId(Guid value) : base(value) { }

    public static StockId New() => new(Guid.NewGuid());
    public static StockId From(Guid value) => new(value);
}

/// <summary>
/// Stock Status enumeration
/// </summary>
public enum StockStatus
{
    Active = 1,
    Suspended = 2,
    Delisted = 3,
    HaltedTrading = 4
}
