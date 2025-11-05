namespace AlgoTrading.Core.DomainEvents.Account;

/// <summary>
/// description : 계좌 잔고가 변경되었을 때 발생하는 도메인 이벤트
/// Details : 입출금, 거래 체결, 수수료 차감 등으로 잔고가 변경될 때 발행됩니다.
///           이전/현재 잔고와 변동 금액을 포함하여 잔고 추적과 감사(Audit)를 지원합니다.
/// Applied technology patterns : Domain Event Pattern, Event Sourcing, Audit Trail Pattern, CQRS
/// </summary>
public class AccountBalanceUpdatedEvent : DomainEvent
{
    /// <summary>
    /// 계좌 ID
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string AccountNumber { get; }

    /// <summary>
    /// 이전 잔고
    /// </summary>
    public decimal PreviousBalance { get; }

    /// <summary>
    /// 현재 잔고
    /// </summary>
    public decimal CurrentBalance { get; }

    /// <summary>
    /// 변동 금액
    /// </summary>
    public decimal ChangeAmount { get; }

    public AccountBalanceUpdatedEvent(Guid accountId, string accountNumber, decimal previousBalance, decimal currentBalance)
    {
        AccountId = accountId;
        AccountNumber = accountNumber;
        PreviousBalance = previousBalance;
        CurrentBalance = currentBalance;
        ChangeAmount = currentBalance - previousBalance;
    }
}
