using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Trading;

/// <summary>
/// description : 특정 종목의 주문만 필터링하는 스펙
/// Details : StockCode를 기준으로 주문을 조회하며, 종목별 거래 내역 및 주문 패턴 분석에 사용됩니다.
///           특정 종목에 대한 매수/매도 주문 현황을 파악하거나, 종목별 평균 체결가를 계산할 때 유용합니다.
///           예: var stockOrders = repository.Where(new OrdersByStockCodeSpecification("005930")); // 삼성전자 주문
/// Applied technology patterns : Specification Pattern, Repository Pattern, Query Object Pattern
/// </summary>
public class OrdersByStockCodeSpecification : Specification<Core.Entities.Trading.Order>
{
    private readonly string _stockCode;

    public OrdersByStockCodeSpecification(string stockCode)
    {
        _stockCode = stockCode;
    }

    public override Expression<Func<Core.Entities.Trading.Order, bool>> ToExpression()
    {
        return order => order.StockCode == _stockCode;
    }
}
