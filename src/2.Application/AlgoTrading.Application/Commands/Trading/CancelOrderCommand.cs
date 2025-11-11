using ErrorOr;
using MediatR;

namespace AlgoTrading.Application.Commands.Trading;

/// <summary>
/// description(설명) : Cancel Order Command (주문 취소 명령)
/// Details(상세설명) : CQRS Command to cancel an existing order (기존 주문을 취소하는 CQRS 명령)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, MediatR Pattern, Result Pattern (CQRS 패턴, MediatR 패턴, Result 패턴)
/// </summary>
public sealed record CancelOrderCommand : IRequest<ErrorOr<bool>>
{
    /// <summary>
    /// 취소할 주문 ID
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    /// 취소 사유
    /// </summary>
    public string Reason { get; init; } = "User requested cancellation";

    /// <summary>
    /// 계좌 번호 (검증용)
    /// </summary>
    public required string AccountNumber { get; init; }
}
