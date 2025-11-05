namespace AlgoTrading.Core.Exceptions.MarketData;

/// <summary>
/// description : 종목 코드 형식이 유효하지 않을 때 발생하는 예외
/// Details : Market Data Context에서 종목 코드(StockCode)가 시장별 형식 규칙을 위반할 때 발생합니다.
///           예: 한국 주식은 6자리 숫자(005930), 미국 주식은 1-5자 알파벳(AAPL, MSFT), 선물은 특정 명명 규칙 등.
///           이 예외는 잘못된 형식의 종목 코드가 시스템에 입력되는 것을 초기에 차단하여 데이터 무결성을 보호합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Validation Pattern, Data Integrity Pattern
/// </summary>
public class InvalidStockCodeException : DomainException
{
    public string StockCode { get; }
    public string ValidationError { get; }

    public InvalidStockCodeException(string stockCode, string validationError)
        : base($"종목 코드 형식이 유효하지 않습니다. StockCode: {stockCode}, ValidationError: {validationError}",
            errorCode: "INVALID_STOCK_CODE")
    {
        StockCode = stockCode;
        ValidationError = validationError;
    }
}
