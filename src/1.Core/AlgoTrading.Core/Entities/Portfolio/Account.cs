using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Portfolio;

/// <summary>
/// description(설명) : Account Aggregate Root representing a trading account (거래 계좌를 나타내는 계좌 집합 루트)
/// Details(상세설명) : Manages account information, balances, buying power, and account lifecycle status (계좌 정보, 잔액, 매수력, 계좌 생명주기 상태를 관리)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern, Domain-Driven Design (집합 루트 패턴, 도메인 주도 설계)
/// </summary>
public sealed class Account : AggregateRoot<AccountId>
{
    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; private set; }

    /// <summary>
    /// 계좌 이름
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 계좌 유형
    /// </summary>
    public AccountType Type { get; private set; }

    /// <summary>
    /// 증권사 이름
    /// </summary>
    public string BrokerName { get; private set; }

    /// <summary>
    /// 계좌 상태
    /// </summary>
    public AccountStatus Status { get; private set; }

    /// <summary>
    /// 총 자산 (현금 + 포지션 가치)
    /// </summary>
    public Money TotalEquity { get; private set; }

    /// <summary>
    /// 현금 잔액
    /// </summary>
    public Money CashBalance { get; private set; }

    /// <summary>
    /// 매수력
    /// </summary>
    public Money BuyingPower { get; private set; }

    /// <summary>
    /// 계좌 소유자 사용자 ID
    /// </summary>
    public string OwnerId { get; private set; }

    /// <summary>
    /// 계좌 개설 시간
    /// </summary>
    public DateTime OpenedAt { get; private set; }

    /// <summary>
    /// 마지막 업데이트 시간
    /// </summary>
    public DateTime LastUpdatedAt { get; private set; }

    private Account() { }

    private Account(
        AccountId id,
        string accountNumber,
        string name,
        AccountType type,
        string brokerName,
        string ownerId)
    {
        Id = id;
        AccountNumber = accountNumber;
        Name = name;
        Type = type;
        BrokerName = brokerName;
        OwnerId = ownerId;
        Status = AccountStatus.Active;
        TotalEquity = Money.Zero;
        CashBalance = Money.Zero;
        BuyingPower = Money.Zero;
        OpenedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// description(설명) : Create a new trading account (새로운 거래 계좌 생성)
    /// Details(상세설명) : Factory method to create an account with broker and owner information (증권사 및 소유자 정보로 계좌를 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern, Domain-Driven Design (팩토리 패턴, 도메인 주도 설계)
    /// </summary>
    /// <returns>Created account instance (생성된 계좌 인스턴스)</returns>
    public static Account Create(
        string accountNumber,
        string name,
        AccountType type,
        string brokerName,
        string ownerId)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be empty", nameof(name));

        var id = new AccountId(Guid.NewGuid());
        return new Account(id, accountNumber, name, type, brokerName, ownerId);
    }

    /// <summary>
    /// description(설명) : Update account balances (계좌 잔액 업데이트)
    /// Details(상세설명) : Updates total equity, cash balance, and buying power from broker feed (증권사 피드로부터 총 자산, 현금 잔액, 매수력을 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateBalances(Money totalEquity, Money cashBalance, Money buyingPower)
    {
        TotalEquity = totalEquity;
        CashBalance = cashBalance;
        BuyingPower = buyingPower;
        LastUpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Suspend account (계좌 정지)
    /// Details(상세설명) : Changes account status to Suspended, preventing trading operations (거래 작업을 방지하여 계좌 상태를 정지로 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Suspend(string reason)
    {
        Status = AccountStatus.Suspended;
        LastUpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Activate account (계좌 활성화)
    /// Details(상세설명) : Changes account status to Active, enabling trading operations (거래 작업을 가능하게 하여 계좌 상태를 활성으로 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Activate()
    {
        Status = AccountStatus.Active;
        LastUpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Close account (계좌 폐쇄)
    /// Details(상세설명) : Changes account status to Closed, permanently disabling the account (계좌를 영구적으로 비활성화하여 계좌 상태를 폐쇄로 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Close()
    {
        Status = AccountStatus.Closed;
        LastUpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }
}

/// <summary>
/// description(설명) : Account ID value object (계좌 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for Account aggregate (Account 집합의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class AccountId : GuidId
{
    public AccountId(Guid value) : base(value) { }

    public static AccountId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Account Type enumeration (계좌 유형 열거형)
/// Details(상세설명) : Defines the type of trading account (거래 계좌의 유형을 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum AccountType
{
    Cash = 1,                   // Cash account (현금 계좌)
    Margin = 2,                 // Margin account (마진 계좌)
    Paper = 3                   // Paper trading account (모의 거래 계좌)
}

/// <summary>
/// description(설명) : Account Status enumeration (계좌 상태 열거형)
/// Details(상세설명) : Represents the current state of an account (계좌의 현재 상태를 나타냄)
/// Applied technology patterns(적용기술패턴) : State Machine Pattern, Enumeration Pattern (상태 머신 패턴, 열거형 패턴)
/// </summary>
public enum AccountStatus
{
    Active = 1,                 // Active for trading (거래 활성)
    Suspended = 2,              // Temporarily suspended (일시 정지)
    Closed = 3,                 // Permanently closed (영구 폐쇄)
    PendingApproval = 4         // Waiting for approval (승인 대기 중)
}
