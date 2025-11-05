namespace AlgoTrading.Core.Exceptions.Trading;

/// <summary>
/// description : 현재 주문 상태에서 수행할 수 없는 작업을 시도할 때 발생하는 예외
/// Details : Order 엔티티는 State Machine 패턴을 따르며, 각 상태(Pending, Submitted, PartiallyFilled, Filled, Cancelled)에서 허용되는 작업이 정해져 있습니다.
///           예: 이미 Filled 상태인 주문을 취소하려는 시도, Cancelled 상태인 주문을 수정하려는 시도 등이 이 예외를 발생시킵니다.
///           이 예외는 주문 생명주기의 불변성을 보호하며, 잘못된 상태 전이로 인한 데이터 불일치를 방지합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), State Machine Pattern, Invariant Protection
/// </summary>
public class InvalidOrderStateException : DomainException
{
    public string CurrentState { get; }
    public string AttemptedAction { get; }

    public InvalidOrderStateException(Guid orderId, string currentState, string attemptedAction)
        : base($"현재 주문 상태에서 작업을 수행할 수 없습니다. OrderId: {orderId}, CurrentState: {currentState}, AttemptedAction: {attemptedAction}",
            orderId, "INVALID_ORDER_STATE")
    {
        CurrentState = currentState;
        AttemptedAction = attemptedAction;
    }
}
