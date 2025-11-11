using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.MarketData;

/// <summary>
/// description(설명) : Stock Info Aggregate Root representing stock basic information (종목 기본 정보를 나타내는 주식 정보 집합 루트)
/// Details(상세설명) : Manages stock metadata including sector, industry, listing info (섹터, 업종, 상장 정보 등 종목 메타데이터 관리)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern, Domain-Driven Design (집합 루트 패턴, 도메인 주도 설계)
/// </summary>
public sealed class StockInfo : AggregateRoot<StockCode>
{
    /// <summary>
    /// 종목 이름 (예: 삼성전자)
    /// </summary>
    public string StockName { get; private set; }

    /// <summary>
    /// 시장 구분 (KOSPI/KOSDAQ/KONEX)
    /// </summary>
    public MarketType MarketType { get; private set; }

    /// <summary>
    /// 섹터 (예: IT, 금융, 화학)
    /// </summary>
    public string? Sector { get; private set; }

    /// <summary>
    /// 업종 (예: 반도체, 은행, 제약)
    /// </summary>
    public string? Industry { get; private set; }

    /// <summary>
    /// 상장일
    /// </summary>
    public DateTime? ListingDate { get; private set; }

    /// <summary>
    /// 상장 주식수 (발행주식수)
    /// </summary>
    public long? ListedShares { get; private set; }

    /// <summary>
    /// 거래 가능 여부
    /// </summary>
    public bool IsTradable { get; private set; }

    /// <summary>
    /// 거래 정지 여부
    /// </summary>
    public bool IsSuspended { get; private set; }

    /// <summary>
    /// 상한가 (전일 종가 기준 +30%)
    /// </summary>
    public Money? UpperLimitPrice { get; private set; }

    /// <summary>
    /// 하한가 (전일 종가 기준 -30%)
    /// </summary>
    public Money? LowerLimitPrice { get; private set; }

    /// <summary>
    /// 정보 최종 수정 일시
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    private StockInfo() { }

    private StockInfo(
        StockCode stockCode,
        string stockName,
        MarketType marketType)
    {
        Id = stockCode;
        StockName = stockName;
        MarketType = marketType;
        IsTradable = true;
        IsSuspended = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public static StockInfo Create(string stockCode, string stockName, MarketType marketType)
    {
        if (string.IsNullOrWhiteSpace(stockCode))
            throw new ArgumentException("Stock code cannot be empty", nameof(stockCode));
        if (string.IsNullOrWhiteSpace(stockName))
            throw new ArgumentException("Stock name cannot be empty", nameof(stockName));

        return new StockInfo(new StockCode(stockCode), stockName, marketType);
    }

    public void UpdateInfo(string? sector, string? industry, DateTime? listingDate, long? listedShares)
    {
        Sector = sector;
        Industry = industry;
        ListingDate = listingDate;
        ListedShares = listedShares;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    public void UpdateLimitPrices(Money? upperLimit, Money? lowerLimit)
    {
        UpperLimitPrice = upperLimit;
        LowerLimitPrice = lowerLimit;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    public void Suspend()
    {
        IsSuspended = true;
        IsTradable = false;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    public void Resume()
    {
        IsSuspended = false;
        IsTradable = true;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }
}

public enum MarketType
{
    KOSPI = 1,
    KOSDAQ = 2,
    KONEX = 3,
    FUTURES = 4,
    OPTIONS = 5
}
