namespace AlgoTrading.Core.Exceptions.Account;

/// <summary>
/// description : 출금 또는 이체 시 계좌 자금이 부족할 때 발생하는 예외
/// Details : Account Context에서 출금(Withdrawal) 또는 계좌 간 이체 시 현재 잔고(Balance)가 요청 금액보다 부족한 경우 발생합니다.
///           거래 주문 시 잔고 부족은 InsufficientBalanceException으로 처리되며, 이 예외는 순수한 자금 이동(출금, 이체) 시에만 사용됩니다.
///           이 예외는 계좌 잔고가 마이너스가 되는 것을 방지하는 핵심 불변성을 보호하며, 금융 시스템의 무결성을 보장합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Invariant Protection, Financial Compliance
/// </summary>
public class InsufficientFundsException : DomainException
{
    public decimal RequestedAmount { get; }
    public decimal AvailableFunds { get; }

    public InsufficientFundsException(Guid accountId, decimal requestedAmount, decimal availableFunds)
        : base($"계좌 자금이 부족합니다. AccountId: {accountId}, Requested: {requestedAmount:N2}, Available: {availableFunds:N2}",
            accountId, "INSUFFICIENT_FUNDS")
    {
        RequestedAmount = requestedAmount;
        AvailableFunds = availableFunds;
    }
}
