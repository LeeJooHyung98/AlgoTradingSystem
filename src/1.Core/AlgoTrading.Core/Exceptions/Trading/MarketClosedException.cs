namespace AlgoTrading.Core.Exceptions.Trading;

/// <summary>
/// description : 시장이 마감된 시간에 주문을 시도할 때 발생하는 예외
/// Details : Trading Context에서 시장 운영 시간(한국 주식: 09:00-15:30) 외에 주문을 생성하려고 할 때 발생합니다.
///           시장별로 운영 시간이 다르므로(미국 주식, 선물 시장 등), MarketType과 현재 시간을 기반으로 검증합니다.
///           장 마감 후 주문은 Pre-Market 또는 After-Hours Trading에서만 허용되며, 일반 거래 시간에는 이 예외가 발생합니다.
///           이 예외는 외부 시장 규정 준수와 무효 주문 방지를 위해 사용됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Business Hours Validation, Market Rules Compliance
/// </summary>
public class MarketClosedException : DomainException
{
    public string MarketType { get; }
    public DateTime AttemptedAt { get; }

    public MarketClosedException(string marketType, DateTime attemptedAt)
        : base($"시장이 마감되어 주문할 수 없습니다. Market: {marketType}, AttemptedAt: {attemptedAt:yyyy-MM-dd HH:mm:ss}",
            errorCode: "MARKET_CLOSED")
    {
        MarketType = marketType;
        AttemptedAt = attemptedAt;
    }
}
