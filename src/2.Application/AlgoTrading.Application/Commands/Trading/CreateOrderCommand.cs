using ErrorOr;
using MediatR;

namespace AlgoTrading.Application.Commands.Trading;

/// <summary>
/// description(설명) : Create Order Command (주문 생성 명령)
/// Details(상세설명) : CQRS Command to create a new trading order (매매 주문을 생성하는 CQRS 명령)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, MediatR Pattern, Result Pattern (CQRS 패턴, MediatR 패턴, Result 패턴)
/// </summary>
public sealed record CreateOrderCommand : IRequest<ErrorOr<Guid>>
{
    /// <summary>
    /// 종목 코드
    /// </summary>
    public required string StockCode { get; init; }

    /// <summary>
    /// 주문 방향 (Buy=1, Sell=2)
    /// </summary>
    public required int Side { get; init; }

    /// <summary>
    /// 주문 유형 (Market=1, Limit=2, Stop=3, StopLimit=4)
    /// </summary>
    public required int Type { get; init; }

    /// <summary>
    /// 주문 수량
    /// </summary>
    public required int Quantity { get; init; }

    /// <summary>
    /// 지정가 (지정가 주문인 경우 필수)
    /// </summary>
    public decimal? LimitPrice { get; init; }

    /// <summary>
    /// 손절가 (손절 주문인 경우 필수)
    /// </summary>
    public decimal? StopPrice { get; init; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public required string AccountNumber { get; init; }

    /// <summary>
    /// 주문 출처 (Manual=1, Strategy=2, API=3)
    /// </summary>
    public int Source { get; init; } = 1; // Default: Manual

    /// <summary>
    /// 전략 ID (전략 자동 주문인 경우)
    /// </summary>
    public Guid? StrategyId { get; init; }

    /// <summary>
    /// 주문 유효기간 (Day=1, GTC=2, IOC=3, FOK=4)
    /// </summary>
    public int TimeInForce { get; init; } = 1; // Default: Day
}
