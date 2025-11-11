using ErrorOr;
using MediatR;

namespace AlgoTrading.Application.Commands.Trading;

/// <summary>
/// description(설명) : Close Position Command (포지션 청산 명령)
/// Details(상세설명) : CQRS Command to close an existing position fully or partially (기존 포지션을 전체 또는 부분 청산하는 CQRS 명령)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, MediatR Pattern, Result Pattern (CQRS 패턴, MediatR 패턴, Result 패턴)
/// </summary>
public sealed record ClosePositionCommand : IRequest<ErrorOr<Guid>>
{
    /// <summary>
    /// 청산할 포지션 ID
    /// </summary>
    public required Guid PositionId { get; init; }

    /// <summary>
    /// 청산 수량 (null이면 전량 청산)
    /// </summary>
    public int? Quantity { get; init; }

    /// <summary>
    /// 청산 주문 유형 (Market=1, Limit=2)
    /// </summary>
    public int OrderType { get; init; } = 1; // Default: Market

    /// <summary>
    /// 지정가 청산인 경우 가격
    /// </summary>
    public decimal? LimitPrice { get; init; }

    /// <summary>
    /// 계좌 번호 (검증용)
    /// </summary>
    public required string AccountNumber { get; init; }

    /// <summary>
    /// 청산 사유 (선택)
    /// </summary>
    public string? Reason { get; init; }
}
