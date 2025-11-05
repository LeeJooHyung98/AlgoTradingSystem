namespace AlgoTrading.Core.Exceptions.Trading;

/// <summary>
/// description : 요청한 주문 ID를 찾을 수 없을 때 발생하는 예외
/// Details : Trading Context에서 존재하지 않는 주문을 조회, 수정, 취소하려고 할 때 발생합니다.
///           잘못된 OrderId가 제공되거나 주문이 이미 삭제된 후 참조되는 경우에 해당하며, 클라이언트에게 404 Not Found 응답을 반환하는 데 사용됩니다.
///           주문은 금융 거래의 핵심 엔티티이므로, 존재하지 않는 주문 참조는 중대한 오류로 간주되어 감사 로그에 기록됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Exception Pattern, Not Found Pattern
/// </summary>
public class OrderNotFoundException : DomainException
{
    public OrderNotFoundException(Guid orderId)
        : base($"주문을 찾을 수 없습니다. OrderId: {orderId}", orderId, "ORDER_NOT_FOUND")
    {
    }

    public OrderNotFoundException(string orderNumber)
        : base($"주문을 찾을 수 없습니다. OrderNumber: {orderNumber}", errorCode: "ORDER_NOT_FOUND")
    {
    }
}
