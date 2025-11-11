using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Portfolio;

/// <summary>
/// description(설명) : Performance Record entity for tracking account performance metrics (계좌 성과 지표를 추적하는 성과 기록 엔티티)
/// Details(상세설명) : Time-series data with risk metrics like Sharpe ratio, drawdown, win rate (샤프비율, 드로다운, 승률 등의 리스크 지표를 포함한 시계열 데이터)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, Time-Series Pattern (엔티티 패턴, 시계열 패턴)
/// </summary>
public sealed class PerformanceRecord : Entity<PerformanceRecordId>
{
    /// <summary>
    /// 계좌 ID
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// 성과 기록 일시
    /// </summary>
    public DateTime RecordDate { get; private set; }

    /// <summary>
    /// 자기자본 가치 (총자산 - 부채)
    /// </summary>
    public Money EquityValue { get; private set; }

    /// <summary>
    /// 일별 수익 금액
    /// </summary>
    public Money DailyReturn { get; private set; }

    /// <summary>
    /// 일별 수익률 (%)
    /// </summary>
    public decimal DailyReturnPercent { get; private set; }

    /// <summary>
    /// 누적 수익 금액
    /// </summary>
    public Money? CumulativeReturn { get; private set; }

    /// <summary>
    /// 누적 수익률 (%)
    /// </summary>
    public decimal? CumulativeReturnPercent { get; private set; }

    /// <summary>
    /// 샤프 비율 (위험 대비 수익률)
    /// </summary>
    public decimal? SharpeRatio { get; private set; }

    /// <summary>
    /// 최대 낙폭 (%) (고점 대비 최대 하락률)
    /// </summary>
    public decimal? MaxDrawdown { get; private set; }

    /// <summary>
    /// 승률 (%) (수익 거래 / 전체 거래)
    /// </summary>
    public decimal? WinRate { get; private set; }

    /// <summary>
    /// 기록 생성 일시
    /// </summary>
    public DateTime RecordedAt { get; private set; }

    private PerformanceRecord() { }

    private PerformanceRecord(
        PerformanceRecordId id,
        AccountId accountId,
        DateTime recordDate,
        Money equityValue,
        Money dailyReturn,
        decimal dailyReturnPercent)
    {
        Id = id;
        AccountId = accountId;
        RecordDate = recordDate;
        EquityValue = equityValue;
        DailyReturn = dailyReturn;
        DailyReturnPercent = dailyReturnPercent;
        RecordedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// description(설명) : Create a performance record (성과 기록 생성)
    /// Details(상세설명) : Factory method to create a daily performance snapshot (일일 성과 스냅샷을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static PerformanceRecord Create(
        AccountId accountId,
        DateTime recordDate,
        Money equityValue,
        Money dailyReturn,
        decimal dailyReturnPercent)
    {
        var id = PerformanceRecordId.New();
        return new PerformanceRecord(
            id,
            accountId,
            recordDate,
            equityValue,
            dailyReturn,
            dailyReturnPercent);
    }

    /// <summary>
    /// description(설명) : Update cumulative metrics (누적 지표 업데이트)
    /// Details(상세설명) : Updates cumulative return and return percent (누적 수익 및 수익률 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateCumulativeMetrics(Money cumulativeReturn, decimal cumulativeReturnPercent)
    {
        CumulativeReturn = cumulativeReturn;
        CumulativeReturnPercent = cumulativeReturnPercent;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update risk metrics (리스크 지표 업데이트)
    /// Details(상세설명) : Updates Sharpe ratio, max drawdown, and win rate (샤프비율, 최대 낙폭, 승률 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateRiskMetrics(decimal? sharpeRatio, decimal? maxDrawdown, decimal? winRate)
    {
        SharpeRatio = sharpeRatio;
        MaxDrawdown = maxDrawdown;
        WinRate = winRate;
        MarkAsModified();
    }
}

/// <summary>
/// description(설명) : Performance Record ID value object (성과 기록 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for PerformanceRecord (PerformanceRecord의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class PerformanceRecordId : GuidId
{
    public PerformanceRecordId(Guid value) : base(value) { }

    public static PerformanceRecordId New() => new(Guid.NewGuid());
}
