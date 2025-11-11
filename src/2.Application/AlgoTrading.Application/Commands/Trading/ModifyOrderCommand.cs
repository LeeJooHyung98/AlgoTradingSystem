using ErrorOr;
using MediatR;

namespace AlgoTrading.Application.Commands.Trading;

/// <summary>
/// description(설명) : Modify Order Command (주문 정정 명령)
/// Details(상세설명) : CQRS Command to modify an existing order's quantity or price (기존 주문의 수량 또는 가격을 정정하는 CQRS 명령)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, MediatR Pattern, Result Pattern (CQRS 패턴, MediatR 패턴, Result 패턴)
/// </summary>
public sealed record ModifyOrderCommand : IRequest<ErrorOr<bool>>
{
    /// <summary>
    /// 정정할 주문 ID
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    /// 새로운 수량 (변경하지 않으면 null)
    /// </summary>
    public int? NewQuantity { get; init; }

    /// <summary>
    /// 새로운 지정가 (변경하지 않으면 null)
    /// </summary>
    public decimal? NewLimitPrice { get; init; }

    /// <summary>
    /// 새로운 손절가 (변경하지 않으면 null)
    /// </summary>
    public decimal? NewStopPrice { get; init; }

    /// <summary>
    /// 계좌 번호 (검증용)
    /// </summary>
    public required string AccountNumber { get; init; }
}
