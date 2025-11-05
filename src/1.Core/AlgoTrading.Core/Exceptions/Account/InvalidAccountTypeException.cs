namespace AlgoTrading.Core.Exceptions.Account;

/// <summary>
/// description : 계좌 유형이 요청한 작업에 적합하지 않을 때 발생하는 예외
/// Details : Account 엔티티의 AccountType(Cash, Margin, ISA 등)에 따라 허용되는 작업이 다릅니다.
///           예: Cash 계좌에서 신용 거래 시도, ISA 계좌에서 파생상품 거래 시도, Retirement 계좌에서 조기 출금 시도 등.
///           이 예외는 계좌 유형별 규제 요구사항과 비즈니스 규칙을 강제하며, 사용자에게 올바른 계좌 유형을 안내합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Account Type Pattern, Compliance Pattern
/// </summary>
public class InvalidAccountTypeException : DomainException
{
    public string AccountType { get; }
    public string AttemptedAction { get; }

    public InvalidAccountTypeException(Guid accountId, string accountType, string attemptedAction)
        : base($"계좌 유형이 작업에 적합하지 않습니다. AccountId: {accountId}, AccountType: {accountType}, AttemptedAction: {attemptedAction}",
            accountId, "INVALID_ACCOUNT_TYPE")
    {
        AccountType = accountType;
        AttemptedAction = attemptedAction;
    }
}
