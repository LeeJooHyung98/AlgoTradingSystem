using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Portfolio;

/// <summary>
/// description(설명) : Account Withdrawal entity for managing withdrawal requests (출금 요청을 관리하는 계좌 출금 엔티티)
/// Details(상세설명) : Tracks withdrawal requests from account to external bank account (계좌에서 외부 은행계좌로의 출금 요청을 추적)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, State Machine Pattern (엔티티 패턴, 상태 머신 패턴)
/// </summary>
public sealed class AccountWithdrawal : Entity<AccountWithdrawalId>
{
    /// <summary>
    /// 계좌 ID
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// 출금 금액
    /// </summary>
    public Money Amount { get; private set; }

    /// <summary>
    /// 은행 코드 (예: 004-KB국민은행, 088-신한은행)
    /// </summary>
    public string BankCode { get; private set; }

    /// <summary>
    /// 출금 대상 은행 계좌번호
    /// </summary>
    public string BankAccountNumber { get; private set; }

    /// <summary>
    /// 출금 계좌 예금주명
    /// </summary>
    public string AccountHolderName { get; private set; }

    /// <summary>
    /// 출금 상태
    /// </summary>
    public WithdrawalStatus Status { get; private set; }

    /// <summary>
    /// 출금 요청 생성 일시
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 출금 승인 일시
    /// </summary>
    public DateTime? ApprovedAt { get; private set; }

    /// <summary>
    /// 출금 처리 시작 일시
    /// </summary>
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// 출금 완료 일시
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// 출금 실패 사유 (실패시에만)
    /// </summary>
    public string? FailureReason { get; private set; }

    private AccountWithdrawal() { }

    private AccountWithdrawal(
        AccountWithdrawalId id,
        AccountId accountId,
        Money amount,
        string bankCode,
        string bankAccountNumber,
        string accountHolderName)
    {
        Id = id;
        AccountId = accountId;
        Amount = amount;
        BankCode = bankCode;
        BankAccountNumber = bankAccountNumber;
        AccountHolderName = accountHolderName;
        Status = WithdrawalStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// description(설명) : Request a withdrawal (출금 요청)
    /// Details(상세설명) : Factory method to create a withdrawal request (출금 요청을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static AccountWithdrawal Request(
        AccountId accountId,
        Money amount,
        string bankCode,
        string bankAccountNumber,
        string accountHolderName)
    {
        if (amount.Amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));

        if (string.IsNullOrWhiteSpace(bankCode))
            throw new ArgumentException("Bank code cannot be empty", nameof(bankCode));

        if (string.IsNullOrWhiteSpace(bankAccountNumber))
            throw new ArgumentException("Bank account number cannot be empty", nameof(bankAccountNumber));

        if (string.IsNullOrWhiteSpace(accountHolderName))
            throw new ArgumentException("Account holder name cannot be empty", nameof(accountHolderName));

        var id = AccountWithdrawalId.New();
        return new AccountWithdrawal(
            id,
            accountId,
            amount,
            bankCode,
            bankAccountNumber,
            accountHolderName);
    }

    /// <summary>
    /// description(설명) : Approve withdrawal request (출금 요청 승인)
    /// Details(상세설명) : Changes status to Approved (상태를 승인됨으로 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Approve()
    {
        if (Status != WithdrawalStatus.Pending)
            throw new InvalidOperationException($"Cannot approve withdrawal in {Status} status");

        Status = WithdrawalStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Start processing withdrawal (출금 처리 시작)
    /// Details(상세설명) : Changes status to Processing (상태를 처리중으로 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void StartProcessing()
    {
        if (Status != WithdrawalStatus.Approved)
            throw new InvalidOperationException($"Cannot process withdrawal in {Status} status");

        Status = WithdrawalStatus.Processing;
        ProcessedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Complete withdrawal (출금 완료)
    /// Details(상세설명) : Changes status to Completed (상태를 완료로 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Complete()
    {
        if (Status != WithdrawalStatus.Processing)
            throw new InvalidOperationException($"Cannot complete withdrawal in {Status} status");

        Status = WithdrawalStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Fail withdrawal (출금 실패)
    /// Details(상세설명) : Changes status to Failed with reason (상태를 실패로 변경하고 사유 기록)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Fail(string reason)
    {
        if (Status == WithdrawalStatus.Completed || Status == WithdrawalStatus.Cancelled)
            throw new InvalidOperationException($"Cannot fail withdrawal in {Status} status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be empty", nameof(reason));

        Status = WithdrawalStatus.Failed;
        FailureReason = reason;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Cancel withdrawal (출금 취소)
    /// Details(상세설명) : Changes status to Cancelled (상태를 취소로 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Cancel()
    {
        if (Status == WithdrawalStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed withdrawal");

        if (Status == WithdrawalStatus.Cancelled)
            throw new InvalidOperationException("Withdrawal is already cancelled");

        Status = WithdrawalStatus.Cancelled;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if withdrawal can be cancelled (출금을 취소할 수 있는지 확인)
    /// Details(상세설명) : Returns true if withdrawal is in Pending or Approved status (출금이 대기 또는 승인 상태이면 true 반환)
    /// </summary>
    public bool CanCancel()
    {
        return Status == WithdrawalStatus.Pending || Status == WithdrawalStatus.Approved;
    }
}

/// <summary>
/// description(설명) : Account Withdrawal ID value object (계좌 출금 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for AccountWithdrawal (AccountWithdrawal의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class AccountWithdrawalId : GuidId
{
    public AccountWithdrawalId(Guid value) : base(value) { }

    public static AccountWithdrawalId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Withdrawal Status enumeration (출금 상태 열거형)
/// Details(상세설명) : Represents the current state of a withdrawal request (출금 요청의 현재 상태를 나타냄)
/// Applied technology patterns(적용기술패턴) : State Machine Pattern, Enumeration Pattern (상태 머신 패턴, 열거형 패턴)
/// </summary>
public enum WithdrawalStatus
{
    Pending = 1,        // 대기중
    Approved = 2,       // 승인됨
    Processing = 3,     // 처리중
    Completed = 4,      // 완료
    Failed = 5,         // 실패
    Cancelled = 6       // 취소
}
