using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Monitoring;

/// <summary>
/// description(설명) : Trading Metric entity for trading activity monitoring (거래 활동 모니터링을 위한 거래 메트릭 엔티티)
/// Details(상세설명) : Time-series data with order counts, execution rates, P&L stored in TimescaleDB (TimescaleDB에 저장되는 주문 건수, 체결률, 손익을 포함한 시계열 데이터)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, Time-Series Pattern, Monitoring Pattern (엔티티 패턴, 시계열 패턴, 모니터링 패턴)
/// </summary>
public sealed class TradingMetric : Entity<TradingMetricId>
{
    /// <summary>
    /// 계좌 ID
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// 메트릭 수집 시각 (TimescaleDB 시계열 기준)
    /// </summary>
    public DateTime MetricTimestamp { get; private set; }

    /// <summary>
    /// 제출된 주문 수
    /// </summary>
    public int OrdersSubmitted { get; private set; }

    /// <summary>
    /// 체결된 주문 수
    /// </summary>
    public int OrdersFilled { get; private set; }

    /// <summary>
    /// 취소된 주문 수
    /// </summary>
    public int OrdersCancelled { get; private set; }

    /// <summary>
    /// 거부된 주문 수
    /// </summary>
    public int OrdersRejected { get; private set; }

    /// <summary>
    /// 총 체결 금액
    /// </summary>
    public Money? TotalExecutionAmount { get; private set; }

    /// <summary>
    /// 평균 주문 체결 시간 (ms)
    /// </summary>
    public decimal? AvgExecutionTimeMs { get; private set; }

    /// <summary>
    /// 오픈된 포지션 수
    /// </summary>
    public int OpenPositions { get; private set; }

    /// <summary>
    /// 청산된 포지션 수
    /// </summary>
    public int ClosedPositions { get; private set; }

    /// <summary>
    /// 실현 손익
    /// </summary>
    public Money? RealizedPl { get; private set; }

    /// <summary>
    /// 미실현 손익
    /// </summary>
    public Money? UnrealizedPl { get; private set; }

    private TradingMetric() { }

    private TradingMetric(
        TradingMetricId id,
        AccountId accountId,
        DateTime metricTimestamp)
    {
        Id = id;
        AccountId = accountId;
        MetricTimestamp = metricTimestamp;
        OrdersSubmitted = 0;
        OrdersFilled = 0;
        OrdersCancelled = 0;
        OrdersRejected = 0;
        OpenPositions = 0;
        ClosedPositions = 0;
    }

