namespace AlgoTrading.Application.DTOs.Trading;

/// <summary>
/// description(설명) : Trade Data Transfer Object (거래 데이터 전송 객체)
/// Details(상세설명) : DTO for transferring completed trade data between application layers (애플리케이션 계층 간 완료된 거래 데이터 전송용 DTO)
/// Applied technology patterns(적용기술패턴) : DTO Pattern, CQRS Pattern (DTO 패턴, CQRS 패턴)
/// </summary>
public sealed class TradeDto
{
    /// <summary>
    /// 거래 ID
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 종목 코드
    /// </summary>
    public string StockCode { get; init; } = string.Empty;

    /// <summary>
    /// 거래 방향 (Long=1, Short=2)
    /// </summary>
    public int Side { get; init; }

    /// <summary>
    /// 거래 방향 이름
    /// </summary>
    public string SideName { get; init; } = string.Empty;

    /// <summary>
    /// 진입 주문 ID
    /// </summary>
    public Guid EntryOrderId { get; init; }

    /// <summary>
    /// 청산 주문 ID
    /// </summary>
    public Guid ExitOrderId { get; init; }

    /// <summary>
    /// 거래 수량
    /// </summary>
    public int Quantity { get; init; }

    /// <summary>
    /// 진입 가격
    /// </summary>
    public decimal EntryPrice { get; init; }

    /// <summary>
    /// 청산 가격
    /// </summary>
    public decimal ExitPrice { get; init; }

    /// <summary>
    /// 진입 시간
    /// </summary>
    public DateTime EntryTime { get; init; }

    /// <summary>
    /// 청산 시간
    /// </summary>
    public DateTime ExitTime { get; init; }

    /// <summary>
    /// 실현 손익
    /// </summary>
    public decimal RealizedPL { get; init; }

    /// <summary>
    /// 실현 손익률 (%)
    /// </summary>
    public decimal RealizedPLPercent { get; init; }

    /// <summary>
    /// 수수료
    /// </summary>
    public decimal Commission { get; init; }

    /// <summary>
    /// 거래 기간 (시간)
    /// </summary>
    public double DurationInHours { get; init; }

    /// <summary>
    /// 거래 기간 (일)
    /// </summary>
    public double DurationInDays { get; init; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// 전략 ID (nullable)
    /// </summary>
    public Guid? StrategyId { get; init; }

    /// <summary>
    /// 거래 결과 (Win=1, Loss=2, Breakeven=3)
    /// </summary>
    public int Result { get; init; }

    /// <summary>
    /// 거래 결과 이름
    /// </summary>
    public string ResultName { get; init; } = string.Empty;

    /// <summary>
    /// 메모 또는 태그
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// 수익 거래 여부
    /// </summary>
    public bool IsWinner { get; init; }

    /// <summary>
    /// 손실 거래 여부
    /// </summary>
    public bool IsLoser { get; init; }

    /// <summary>
    /// 당일 매매 여부
    /// </summary>
    public bool IsDayTrade { get; init; }

    /// <summary>
    /// 수수료 차감 전 총 손익
    /// </summary>
    public decimal GrossPL { get; init; }
}
