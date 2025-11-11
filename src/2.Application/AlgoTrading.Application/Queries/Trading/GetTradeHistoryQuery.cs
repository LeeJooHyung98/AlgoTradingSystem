using AlgoTrading.Application.DTOs.Trading;
using ErrorOr;
using MediatR;

namespace AlgoTrading.Application.Queries.Trading;

/// <summary>
/// description(설명) : Get Trade History Query (거래 이력 조회 쿼리)
/// Details(상세설명) : CQRS Query to retrieve completed trade history for an account (계좌의 완료된 거래 이력을 조회하는 CQRS 쿼리)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, MediatR Pattern, DTO Pattern (CQRS 패턴, MediatR 패턴, DTO 패턴)
/// </summary>
public sealed record GetTradeHistoryQuery : IRequest<ErrorOr<IEnumerable<TradeDto>>>
{
    /// <summary>
    /// 계좌 번호
    /// </summary>
    public required string AccountNumber { get; init; }

    /// <summary>
    /// 조회 시작 날짜 (선택)
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// 조회 종료 날짜 (선택)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// 종목 코드 필터 (선택)
    /// </summary>
    public string? StockCode { get; init; }

    /// <summary>
    /// 전략 ID 필터 (선택)
    /// </summary>
    public Guid? StrategyId { get; init; }

    /// <summary>
    /// 거래 결과 필터 (Win=1, Loss=2, Breakeven=3, 선택)
    /// </summary>
    public int? Result { get; init; }

    /// <summary>
    /// 페이지 번호 (선택, 기본값: 1)
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// 페이지 크기 (선택, 기본값: 50)
    /// </summary>
    public int PageSize { get; init; } = 50;
}
