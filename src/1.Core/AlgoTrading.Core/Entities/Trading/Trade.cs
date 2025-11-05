using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Trading;

/// <summary>
/// description(설명) : Trade Entity - Represents a completed trade with entry and exit (완료된 거래 엔티티 - 진입과 청산을 포함)
/// Details(상세설명) : Aggregates order executions into completed trades for performance analysis and record keeping (주문 체결을 완료된 거래로 집계하여 성과 분석 및 기록 관리)
/// Applied technology patterns(적용기술패턴) : Entity Pattern (엔티티 패턴), Domain-Driven Design (도메인 주도 설계), Factory Pattern (팩토리 패턴)
/// </summary>
public sealed class Trade : Entity<TradeId>
{
    /// <summary>
    /// 종목 코드
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// 거래 방향 (롱/숏)
    /// </summary>
    public PositionSide Side { get; private set; }

    /// <summary>
    /// 진입 주문 ID
    /// </summary>
    public OrderId EntryOrderId { get; private set; }

    /// <summary>
    /// 청산 주문 ID
    /// </summary>
    public OrderId ExitOrderId { get; private set; }

    /// <summary>
    /// 거래 수량
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// 진입 가격
    /// </summary>
    public decimal EntryPrice { get; private set; }

    /// <summary>
    /// 청산 가격
    /// </summary>
    public decimal ExitPrice { get; private set; }

    /// <summary>
    /// 진입 시간
    /// </summary>
    public DateTime EntryTime { get; private set; }

    /// <summary>
    /// 청산 시간
    /// </summary>
    public DateTime ExitTime { get; private set; }

    /// <summary>
    /// 실현 손익
    /// </summary>
    public Money RealizedPL { get; private set; }

    /// <summary>
    /// 실현 손익률 (%)
    /// </summary>
    public decimal RealizedPLPercent { get; private set; }

    /// <summary>
    /// 수수료
    /// </summary>
    public Money Commission { get; private set; }

