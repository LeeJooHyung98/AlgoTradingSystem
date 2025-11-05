using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.MarketData;

/// <summary>
/// description : 특정 섹터의 종목만 필터링하는 스펙
/// Details : Sector(Technology, Finance, Healthcare, Energy 등)를 기준으로 종목을 조회합니다.
///           섹터별 종목 분석, 포트폴리오 섹터 배분 확인, 섹터 로테이션 전략 구현 시 유용합니다.
///           예: var techStocks = repository.Where(new StocksBySectorSpecification("Technology")); // 기술주만 조회
/// Applied technology patterns : Specification Pattern, Repository Pattern, Sector Analysis Pattern
/// </summary>
public class StocksBySectorSpecification : Specification<Core.Entities.MarketData.Stock>
{
    private readonly string _sector;

    public StocksBySectorSpecification(string sector)
    {
        _sector = sector;
    }

    public override Expression<Func<Core.Entities.MarketData.Stock, bool>> ToExpression()
    {
        return stock => stock.Sector == _sector;
    }
}
