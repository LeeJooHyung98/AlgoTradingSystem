namespace AlgoTrading.Core.Exceptions.Account;

/// <summary>
/// description : 요청한 계좌 ID를 찾을 수 없을 때 발생하는 예외
/// Details : Account Context에서 존재하지 않는 계좌를 조회, 수정, 삭제하려고 할 때 발생합니다.
///           잘못된 AccountId 또는 AccountNumber가 제공되거나, 계좌가 삭제된 후 참조되는 경우에 해당하며, 클라이언트에게 404 Not Found 응답을 반환하는 데 사용됩니다.
///           계좌는 모든 거래의 기반이 되는 핵심 엔티티이므로, 존재하지 않는 계좌 참조는 중대한 오류로 간주되어 감사 로그에 기록됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Exception Pattern, Not Found Pattern
/// </summary>
public class AccountNotFoundException : DomainException
{
    public AccountNotFoundException(Guid accountId)
        : base($"계좌를 찾을 수 없습니다. AccountId: {accountId}", accountId, "ACCOUNT_NOT_FOUND")
    {
    }

    public AccountNotFoundException(string accountNumber)
        : base($"계좌를 찾을 수 없습니다. AccountNumber: {accountNumber}", errorCode: "ACCOUNT_NOT_FOUND")
    {
    }
}
