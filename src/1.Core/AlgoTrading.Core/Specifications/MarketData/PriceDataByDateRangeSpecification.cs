using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.MarketData;

/// <summary>
/// description : 특정 날짜 범위의 가격 데이터만 필터링하는 스펙
/// Details : 시작 날짜(FromDate)와 종료 날짜(ToDate) 사이의 가격 데이터를 조회하는 데 사용됩니다.
///           백테스트, 차트 표시, 기술적 지표 계산 등에서 특정 기간의 과거 데이터가 필요할 때 사용됩니다.
///           TimescaleDB에 저장된 대용량 시계열 데이터를 효율적으로 조회하기 위해 날짜 범위로 필터링합니다.
/// Applied technology patterns : Specification Pattern, Repository Pattern, Time-Series Data Pattern
/// </summary>
public class PriceDataByDateRangeSpecification : Specification<Core.Entities.MarketData.PriceData>
{
    private readonly DateTime _fromDate;
    private readonly DateTime _toDate;

    public PriceDataByDateRangeSpecification(DateTime fromDate, DateTime toDate)
    {
        _fromDate = fromDate;
        _toDate = toDate;
    }

    public override Expression<Func<Core.Entities.MarketData.PriceData, bool>> ToExpression()
    {
        return priceData => priceData.Timestamp >= _fromDate && priceData.Timestamp <= _toDate;
    }
}
