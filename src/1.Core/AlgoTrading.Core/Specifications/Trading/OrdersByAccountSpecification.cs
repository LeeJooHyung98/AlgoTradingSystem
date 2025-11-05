using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Trading;

/// <summary>
/// description : 특정 계좌의 주문만 필터링하는 스펙
/// Details : AccountNumber를 기준으로 주문을 조회하며, 멀티 계좌 환경에서 계좌별 주문 내역을 분리하여 관리할 때 사용됩니다.
///           계좌별 거래 내역, 주문 통계, 성과 분석 등에서 특정 계좌의 주문만 필터링해야 할 때 필수적입니다.
///           예: var accountOrders = repository.Where(new OrdersByAccountSpecification("1234567890"));
/// Applied technology patterns : Specification Pattern, Repository Pattern, Multi-Tenancy Pattern
/// </summary>
public class OrdersByAccountSpecification : Specification<Core.Entities.Trading.Order>
{
    private readonly string _accountNumber;

    public OrdersByAccountSpecification(string accountNumber)
    {
        _accountNumber = accountNumber;
    }

    public override Expression<Func<Core.Entities.Trading.Order, bool>> ToExpression()
    {
        return order => order.AccountNumber == _accountNumber;
    }
}
