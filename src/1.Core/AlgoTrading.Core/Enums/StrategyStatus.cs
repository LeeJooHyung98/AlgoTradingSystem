namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 트레이딩 전략의 생명주기 상태를 나타내는 열거형
/// Details : 전략의 라이프사이클을 Draft(작성 중), Testing(백테스트/페이퍼 트레이딩), Active(실전 운영), Inactive(비활성화), Archived(보관)로 관리합니다. Strategy Context에서 상태 전이는 State Machine Pattern을 통해 제어되며, Draft → Testing → Active 순서로만 진행 가능합니다. Active 상태의 전략만 실시간 매매 신호를 생성하고, Inactive 전략은 일시 중지되어 재활성화 가능하며, Archived 전략은 읽기 전용으로 보관됩니다.
/// Applied technology patterns : State Pattern (GoF), State Machine Pattern, Lifecycle Management
/// </summary>
public enum StrategyStatus
{
    /// <summary>
    /// 초안 (작성 중)
    /// </summary>
    Draft = 1,

    /// <summary>
    /// 테스트 중
    /// </summary>
    Testing = 2,

    /// <summary>
    /// 활성화 (운영 중)
    /// </summary>
    Active = 3,

    /// <summary>
    /// 비활성화
    /// </summary>
    Inactive = 4,

    /// <summary>
    /// 보관됨
    /// </summary>
    Archived = 5
}
