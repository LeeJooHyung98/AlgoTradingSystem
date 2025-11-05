namespace AlgoTrading.Core.Exceptions.MarketData;

/// <summary>
/// description : 요청한 기간의 가격 데이터를 찾을 수 없을 때 발생하는 예외
/// Details : Market Data Context에서 특정 종목의 특정 기간(FromDate ~ ToDate) 또는 특정 시간대의 OHLC 데이터를 조회했으나 존재하지 않을 때 발생합니다.
///           신규 상장 종목, 데이터 수집 실패, 시장 휴장일, TimescaleDB 동기화 지연 등의 이유로 발생할 수 있습니다.
///           이 예외는 백테스트 및 전략 실행에 필수적인 과거 데이터 부족을 나타내며, 데이터 수집 작업을 트리거하는 데 사용될 수 있습니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Exception Pattern, Data Availability Pattern
/// </summary>
public class PriceDataNotFoundException : DomainException
{
    public string StockCode { get; }
    public DateTime FromDate { get; }
    public DateTime ToDate { get; }

    public PriceDataNotFoundException(string stockCode, DateTime fromDate, DateTime toDate)
        : base($"가격 데이터를 찾을 수 없습니다. StockCode: {stockCode}, Period: {fromDate:yyyy-MM-dd} ~ {toDate:yyyy-MM-dd}",
            errorCode: "PRICE_DATA_NOT_FOUND")
    {
        StockCode = stockCode;
        FromDate = fromDate;
        ToDate = toDate;
    }

    public PriceDataNotFoundException(string stockCode, DateTime date)
        : base($"가격 데이터를 찾을 수 없습니다. StockCode: {stockCode}, Date: {date:yyyy-MM-dd}",
            errorCode: "PRICE_DATA_NOT_FOUND")
    {
        StockCode = stockCode;
        FromDate = date;
        ToDate = date;
    }
}