    /// <summary>
    /// 거래 기간
    /// </summary>
    public TimeSpan Duration => ExitTime - EntryTime;

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; private set; }

    /// <summary>
    /// 전략 ID (전략 거래인 경우)
    /// </summary>
    public Guid? StrategyId { get; private set; }

    /// <summary>
    /// 거래 결과 (수익/손실/본전)
    /// </summary>
    public TradeResult Result { get; private set; }

    /// <summary>
    /// 메모 또는 태그
    /// </summary>
    public string? Notes { get; private set; }

    private Trade() { } // EF Core

    private Trade(
        TradeId id,
        StockCode stockCode,
        PositionSide side,
        OrderId entryOrderId,
        OrderId exitOrderId,
        int quantity,
        decimal entryPrice,
        decimal exitPrice,
        DateTime entryTime,
        DateTime exitTime,
        Money commission,
        string accountNumber,
        Guid? strategyId = null)
    {
        Id = id;
        StockCode = stockCode;
        Side = side;
        EntryOrderId = entryOrderId;
        ExitOrderId = exitOrderId;
        Quantity = quantity;
        EntryPrice = entryPrice;
        ExitPrice = exitPrice;
        EntryTime = entryTime;
        ExitTime = exitTime;
        Commission = commission;
        AccountNumber = accountNumber;
        StrategyId = strategyId;

        CalculateProfitLoss();
        DetermineResult();
    }

    /// <summary>
    /// description(설명) : Create a completed trade (완료된 거래 생성)
    /// Details(상세설명) : Factory method to create a trade record after both entry and exit orders are filled (진입과 청산 주문이 모두 체결된 후 거래 기록 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created Trade instance (새로 생성된 거래 인스턴스)</returns>
    public static Trade Create(
        StockCode stockCode,
        PositionSide side,
        OrderId entryOrderId,
        OrderId exitOrderId,
        int quantity,
        decimal entryPrice,
        decimal exitPrice,
        DateTime entryTime,
        DateTime exitTime,
        Money commission,
        string accountNumber,
        Guid? strategyId = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (entryPrice <= 0)
            throw new ArgumentException("Entry price must be positive", nameof(entryPrice));

        if (exitPrice <= 0)
            throw new ArgumentException("Exit price must be positive", nameof(exitPrice));

        if (exitTime <= entryTime)
            throw new ArgumentException("Exit time must be after entry time");

        var id = new TradeId(Guid.NewGuid());
        return new Trade(
            id,
            stockCode,
            side,
            entryOrderId,
            exitOrderId,
            quantity,
            entryPrice,
            exitPrice,
            entryTime,
            exitTime,
            commission,
            accountNumber,
            strategyId);
    }

    /// <summary>
    /// description(설명) : Add notes to the trade (거래에 메모 추가)
    /// Details(상세설명) : Allows adding descriptive notes or tags for trade analysis and review (거래 분석 및 검토를 위한 설명 메모나 태그 추가)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void AddNotes(string notes)
    {
        Notes = notes;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update commission and recalculate P/L (수수료 업데이트 및 손익 재계산)
    /// Details(상세설명) : Updates commission amount and recalculates realized profit/loss accordingly (수수료 금액 업데이트 후 실현 손익을 재계산)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateCommission(Money commission)
    {
        Commission = commission;
        CalculateProfitLoss();
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if trade is profitable (거래가 수익인지 확인)
    /// Details(상세설명) : Returns true if the realized P/L is positive (실현 손익이 양수인 경우 true 반환)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>True if trade is profitable (거래가 수익인 경우 true)</returns>
    public bool IsWinner()
    {
        return Result == TradeResult.Win;
    }

    /// <summary>
    /// description(설명) : Check if trade is a loss (거래가 손실인지 확인)
    /// Details(상세설명) : Returns true if the realized P/L is negative (실현 손익이 음수인 경우 true 반환)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>True if trade is a loss (거래가 손실인 경우 true)</returns>
    public bool IsLoser()
    {
        return Result == TradeResult.Loss;
    }

    /// <summary>
    /// description(설명) : Check if it's a day trade (당일 매매인지 확인)
    /// Details(상세설명) : Returns true if entry and exit occur on the same date (진입과 청산이 같은 날짜에 발생한 경우 true 반환)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>True if entry and exit are on same date (진입과 청산이 같은 날인 경우 true)</returns>
    public bool IsDayTrade()
    {
        return EntryTime.Date == ExitTime.Date;
    }

    /// <summary>
    /// description(설명) : Get gross profit/loss before commission (수수료 차감 전 총 손익 조회)
    /// Details(상세설명) : Calculates profit/loss without deducting commissions based on position side (포지션 방향에 따라 수수료를 차감하지 않은 손익 계산)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Gross profit/loss amount (총 손익 금액)</returns>
    public Money GetGrossPL()
    {
        if (Side == PositionSide.Long)
            return new Money((ExitPrice - EntryPrice) * Quantity);
        else
            return new Money((EntryPrice - ExitPrice) * Quantity);
    }

    /// <summary>
    /// description(설명) : Get trade duration in hours (거래 기간을 시간 단위로 조회)
    /// Details(상세설명) : Returns the total hours between entry and exit times (진입과 청산 사이의 총 시간 반환)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Duration in hours (시간 단위 기간)</returns>
    public double GetDurationInHours()
    {
        return Duration.TotalHours;
    }

    /// <summary>
    /// description(설명) : Get trade duration in days (거래 기간을 일 단위로 조회)
    /// Details(상세설명) : Returns the total days between entry and exit times (진입과 청산 사이의 총 일수 반환)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Duration in days (일 단위 기간)</returns>
    public double GetDurationInDays()
    {
        return Duration.TotalDays;
    }

    private void CalculateProfitLoss()
    {
        var grossPL = GetGrossPL();
        RealizedPL = grossPL - Commission;

        var costBasis = EntryPrice * Quantity;
        RealizedPLPercent = costBasis > 0 ? (RealizedPL.Amount / costBasis) * 100 : 0;
    }

    private void DetermineResult()
    {
        if (RealizedPL.Amount > 0)
            Result = TradeResult.Win;
        else if (RealizedPL.Amount < 0)
            Result = TradeResult.Loss;
        else
            Result = TradeResult.Breakeven;
    }
}

/// <summary>
/// description(설명) : Trade identifier value object (거래 식별자 값 객체)
/// Details(상세설명) : Strongly-typed identifier for Trade entity using GUID (GUID를 사용한 거래 엔티티의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class TradeId : GuidId
{
    public TradeId(Guid value) : base(value) { }

    public static TradeId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Trade result enumeration (거래 결과 열거형)
/// Details(상세설명) : Categorizes trade outcomes as win, loss, or breakeven (거래 결과를 수익, 손실, 본전으로 분류)
/// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum TradeResult
{
    Win = 1,        // Profitable trade (수익 거래)
    Loss = 2,       // Losing trade (손실 거래)
    Breakeven = 3   // No profit or loss (본전 거래)
}
