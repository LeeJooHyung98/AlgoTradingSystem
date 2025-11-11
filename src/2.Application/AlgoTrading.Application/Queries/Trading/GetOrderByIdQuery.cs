using AlgoTrading.Application.DTOs.Trading;
using ErrorOr;
using MediatR;

namespace AlgoTrading.Application.Queries.Trading;

/// <summary>
/// description(설명) : Get Order By ID Query (ID로 주문 조회 쿼리)
/// Details(상세설명) : CQRS Query to retrieve a single order by its ID (ID로 단일 주문을 조회하는 CQRS 쿼리)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, MediatR Pattern, DTO Pattern (CQRS 패턴, MediatR 패턴, DTO 패턴)
/// </summary>
public sealed record GetOrderByIdQuery : IRequest<ErrorOr<OrderDto>>
{
    /// <summary>
    /// 조회할 주문 ID
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    /// 계좌 번호 (권한 검증용, 선택)
    /// </summary>
    public string? AccountNumber { get; init; }
}
