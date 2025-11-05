namespace AlgoTrading.Core.Exceptions.Trading;

/// <summary>
/// description : 유효하지 않은 주문 유형이 지정되었을 때 발생하는 예외
/// Details : Order 엔티티 생성 시 지원하지 않는 OrderType(Market, Limit, Stop, StopLimit 외)이 제공되거나,
///           특정 시장/계좌 유형에서 허용되지 않는 주문 유형을 사용하려고 할 때 발생합니다.
///           예: 한국 주식 시장에서 StopLimit 주문이 지원되지 않는 경우, Margin 계좌가 아닌데 신용 매도 주문을 시도하는 경우 등.
///           이 예외는 도메인 규칙과 외부 시장 규정 준수를 동시에 보장하는 데 사용됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Validation Pattern, Market Rules Compliance
/// </summary>
public class InvalidOrderTypeException : DomainException
{
    public string OrderType { get; }

    public InvalidOrderTypeException(string orderType, string reason)
        : base($"유효하지 않은 주문 유형입니다. OrderType: {orderType}, Reason: {reason}", errorCode: "INVALID_ORDER_TYPE")
    {
        OrderType = orderType;
    }
}
