using AlgoTrading.Application.DTOs.Trading;
using ErrorOr;
using MediatR;

namespace AlgoTrading.Application.Queries.Trading;

/// <summary>
/// description(설명) : Get Positions Query (포지션 조회 쿼리)
/// Details(상세설명) : CQRS Query to retrieve all open positions for an account (계좌의 모든 열린 포지션을 조회하는 CQRS 쿼리)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, MediatR Pattern, DTO Pattern (CQRS 패턴, MediatR 패턴, DTO 패턴)
/// </summary>
public sealed record GetPositionsQuery : IRequest<ErrorOr<IEnumerable<PositionDto>>>
{
    /// <summary>
    /// 계좌 번호
    /// </summary>
    public required string AccountNumber { get; init; }

    /// <summary>
    /// 열린 포지션만 조회 (기본값: true)
    /// </summary>
    public bool OnlyOpen { get; init; } = true;

    /// <summary>
    /// 종목 코드 필터 (선택)
    /// </summary>
    public string? StockCode { get; init; }

    /// <summary>
    /// 전략 ID 필터 (선택)
    /// </summary>
    public Guid? StrategyId { get; init; }
}
