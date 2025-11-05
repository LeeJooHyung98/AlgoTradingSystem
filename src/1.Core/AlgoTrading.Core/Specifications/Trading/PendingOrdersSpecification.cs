using AlgoTrading.Core.Enums;
using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Trading;

/// <summary>
/// description : 대기 중인 주문만 필터링하는 스펙
/// Details : OrderStatus가 Pending 상태인 주문을 조회하는 데 사용됩니다.
///           대기 중인 주문은 아직 거래소에 전송되지 않았거나 승인 대기 중인 주문으로, 수정 또는 취소가 가능한 상태입니다.
///           주문 관리 시스템에서 사용자가 대기 중인 주문 목록을 확인하고 관리할 때 자주 사용되는 스펙입니다.
/// Applied technology patterns : Specification Pattern, Repository Pattern, Query Object Pattern
/// </summary>
public class PendingOrdersSpecification : Specification<Core.Entities.Trading.Order>
{
    public override Expression<Func<Core.Entities.Trading.Order, bool>> ToExpression()
    {
        return order => order.Status.Equals(OrderStatus.Pending);
    }
}
