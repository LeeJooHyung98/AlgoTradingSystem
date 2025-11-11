namespace AlgoTrading.Application.DTOs.Trading;

/// <summary>
/// description(설명) : Order Data Transfer Object (주문 데이터 전송 객체)
/// Details(상세설명) : DTO for transferring order data between application layers (애플리케이션 계층 간 주문 데이터 전송용 DTO)
/// Applied technology patterns(적용기술패턴) : DTO Pattern, CQRS Pattern (DTO 패턴, CQRS 패턴)
/// </summary>
public sealed class OrderDto
{
    /// <summary>
    /// 주문 ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 종목 코드
    /// </summary>
    public string StockCode { get; init; } = string.Empty;

    /// <summary>
    /// 주문 방향 (Buy=1, Sell=2)
    /// </summary>
    public int Side { get; init; }

    /// <summary>
    /// 주문 방향 이름
    /// </summary>
    public string SideName { get; init; } = string.Empty;

    /// <summary>
    /// 주문 유형 (Market=1, Limit=2, Stop=3, StopLimit=4)
    /// </summary>
    public int Type { get; init; }

    /// <summary>
    /// 주문 유형 이름
    /// </summary>
    public string TypeName { get; init; } = string.Empty;

    /// <summary>
    /// 주문 수량
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// 체결된 수량
    /// </summary>
    public int FilledQuantity { get; init; }

    /// <summary>
    /// 남은 수량
    /// </summary>
    public int RemainingQuantity { get; init; }

    /// <summary>
    /// 지정가 (nullable)
    /// </summary>
    public decimal? LimitPrice { get; init; }

    /// <summary>
    /// 손절가 (nullable)
    /// </summary>
    public decimal? StopPrice { get; init; }

    /// <summary>
    /// 평균 체결가 (nullable)
    /// </summary>
    public decimal? AverageFillPrice { get; init; }

    /// <summary>
    /// 주문 상태 (PendingSubmit=1, Submitted=2, Accepted=3, PartiallyFilled=4, Filled=5, Cancelled=6, Rejected=7)
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// 주문 상태 이름
    /// </summary>
    public string StatusName { get; init; } = string.Empty;

    /// <summary>
    /// 주문 유효기간 (Day=1, GTC=2, IOC=3, FOK=4)
    /// </summary>
    public int TimeInForce { get; init; }

    /// <summary>
    /// 주문 유효기간 이름
    /// </summary>
    public string TimeInForceName { get; init; } = string.Empty;

    /// <summary>
    /// 주문 제출 시간
    /// </summary>
    public DateTime SubmittedAt { get; init; }

    /// <summary>
    /// 주문 승인 시간 (nullable)
    /// </summary>
    public DateTime? AcceptedAt { get; init; }

    /// <summary>
    /// 주문 완료 시간 (nullable)
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// 주문 취소 시간 (nullable)
    /// </summary>
    public DateTime? CancelledAt { get; init; }

    /// <summary>
    /// 증권사 주문 ID
    /// </summary>
    public string? BrokerOrderId { get; init; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// 주문 출처 (Manual=1, Strategy=2, API=3)
    /// </summary>
    public int Source { get; init; }

    /// <summary>
    /// 주문 출처 이름
    /// </summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>
    /// 전략 ID (nullable)
    /// </summary>
    public Guid? StrategyId { get; init; }

    /// <summary>
    /// 취소 사유
    /// </summary>
    public string? CancellationReason { get; init; }

    /// <summary>
    /// 거부 사유
    /// </summary>
    public string? RejectionReason { get; init; }

    /// <summary>
    /// 체결률 (0.0 ~ 1.0)
    /// </summary>
    public decimal FillRate { get; init; }

    /// <summary>
    /// 예상 주문 금액
    /// </summary>
    public decimal EstimatedValue { get; init; }

    /// <summary>
    /// 주문 활성 상태 여부
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// 주문 완료 상태 여부
    /// </summary>
    public bool IsCompleted { get; init; }
}
