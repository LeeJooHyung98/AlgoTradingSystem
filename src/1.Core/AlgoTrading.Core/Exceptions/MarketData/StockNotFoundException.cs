namespace AlgoTrading.Core.Exceptions.MarketData;

/// <summary>
/// description : 요청한 종목을 찾을 수 없을 때 발생하는 예외
/// Details : Market Data Context에서 존재하지 않는 종목 코드(StockCode)를 조회하려고 할 때 발생합니다.
///           잘못된 종목 코드가 제공되거나, 상장 폐지된 종목을 참조하는 경우, 또는 마스터 데이터가 아직 동기화되지 않은 경우에 발생합니다.
///           이 예외는 유효하지 않은 종목에 대한 주문 생성을 방지하며, 클라이언트에게 404 Not Found 응답을 반환하는 데 사용됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Exception Pattern, Not Found Pattern
/// </summary>
public class StockNotFoundException : DomainException
{
    public string StockCode { get; }

    public StockNotFoundException(string stockCode)
        : base($"종목을 찾을 수 없습니다. StockCode: {stockCode}", errorCode: "STOCK_NOT_FOUND")
    {
        StockCode = stockCode;
    }

    public StockNotFoundException(Guid stockId)
        : base($"종목을 찾을 수 없습니다. StockId: {stockId}", stockId, "STOCK_NOT_FOUND")
    {
        StockCode = string.Empty;
    }
}
