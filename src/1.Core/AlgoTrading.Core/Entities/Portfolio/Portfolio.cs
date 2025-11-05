using AlgoTrading.Core.ValueObjects;
using AlgoTrading.Core.DomainEvents;
using AlgoTrading.Core.Entities.Trading;

namespace AlgoTrading.Core.Entities.Portfolio;

/// <summary>
/// description(설명) : Portfolio Aggregate Root representing a collection of positions and cash (포지션과 현금의 집합을 나타내는 포트폴리오 집합 루트)
/// Details(상세설명) : Manages positions, cash balance, and tracks realized/unrealized P&L with total value calculation (포지션, 현금 잔액을 관리하고 실현/미실현 손익과 총 가치를 계산하여 추적)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern, Domain-Driven Design (집합 루트 패턴, 도메인 주도 설계)
/// </summary>
public sealed class Portfolio : AggregateRoot<PortfolioId>
{
    private readonly List<Position> _positions = new();

    /// <summary>
    /// 포트폴리오 이름
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 계좌 ID
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// 현금 잔액
    /// </summary>
    public Money CashBalance { get; private set; }

    /// <summary>
    /// 총 가치 (현금 + 포지션)
    /// </summary>
    public Money TotalValue => CashBalance + new Money(GetPositionsValue());

    /// <summary>
    /// 포지션 목록
    /// </summary>
    public IReadOnlyList<Position> Positions => _positions.AsReadOnly();

    /// <summary>
    /// 총 미실현 손익
    /// </summary>
    public Money UnrealizedPL => new Money(_positions.Sum(p => p.UnrealizedPL.Amount));

    /// <summary>
    /// 총 실현 손익 (전체 기간)
    /// </summary>
    public Money RealizedPL { get; private set; }

    /// <summary>
    /// 초기 자본
    /// </summary>
    public Money InitialCapital { get; private set; }

    /// <summary>
    /// 총 수익률 (퍼센트)
    /// </summary>
    public decimal TotalReturnPercent => InitialCapital.Amount > 0
        ? ((TotalValue.Amount - InitialCapital.Amount) / InitialCapital.Amount) * 100
        : 0;

    private Portfolio()
    {
        RealizedPL = Money.Zero;
    }

    private Portfolio(
        PortfolioId id,
        string name,
        AccountId accountId,
        Money initialCapital)
    {
        Id = id;
        Name = name;
        AccountId = accountId;
        InitialCapital = initialCapital;
        CashBalance = initialCapital;
        RealizedPL = Money.Zero;
    }

    /// <summary>
    /// description(설명) : Create a new portfolio (새로운 포트폴리오 생성)
    /// Details(상세설명) : Factory method to create a portfolio with initial capital (초기 자본으로 포트폴리오를 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern, Domain-Driven Design (팩토리 패턴, 도메인 주도 설계)
    /// </summary>
    /// <returns>Created portfolio instance (생성된 포트폴리오 인스턴스)</returns>
    public static Portfolio Create(
        string name,
        AccountId accountId,
        Money initialCapital)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Portfolio name cannot be empty", nameof(name));

        if (initialCapital.Amount <= 0)
            throw new ArgumentException("Initial capital must be positive");

        var id = new PortfolioId(Guid.NewGuid());
        return new Portfolio(id, name, accountId, initialCapital);
    }

    /// <summary>
    /// description(설명) : Add a position to the portfolio (포트폴리오에 포지션 추가)
    /// Details(상세설명) : Adds a new position to the portfolio's position collection (포트폴리오의 포지션 컬렉션에 새 포지션을 추가)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void AddPosition(Position position)
    {
        _positions.Add(position);
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Remove a position from the portfolio (포트폴리오에서 포지션 제거)
    /// Details(상세설명) : Removes an existing position from the portfolio's position collection (포트폴리오의 포지션 컬렉션에서 기존 포지션을 제거)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void RemovePosition(Position position)
    {
        _positions.Remove(position);
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Deposit cash into portfolio (포트폴리오에 현금 입금)
    /// Details(상세설명) : Increases cash balance with validation (검증과 함께 현금 잔액을 증가)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Value Validation (도메인 주도 설계, 값 검증)
    /// </summary>
    public void Deposit(Money amount)
    {
        if (amount.Amount <= 0)
            throw new ArgumentException("Deposit amount must be positive");

        CashBalance += amount;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Withdraw cash from portfolio (포트폴리오에서 현금 출금)
    /// Details(상세설명) : Decreases cash balance with validation for sufficient funds (충분한 자금에 대한 검증과 함께 현금 잔액을 감소)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Value Validation (도메인 주도 설계, 값 검증)
    /// </summary>
    public void Withdraw(Money amount)
    {
        if (amount.Amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive");

        if (amount > CashBalance)
            throw new InvalidOperationException("Insufficient cash balance");

        CashBalance -= amount;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Record a realized profit or loss (실현 손익 기록)
    /// Details(상세설명) : Updates realized P&L and cash balance when position is closed (포지션 청산 시 실현 손익과 현금 잔액을 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void RecordRealizedPL(Money profitLoss)
    {
        RealizedPL += profitLoss;
        CashBalance += profitLoss;
        MarkAsModified();
    }

    /// <summary>
    /// Get total value of all positions
    /// </summary>
    private decimal GetPositionsValue()
    {
        return _positions.Sum(p => p.MarketValue.Amount);
    }

    /// <summary>
    /// description(설명) : Get available cash for trading (거래 가능한 현금 조회)
    /// Details(상세설명) : Returns current cash balance available for new trades (새로운 거래에 사용 가능한 현재 현금 잔액 반환)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>Available cash balance (사용 가능 현금 잔액)</returns>
    public Money GetAvailableCash()
    {
        return CashBalance;
    }

    /// <summary>
    /// description(설명) : Check if portfolio can afford a purchase (포트폴리오가 매수 가능한지 확인)
    /// Details(상세설명) : Validates if cash balance is sufficient for the specified amount (현금 잔액이 지정된 금액에 충분한지 검증)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>True if portfolio can afford the amount (금액을 감당할 수 있으면 true)</returns>
    public bool CanAfford(Money amount)
    {
        return CashBalance >= amount;
    }
}

/// <summary>
/// description(설명) : Portfolio ID value object (포트폴리오 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for Portfolio aggregate (Portfolio 집합의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class PortfolioId : GuidId
{
    public PortfolioId(Guid value) : base(value) { }

    public static PortfolioId New() => new(Guid.NewGuid());
}
