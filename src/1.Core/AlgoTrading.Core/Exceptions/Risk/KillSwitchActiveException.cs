namespace AlgoTrading.Core.Exceptions.Risk;

/// <summary>
/// description : 킬 스위치가 활성화되어 모든 거래가 차단되었을 때 발생하는 예외
/// Details : Risk Management Context에서 킬 스위치(Circuit Breaker)가 활성화된 상태에서 신규 주문을 시도하면 발생합니다.
///           킬 스위치는 일일 손실 한도 초과, 시스템 오류, 수동 긴급 정지 등의 상황에서 활성화되며, 모든 자동 거래를 즉시 차단합니다.
///           이 예외가 발생하면 사용자는 반드시 상황을 확인하고 수동으로 킬 스위치를 해제해야 거래를 재개할 수 있습니다.
///           킬 스위치는 시스템 전체의 안전을 보장하는 최후의 방어 수단입니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Circuit Breaker Pattern, Emergency Stop Pattern
/// </summary>
public class KillSwitchActiveException : DomainException
{
    public string ActivationReason { get; }
    public DateTime ActivatedAt { get; }

    public KillSwitchActiveException(string activationReason, DateTime activatedAt)
        : base($"킬 스위치가 활성화되어 모든 거래가 차단되었습니다. Reason: {activationReason}, ActivatedAt: {activatedAt:yyyy-MM-dd HH:mm:ss}",
            errorCode: "KILL_SWITCH_ACTIVE")
    {
        ActivationReason = activationReason;
        ActivatedAt = activatedAt;
    }

    public KillSwitchActiveException(Guid accountId, string activationReason, DateTime activatedAt)
        : base($"킬 스위치가 활성화되어 모든 거래가 차단되었습니다. AccountId: {accountId}, Reason: {activationReason}, ActivatedAt: {activatedAt:yyyy-MM-dd HH:mm:ss}",
            accountId, "KILL_SWITCH_ACTIVE")
    {
        ActivationReason = activationReason;
        ActivatedAt = activatedAt;
    }
}
