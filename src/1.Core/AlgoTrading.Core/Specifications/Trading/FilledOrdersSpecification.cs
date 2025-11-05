using AlgoTrading.Core.Enums;
using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Trading;

/// <summary>
/// description : 체결 완료된 주문만 필터링하는 스펙
/// Details : OrderStatus가 Filled 상태인 주문을 조회하는 데 사용됩니다.
///           체결 완료 주문은 거래소에서 전량 체결된 주문으로, 포지션 생성 및 손익 계산의 기반이 됩니다.
///           거래 내역 조회, 성과 분석, 세금 계산 등에서 체결 완료 주문 목록이 필요할 때 사용됩니다.
/// Applied technology patterns : Specification Pattern, Repository Pattern, Query Object Pattern
/// </summary>
public class FilledOrdersSpecification : Specification<Core.Entities.Trading.Order>
{
    public override Expression<Func<Core.Entities.Trading.Order, bool>> ToExpression()
    {
        return order => order.Status.Equals(OrderStatus.Filled);
    }
}
