namespace AlgoTrading.Application.DTOs.Trading;

/// <summary>
/// description(설명) : Position Data Transfer Object (포지션 데이터 전송 객체)
/// Details(상세설명) : DTO for transferring position data between application layers (애플리케이션 계층 간 포지션 데이터 전송용 DTO)
/// Applied technology patterns(적용기술패턴) : DTO Pattern, CQRS Pattern (DTO 패턴, CQRS 패턴)
/// </summary>
public sealed class PositionDto
{
    /// <summary>
    /// 포지션 ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 종목 코드
    /// </summary>
    public string StockCode { get; init; } = string.Empty;

    /// <summary>
    /// 포지션 방향 (Long=1, Short=2)
    /// </summary>
    public int Side { get; init; }

    /// <summary>
    /// 포지션 방향 이름
    /// </summary>
    public string SideName { get; init; } = string.Empty;

    /// <summary>
    /// 현재 수량
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// 평균 진입 가격
    /// </summary>
    public decimal AverageEntryPrice { get; init; }

    /// <summary>
    /// 현재 시장 가격
    /// </summary>
    public decimal CurrentPrice { get; init; }

    /// <summary>
    /// 진입 주문 ID
    /// </summary>
    public Guid EntryOrderId { get; init; }

    /// <summary>
    /// 손절 가격 (nullable)
    /// </summary>
    public decimal? StopLossPrice { get; init; }

    /// <summary>
    /// 익절 가격 (nullable)
    /// </summary>
    public decimal? TakeProfitPrice { get; init; }

    /// <summary>
    /// 포지션 오픈 시간
    /// </summary>
    public DateTime OpenedAt { get; init; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// 전략 ID (nullable)
    /// </summary>
    public Guid? StrategyId { get; init; }

    /// <summary>
    /// 총 비용 기준
    /// </summary>
    public decimal CostBasis { get; init; }

    /// <summary>
    /// 현재 시장 가치
    /// </summary>
    public decimal MarketValue { get; init; }

    /// <summary>
    /// 미실현 손익
    /// </summary>
    public decimal UnrealizedPL { get; init; }

    /// <summary>
    /// 미실현 손익 비율 (%)
    /// </summary>
    public decimal UnrealizedPLPercent { get; init; }

    /// <summary>
    /// 수익 중 여부
    /// </summary>
    public bool IsProfitable { get; init; }

    /// <summary>
    /// 손실 중 여부
    /// </summary>
    public bool IsInLoss { get; init; }

    /// <summary>
    /// 보유 기간 (시간)
    /// </summary>
    public double DurationInHours { get; init; }

    /// <summary>
    /// 보유 기간 (일)
    /// </summary>
    public double DurationInDays { get; init; }

    /// <summary>
    /// 리스크 금액 (손절가 기준, nullable)
    /// </summary>
    public decimal? RiskAmount { get; init; }

    /// <summary>
    /// 리스크-보상 비율 (nullable)
    /// </summary>
    public decimal? RiskRewardRatio { get; init; }
}
