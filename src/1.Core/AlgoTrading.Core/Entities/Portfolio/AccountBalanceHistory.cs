using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Portfolio;

/// <summary>
/// description(설명) : Account Balance History entity for tracking balance changes over time (시간 경과에 따른 잔고 변화를 추적하는 계좌 잔고 히스토리 엔티티)
/// Details(상세설명) : Time-series data stored in TimescaleDB for historical balance tracking (TimescaleDB에 저장되는 시계열 데이터로 과거 잔고 추적)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, Time-Series Pattern (엔티티 패턴, 시계열 패턴)
/// </summary>
public sealed class AccountBalanceHistory : Entity<AccountBalanceHistoryId>
{
    /// <summary>
    /// 계좌 ID
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// 잔고 기준 일시
    /// </summary>
    public DateTime BalanceDate { get; private set; }

    /// <summary>
    /// 현금 잔고
    /// </summary>
    public Money CashBalance { get; private set; }

    /// <summary>
    /// 총 자산
    /// </summary>
    public Money TotalAssets { get; private set; }

    /// <summary>
    /// 총 평가 금액
    /// </summary>
    public Money TotalEvaluation { get; private set; }

    /// <summary>
    /// 총 손익
    /// </summary>
    public Money TotalProfitLoss { get; private set; }

    /// <summary>
    /// 기록 생성 일시
    /// </summary>
    public DateTime RecordedAt { get; private set; }

    private AccountBalanceHistory() { }

    private AccountBalanceHistory(
        AccountBalanceHistoryId id,
        AccountId accountId,
        DateTime balanceDate,
        Money cashBalance,
        Money totalAssets,
        Money totalEvaluation,
        Money totalProfitLoss)
    {
        Id = id;
        AccountId = accountId;
        BalanceDate = balanceDate;
        CashBalance = cashBalance;
        TotalAssets = totalAssets;
        TotalEvaluation = totalEvaluation;
        TotalProfitLoss = totalProfitLoss;
        RecordedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// description(설명) : Create a balance snapshot (잔고 스냅샷 생성)
    /// Details(상세설명) : Factory method to create a balance history record (잔고 히스토리 레코드를 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static AccountBalanceHistory Create(
        AccountId accountId,
        DateTime balanceDate,
        Money cashBalance,
        Money totalAssets,
        Money totalEvaluation,
        Money totalProfitLoss)
    {
        var id = AccountBalanceHistoryId.New();
        return new AccountBalanceHistory(
            id,
            accountId,
            balanceDate,
            cashBalance,
            totalAssets,
            totalEvaluation,
            totalProfitLoss);
    }
}

/// <summary>
/// description(설명) : Account Balance History ID value object (계좌 잔고 히스토리 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for AccountBalanceHistory (AccountBalanceHistory의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class AccountBalanceHistoryId : GuidId
{
    public AccountBalanceHistoryId(Guid value) : base(value) { }

    public static AccountBalanceHistoryId New() => new(Guid.NewGuid());
}
