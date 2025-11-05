namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 출금 요청의 처리 상태를 추적하는 열거형
/// Details : 계좌에서 자금 출금 요청의 처리 단계를 Pending(대기 중), Processing(처리 중), Completed(완료), Failed(실패), Cancelled(취소)로 관리합니다. Account Context에서 출금 워크플로우를 관리하며, 각 상태 전이 시 도메인 이벤트(WithdrawalRequestedEvent 등)를 발행합니다. Processing 상태에서는 잔액 검증, 제한 사항 확인, 외부 은행 API 호출이 수행되며, Failed 상태는 재시도 가능하지만 Completed와 Cancelled는 최종 상태입니다.
/// Applied technology patterns : Saga Pattern, State Machine Pattern, Event Sourcing
/// </summary>
public enum WithdrawalStatus
{
    /// <summary>
    /// 대기 중
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 처리 중
    /// </summary>
    Processing = 2,

    /// <summary>
    /// 완료됨
    /// </summary>
    Completed = 3,

    /// <summary>
    /// 실패함
    /// </summary>
    Failed = 4,

    /// <summary>
    /// 취소됨
    /// </summary>
    Cancelled = 5
}
