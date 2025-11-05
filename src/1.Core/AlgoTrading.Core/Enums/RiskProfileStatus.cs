namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 리스크 관리 프로필의 생명주기 상태
/// Details : Risk Management Context에서 리스크 프로필의 상태를 Draft(작성 중), Active(운영 중), Inactive(비활성화), Archived(보관)로 관리합니다. 리스크 프로필은 포지션 한도, VaR 제한, 손절 규칙 등을 정의하며, Active 상태의 프로필만 실시간 리스크 모니터링에 적용됩니다. 계좌나 전략에 할당된 리스크 프로필이 Inactive 상태로 변경되면 해당 전략의 자동 거래가 중지됩니다. Draft 프로필은 백테스트를 통해 검증 후 Active로 전환됩니다.
/// Applied technology patterns : State Pattern, Lifecycle Management, Policy Pattern
/// </summary>
public enum RiskProfileStatus
{
    /// <summary>
    /// 초안 (작성 중)
    /// </summary>
    Draft = 1,

    /// <summary>
    /// 활성화
    /// </summary>
    Active = 2,

    /// <summary>
    /// 비활성화
    /// </summary>
    Inactive = 3,

    /// <summary>
    /// 보관됨
    /// </summary>
    Archived = 4
}
