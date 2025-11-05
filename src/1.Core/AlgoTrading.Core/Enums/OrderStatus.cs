namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 주문의 생명주기 상태를 추적하는 열거형
/// Details : Trading Context에서 Order 엔티티의 상태를 Pending(대기 중), Submitted(거래소 접수), PartiallyFilled(부분 체결), Filled(전량 체결), Cancelled(취소), Rejected(거래소 거부), Expired(만료)로 관리합니다. 상태 전이는 State Machine Pattern을 따르며, Pending → Submitted → (PartiallyFilled) → Filled 순서로 진행됩니다. 각 상태 변경 시 도메인 이벤트(OrderCreatedEvent, OrderFilledEvent 등)가 발행되어 Account Context의 잔고 업데이트와 Risk Management Context의 포지션 추적을 트리거합니다. Filled와 Cancelled/Rejected/Expired는 최종 상태입니다.
/// Applied technology patterns : State Machine Pattern, Event Sourcing, Domain Event Pattern
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// 대기 중 (접수 전)
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 접수됨
    /// </summary>
    Submitted = 2,

    /// <summary>
    /// 부분 체결
    /// </summary>
    PartiallyFilled = 3,

    /// <summary>
    /// 전량 체결
    /// </summary>
    Filled = 4,

    /// <summary>
    /// 취소됨
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// 거부됨
    /// </summary>
    Rejected = 6,

    /// <summary>
    /// 만료됨
    /// </summary>
    Expired = 7
}
