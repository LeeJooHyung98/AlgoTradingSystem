namespace AlgoTrading.Core.Exceptions.Trading;

/// <summary>
/// description : 주문 실행에 필요한 잔고가 부족할 때 발생하는 예외
/// Details : Order 엔티티 생성 또는 체결 시 Account의 가용 잔고(AvailableBalance)가 주문 금액보다 부족한 경우 발생합니다.
///           매수 주문의 경우 (Price × Quantity) + 수수료가 AvailableBalance를 초과하면 발생하며,
///           매도 주문의 경우 보유 수량이 부족하면 발생합니다(엄밀히는 InsufficientPositionException이 더 정확).
///           이 예외는 금융 시스템의 핵심 불변성(과도한 매수 방지, 마이너스 잔고 방지)을 보호합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Invariant Protection, Financial Compliance
/// </summary>
public class InsufficientBalanceException : DomainException
{
    public decimal RequiredAmount { get; }
    public decimal AvailableBalance { get; }

    public InsufficientBalanceException(Guid accountId, decimal requiredAmount, decimal availableBalance)
        : base($"잔고가 부족합니다. AccountId: {accountId}, Required: {requiredAmount:N2}, Available: {availableBalance:N2}",
            accountId, "INSUFFICIENT_BALANCE")
    {
        RequiredAmount = requiredAmount;
        AvailableBalance = availableBalance;
    }
}
