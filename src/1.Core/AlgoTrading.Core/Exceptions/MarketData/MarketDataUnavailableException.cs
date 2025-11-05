namespace AlgoTrading.Core.Exceptions.MarketData;

/// <summary>
/// description : 시장 데이터를 현재 이용할 수 없을 때 발생하는 예외
/// Details : Market Data Service에서 외부 데이터 제공업체(Kiwoom, eBest 등)로부터 실시간 시세 또는 과거 데이터를 받지 못할 때 발생합니다.
///           네트워크 장애, API 제한 초과, 데이터 제공업체 시스템 점검, 시장 휴장일 등 다양한 원인으로 발생할 수 있습니다.
///           이 예외가 발생하면 전략 실행이 일시 중지되며, 데이터 복구 시까지 대기하거나 대체 데이터 소스로 전환해야 합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Resilience Pattern, Circuit Breaker Pattern
/// </summary>
public class MarketDataUnavailableException : DomainException
{
    public string DataProvider { get; }
    public string UnavailableReason { get; }

    public MarketDataUnavailableException(string dataProvider, string unavailableReason)
        : base($"시장 데이터를 이용할 수 없습니다. DataProvider: {dataProvider}, Reason: {unavailableReason}",
            errorCode: "MARKET_DATA_UNAVAILABLE")
    {
        DataProvider = dataProvider;
        UnavailableReason = unavailableReason;
    }

    public MarketDataUnavailableException(string dataProvider, string unavailableReason, Exception innerException)
        : base($"시장 데이터를 이용할 수 없습니다. DataProvider: {dataProvider}, Reason: {unavailableReason}",
            innerException, errorCode: "MARKET_DATA_UNAVAILABLE")
    {
        DataProvider = dataProvider;
        UnavailableReason = unavailableReason;
    }
}
