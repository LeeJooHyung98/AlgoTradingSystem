using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.MarketData;

/// <summary>
/// description : 특정 시장의 종목만 필터링하는 스펙
/// Details : Market(KOSPI, KOSDAQ, NYSE, NASDAQ 등)을 기준으로 종목을 조회합니다.
///           시장별로 거래 시간, 수수료, 규제가 다르므로 시장별 종목 목록을 가져와 적절한 거래 전략을 적용합니다.
///           예: var kosdaqStocks = repository.Where(new StocksByMarketSpecification("KOSDAQ")); // 코스닥 종목만 조회
/// Applied technology patterns : Specification Pattern, Repository Pattern, Market Segmentation Pattern
/// </summary>
public class StocksByMarketSpecification : Specification<Core.Entities.MarketData.Stock>
{
    private readonly string _market;

    public StocksByMarketSpecification(string market)
    {
        _market = market;
    }

    public override Expression<Func<Core.Entities.MarketData.Stock, bool>> ToExpression()
    {
        return stock => stock.Market == _market;
    }
}