    /// <summary>
    /// description(설명) : Create trading metric record (거래 메트릭 레코드 생성)
    /// Details(상세설명) : Factory method to create trading activity snapshot (거래 활동 스냅샷을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static TradingMetric Create(
        AccountId accountId,
        DateTime metricTimestamp)
    {
        var id = TradingMetricId.New();
        return new TradingMetric(id, accountId, metricTimestamp);
    }

    /// <summary>
    /// description(설명) : Update order counts (주문 건수 업데이트)
    /// Details(상세설명) : Updates submitted, filled, cancelled, and rejected order counts (제출, 체결, 취소, 거부 건수 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateOrderCounts(int ordersSubmitted, int ordersFilled, int ordersCancelled, int ordersRejected)
    {
        if (ordersSubmitted < 0)
            throw new ArgumentException("Orders submitted cannot be negative", nameof(ordersSubmitted));

        if (ordersFilled < 0)
            throw new ArgumentException("Orders filled cannot be negative", nameof(ordersFilled));

        if (ordersCancelled < 0)
            throw new ArgumentException("Orders cancelled cannot be negative", nameof(ordersCancelled));

        if (ordersRejected < 0)
            throw new ArgumentException("Orders rejected cannot be negative", nameof(ordersRejected));

        OrdersSubmitted = ordersSubmitted;
        OrdersFilled = ordersFilled;
        OrdersCancelled = ordersCancelled;
        OrdersRejected = ordersRejected;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update execution metrics (체결 지표 업데이트)
    /// Details(상세설명) : Updates average execution time and total execution amount (평균 체결 시간 및 총 체결 금액 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateExecutionMetrics(decimal? avgExecutionTimeMs, Money? totalExecutionAmount)
    {
        if (avgExecutionTimeMs.HasValue && avgExecutionTimeMs.Value < 0)
            throw new ArgumentException("Average execution time cannot be negative", nameof(avgExecutionTimeMs));

        AvgExecutionTimeMs = avgExecutionTimeMs;
        TotalExecutionAmount = totalExecutionAmount;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update position counts (포지션 수 업데이트)
    /// Details(상세설명) : Updates open and closed position counts (오픈 및 청산 포지션 수 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdatePositionCounts(int openPositions, int closedPositions)
    {
        if (openPositions < 0)
            throw new ArgumentException("Open positions cannot be negative", nameof(openPositions));

        if (closedPositions < 0)
            throw new ArgumentException("Closed positions cannot be negative", nameof(closedPositions));

        OpenPositions = openPositions;
        ClosedPositions = closedPositions;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update profit/loss metrics (손익 지표 업데이트)
    /// Details(상세설명) : Updates realized and unrealized P&L (실현 및 미실현 손익 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateProfitLossMetrics(Money? realizedPl, Money? unrealizedPl)
    {
        RealizedPl = realizedPl;
        UnrealizedPl = unrealizedPl;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Calculate fill rate (체결률 계산)
    /// Details(상세설명) : Returns the percentage of submitted orders that were filled (제출된 주문 중 체결된 비율 반환)
    /// </summary>
    public decimal GetFillRate()
    {
        if (OrdersSubmitted == 0)
            return 0;

        return ((decimal)OrdersFilled / OrdersSubmitted) * 100;
    }

    /// <summary>
    /// description(설명) : Calculate rejection rate (거부율 계산)
    /// Details(상세설명) : Returns the percentage of submitted orders that were rejected (제출된 주문 중 거부된 비율 반환)
    /// </summary>
    public decimal GetRejectionRate()
    {
        if (OrdersSubmitted == 0)
            return 0;

        return ((decimal)OrdersRejected / OrdersSubmitted) * 100;
    }

    /// <summary>
    /// description(설명) : Get total P&L (총 손익 가져오기)
    /// Details(상세설명) : Returns the sum of realized and unrealized P&L (실현 및 미실현 손익의 합계 반환)
    /// </summary>
    public Money? GetTotalProfitLoss()
    {
        if (RealizedPl == null || UnrealizedPl == null)
            return null;

        var totalAmount = RealizedPl.Amount + UnrealizedPl.Amount;
        return new Money(totalAmount, RealizedPl.Currency);
    }

    /// <summary>
    /// description(설명) : Check if trading is performing well (거래가 잘 수행되고 있는지 확인)
    /// Details(상세설명) : Returns true if fill rate > 80% and rejection rate < 5% (체결률 > 80% 및 거부율 < 5%이면 true 반환)
    /// </summary>
    public bool IsPerformingWell()
    {
        return GetFillRate() > 80 && GetRejectionRate() < 5;
    }

    /// <summary>
    /// description(설명) : Check if trading is underperforming (거래가 저조한지 확인)
    /// Details(상세설명) : Returns true if fill rate < 50% or high rejection rate (체결률 < 50% 또는 높은 거부율이면 true 반환)
    /// </summary>
    public bool IsUnderperforming()
    {
        return GetFillRate() < 50 || GetRejectionRate() > 20;
    }
}

/// <summary>
/// description(설명) : Trading Metric ID value object (거래 메트릭 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for TradingMetric (TradingMetric의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class TradingMetricId : GuidId
{
    public TradingMetricId(Guid value) : base(value) { }

    public static TradingMetricId New() => new(Guid.NewGuid());
}
