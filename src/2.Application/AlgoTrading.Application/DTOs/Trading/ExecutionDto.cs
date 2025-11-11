namespace AlgoTrading.Application.DTOs.Trading;

/// <summary>
/// description(설명) : Execution Data Transfer Object (체결 데이터 전송 객체)
/// Details(상세설명) : DTO for transferring order execution data between application layers (애플리케이션 계층 간 주문 체결 데이터 전송용 DTO)
/// Applied technology patterns(적용기술패턴) : DTO Pattern, CQRS Pattern (DTO 패턴, CQRS 패턴)
/// </summary>
public sealed class ExecutionDto
{
    /// <summary>
    /// 체결 ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 체결된 주문 ID
    /// </summary>
    public Guid OrderId { get; init; }

    /// <summary>
    /// 종목 코드
    /// </summary>
    public string StockCode { get; init; } = string.Empty;

    /// <summary>
    /// 체결 방향 (Buy=1, Sell=2)
    /// </summary>
    public int Side { get; init; }

    /// <summary>
    /// 체결 방향 이름
    /// </summary>
    public string SideName { get; init; } = string.Empty;

    /// <summary>
    /// 체결 수량
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// 체결 가격
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// 체결 금액
    /// </summary>
    public decimal Value { get; init; }

    /// <summary>
    /// 체결 수수료
    /// </summary>
    public decimal Commission { get; init; }

    /// <summary>
    /// 체결 시간
    /// </summary>
    public DateTime ExecutionTime { get; init; }

    /// <summary>
    /// 증권사 체결 ID
    /// </summary>
    public string? BrokerExecutionId { get; init; }

    /// <summary>
    /// 체결 거래소
    /// </summary>
    public string? ExecutionVenue { get; init; }

    /// <summary>
    /// 부분 체결 여부
    /// </summary>
    public bool IsPartialFill { get; init; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// 수수료 차감 후 순 금액
    /// </summary>
    public decimal NetValue { get; init; }

    /// <summary>
    /// 수수료를 포함한 실효 가격
    /// </summary>
    public decimal EffectivePrice { get; init; }
}
