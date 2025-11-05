namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 증권 계좌의 운영 상태를 나타내는 열거형
/// Details : 계좌의 생명주기 상태를 Active(정상 운영), Suspended(일시 정지), Closed(폐쇄), Inactive(비활성화)로 관리합니다. Account Context에서 계좌 상태에 따라 거래 가능 여부를 제어하며, Active 상태만 주문 생성이 가능합니다. Suspended 상태는 리스크 위반이나 규제 이슈로 거래가 일시 중지된 상태이며, Closed 계좌는 영구적으로 사용 불가능합니다. Risk Management Context는 계좌 상태 변경 이벤트를 구독하여 활성 전략을 자동으로 중지시킵니다.
/// Applied technology patterns : State Pattern, Event-Driven Architecture, Lifecycle Management
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// 활성화
    /// </summary>
    Active = 1,

    /// <summary>
    /// 정지됨
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// 폐쇄됨
    /// </summary>
    Closed = 3,

    /// <summary>
    /// 비활성화
    /// </summary>
    Inactive = 4
}
