using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.MarketData;

/// <summary>
/// description(설명) : Tick Data entity for individual trade execution records (개별 체결 거래 기록을 위한 틱 데이터 엔티티)
/// Details(상세설명) : Time-series data with every trade execution, stored in TimescaleDB Hypertable (TimescaleDB 하이퍼테이블에 저장되는 모든 체결 거래의 시계열 데이터)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, Time-Series Pattern (엔티티 패턴, 시계열 패턴)
/// </summary>
public sealed class TickData : Entity<TickDataId>
{
    /// <summary>
    /// 종목 코드
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// 체결 가격
    /// </summary>
    public Money ExecutionPrice { get; private set; }

    /// <summary>
    /// 체결 수량 (주)
    /// </summary>
    public long ExecutionVolume { get; private set; }

    /// <summary>
    /// 체결 구분 (ASK: 매도체결, BID: 매수체결)
    /// </summary>
    public string ExecutionSide { get; private set; }

    /// <summary>
    /// 체결 시각 (TimescaleDB 시계열 기준)
    /// </summary>
    public DateTime TickTimestamp { get; private set; }

    private TickData() { }

    private TickData(
        TickDataId id,
        StockCode stockCode,
        Money executionPrice,
        long executionVolume,
        string executionSide,
        DateTime tickTimestamp)
    {
        Id = id;
        StockCode = stockCode;
        ExecutionPrice = executionPrice;
        ExecutionVolume = executionVolume;
        ExecutionSide = executionSide;
        TickTimestamp = tickTimestamp;
    }

    /// <summary>
    /// description(설명) : Create tick data record (틱 데이터 레코드 생성)
    /// Details(상세설명) : Factory method to create trade execution record (체결 거래 레코드를 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static TickData Create(
        StockCode stockCode,
        Money executionPrice,
        long executionVolume,
        string executionSide,
        DateTime tickTimestamp)
    {
        if (executionVolume <= 0)
            throw new ArgumentException("Execution volume must be positive", nameof(executionVolume));

        if (executionPrice.Amount <= 0)
            throw new ArgumentException("Execution price must be positive", nameof(executionPrice));

        if (string.IsNullOrWhiteSpace(executionSide))
            throw new ArgumentException("Execution side cannot be empty", nameof(executionSide));

        if (executionSide != "ASK" && executionSide != "BID")
            throw new ArgumentException("Execution side must be ASK or BID", nameof(executionSide));

        var id = TickDataId.New();
        return new TickData(
            id,
            stockCode,
            executionPrice,
            executionVolume,
            executionSide,
            tickTimestamp);
    }

    /// <summary>
    /// description(설명) : Check if tick is ask-side (매도 체결인지 확인)
    /// Details(상세설명) : Returns true if execution side is ASK (체결 구분이 ASK이면 true 반환)
    /// </summary>
    public bool IsAskSide()
    {
        return ExecutionSide == "ASK";
    }

    /// <summary>
    /// description(설명) : Check if tick is bid-side (매수 체결인지 확인)
    /// Details(상세설명) : Returns true if execution side is BID (체결 구분이 BID이면 true 반환)
    /// </summary>
    public bool IsBidSide()
    {
        return ExecutionSide == "BID";
    }

    /// <summary>
    /// description(설명) : Calculate execution value (체결 금액 계산)
    /// Details(상세설명) : Returns the total value of this execution (이 체결의 총 금액 반환)
    /// </summary>
    public Money GetExecutionValue()
    {
        var totalAmount = ExecutionPrice.Amount * ExecutionVolume;
        return new Money(totalAmount, ExecutionPrice.Currency);
    }
}

/// <summary>
/// description(설명) : Tick Data ID value object (틱 데이터 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for TickData (TickData의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class TickDataId : GuidId
{
    public TickDataId(Guid value) : base(value) { }

    public static TickDataId New() => new(Guid.NewGuid());
}
