namespace AlgoTrading.Core.DomainEvents.Account;

/// <summary>
/// description : 계좌에서 자금 출금이 요청되었을 때 발생하는 도메인 이벤트
/// Details : Account Context에서 사용자가 출금을 요청하면 Account 엔티티가 발행합니다. WithdrawalId(출금 요청 ID), Amount(출금 금액), RequestedAt(요청 시각)을 포함하여 출금 요청 정보를 제공합니다. 이벤트 구독자는 계좌 잔고와 출금 제한(WithdrawalBlock)을 검증하고, 열린 포지션이 있는지 확인하며, 외부 은행 API를 호출하여 출금을 처리합니다. 출금 처리는 Saga Pattern으로 관리되며, 각 단계(검증, 승인, 실행)의 성공/실패에 따라 후속 이벤트(WithdrawalCompletedEvent, WithdrawalFailedEvent)가 발행됩니다.
/// Applied technology patterns : Domain Event Pattern, Saga Pattern, Withdrawal Workflow Pattern
/// </summary>
public class WithdrawalRequestedEvent : DomainEvent
{
    /// <summary>
    /// 출금 요청 ID
    /// </summary>
    public Guid WithdrawalId { get; }

    /// <summary>
    /// 계좌 ID
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string AccountNumber { get; }

    /// <summary>
    /// 출금 금액
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// 요청 시간
    /// </summary>
    public DateTime RequestedAt { get; }

    public WithdrawalRequestedEvent(Guid withdrawalId, Guid accountId, string accountNumber, decimal amount, DateTime requestedAt)
    {
        WithdrawalId = withdrawalId;
        AccountId = accountId;
        AccountNumber = accountNumber;
        Amount = amount;
        RequestedAt = requestedAt;
    }
}
