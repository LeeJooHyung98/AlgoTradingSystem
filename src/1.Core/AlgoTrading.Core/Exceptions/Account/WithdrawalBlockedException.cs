namespace AlgoTrading.Core.Exceptions.Account;

/// <summary>
/// description : 출금이 차단된 계좌에서 출금을 시도할 때 발생하는 예외
/// Details : Account 엔티티에 WithdrawalBlock(출금 제한)이 설정된 상태에서 출금을 요청하면 발생합니다.
///           출금 제한은 규제 요구사항(자금세탁방지), 사용자 보안 설정, 미결제 거래 존재, 법적 분쟁 등의 이유로 설정될 수 있습니다.
///           이 예외는 사용자에게 출금 차단 사유를 명확하게 전달하며, 차단을 해제하기 위한 조치(본인 인증, 서류 제출 등)를 안내합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Security Pattern, Compliance Pattern
/// </summary>
public class WithdrawalBlockedException : DomainException
{
    public string BlockReason { get; }
    public DateTime? BlockedUntil { get; }

    public WithdrawalBlockedException(Guid accountId, string blockReason, DateTime? blockedUntil = null)
        : base($"출금이 차단되었습니다. AccountId: {accountId}, Reason: {blockReason}" +
              (blockedUntil.HasValue ? $", BlockedUntil: {blockedUntil.Value:yyyy-MM-dd HH:mm:ss}" : ""),
            accountId, "WITHDRAWAL_BLOCKED")
    {
        BlockReason = blockReason;
        BlockedUntil = blockedUntil;
    }
}
