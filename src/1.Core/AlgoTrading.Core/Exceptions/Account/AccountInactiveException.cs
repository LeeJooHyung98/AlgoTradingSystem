namespace AlgoTrading.Core.Exceptions.Account;

/// <summary>
/// description : 비활성 상태의 계좌로 거래를 시도할 때 발생하는 예외
/// Details : Account 엔티티의 Status가 Active가 아닌 상태(Suspended, Closed, Dormant)에서 거래를 시도하면 발생합니다.
///           계좌는 규제 요구사항, 사용자 요청, 시스템 정책 등의 이유로 비활성화될 수 있으며, 비활성 계좌는 모든 거래가 차단됩니다.
///           이 예외는 비활성 계좌를 통한 무단 거래를 방지하며, 사용자에게 계좌 활성화가 필요함을 알립니다.
/// Applied technology patterns : Domain-Driven Design (DDD), State Validation Pattern, Security Pattern
/// </summary>
public class AccountInactiveException : DomainException
{
    public string AccountStatus { get; }

    public AccountInactiveException(Guid accountId, string accountStatus)
        : base($"비활성 상태의 계좌로 거래할 수 없습니다. AccountId: {accountId}, Status: {accountStatus}",
            accountId, "ACCOUNT_INACTIVE")
    {
        AccountStatus = accountStatus;
    }

    public AccountInactiveException(string accountNumber, string accountStatus)
        : base($"비활성 상태의 계좌로 거래할 수 없습니다. AccountNumber: {accountNumber}, Status: {accountStatus}",
            errorCode: "ACCOUNT_INACTIVE")
    {
        AccountStatus = accountStatus;
    }
}
