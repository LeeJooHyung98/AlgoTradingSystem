using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Portfolio;

/// <summary>
/// description(설명) : Transaction Entity - Represents a financial transaction in the portfolio (거래 엔티티 - 포트폴리오의 금융 거래를 나타냄)
/// Details(상세설명) : Records all portfolio transactions including deposits, withdrawals, stock trades, dividends, and fees (입출금, 주식 매매, 배당금, 수수료 등 모든 포트폴리오 거래 기록)
/// Applied technology patterns(적용기술패턴) : Entity Pattern (엔티티 패턴), Domain-Driven Design (도메인 주도 설계), Factory Pattern (팩토리 패턴)
/// </summary>
public sealed class Transaction : Entity<TransactionId>
{
    /// <summary>
    /// 포트폴리오 ID
    /// </summary>
    public PortfolioId PortfolioId { get; private set; }

    /// <summary>
    /// 거래 유형
    /// </summary>
    public TransactionType Type { get; private set; }

    /// <summary>
    /// 거래 금액
    /// </summary>
    public Money Amount { get; private set; }

    /// <summary>
    /// 종목 코드 (해당하는 경우)
    /// </summary>
    public StockCode? StockCode { get; private set; }

    /// <summary>
    /// 수량 (주식 거래인 경우)
    /// </summary>
    public int? Quantity { get; private set; }

    /// <summary>
    /// 가격 (주식 거래인 경우)
    /// </summary>
    public decimal? Price { get; private set; }

    /// <summary>
    /// 거래 설명
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// 거래 시간
    /// </summary>
    public DateTime TransactionDate { get; private set; }

    /// <summary>
    /// 연관된 주문 ID (해당하는 경우)
    /// </summary>
    public Guid? OrderId { get; private set; }

    /// <summary>
    /// 거래 후 잔액
    /// </summary>
    public Money BalanceAfter { get; private set; }

    private Transaction() { }

    private Transaction(
        TransactionId id,
        PortfolioId portfolioId,
        TransactionType type,
        Money amount,
        string description,
        DateTime transactionDate,
        Money balanceAfter)
    {
        Id = id;
        PortfolioId = portfolioId;
        Type = type;
        Amount = amount;
        Description = description;
        TransactionDate = transactionDate;
        BalanceAfter = balanceAfter;
    }

    /// <summary>
    /// description(설명) : Create a deposit transaction (입금 거래 생성)
    /// Details(상세설명) : Factory method to record cash deposit into portfolio (포트폴리오로의 현금 입금 기록하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created deposit Transaction (새로 생성된 입금 거래)</returns>
    public static Transaction CreateDeposit(
        PortfolioId portfolioId,
        Money amount,
        Money balanceAfter,
        string description = "Cash deposit")
    {
        var id = new TransactionId(Guid.NewGuid());
        return new Transaction(id, portfolioId, TransactionType.Deposit, amount, description, DateTime.UtcNow, balanceAfter);
    }

    /// <summary>
    /// description(설명) : Create a withdrawal transaction (출금 거래 생성)
    /// Details(상세설명) : Factory method to record cash withdrawal from portfolio (포트폴리오에서의 현금 출금 기록하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created withdrawal Transaction (새로 생성된 출금 거래)</returns>
    public static Transaction CreateWithdrawal(
        PortfolioId portfolioId,
        Money amount,
        Money balanceAfter,
        string description = "Cash withdrawal")
    {
        var id = new TransactionId(Guid.NewGuid());
        return new Transaction(id, portfolioId, TransactionType.Withdrawal, amount, description, DateTime.UtcNow, balanceAfter);
    }

    /// <summary>
    /// description(설명) : Create a buy transaction (매수 거래 생성)
    /// Details(상세설명) : Factory method to record stock purchase transaction (주식 매수 거래 기록하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created buy Transaction (새로 생성된 매수 거래)</returns>
    public static Transaction CreateBuy(
        PortfolioId portfolioId,
        StockCode stockCode,
        int quantity,
        decimal price,
        Money totalAmount,
        Money balanceAfter,
        Guid? orderId = null)
    {
        var id = new TransactionId(Guid.NewGuid());
        var transaction = new Transaction(
            id, portfolioId, TransactionType.Buy, totalAmount,
            $"Buy {quantity} shares of {stockCode}", DateTime.UtcNow, balanceAfter);

        transaction.StockCode = stockCode;
        transaction.Quantity = quantity;
        transaction.Price = price;
        transaction.OrderId = orderId;

        return transaction;
    }

    /// <summary>
    /// description(설명) : Create a sell transaction (매도 거래 생성)
    /// Details(상세설명) : Factory method to record stock sale transaction (주식 매도 거래 기록하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created sell Transaction (새로 생성된 매도 거래)</returns>
    public static Transaction CreateSell(
        PortfolioId portfolioId,
        StockCode stockCode,
        int quantity,
        decimal price,
        Money totalAmount,
        Money balanceAfter,
        Guid? orderId = null)
    {
        var id = new TransactionId(Guid.NewGuid());
        var transaction = new Transaction(
            id, portfolioId, TransactionType.Sell, totalAmount,
            $"Sell {quantity} shares of {stockCode}", DateTime.UtcNow, balanceAfter);

        transaction.StockCode = stockCode;
        transaction.Quantity = quantity;
        transaction.Price = price;
        transaction.OrderId = orderId;

        return transaction;
    }

    /// <summary>
    /// description(설명) : Create a dividend transaction (배당금 거래 생성)
    /// Details(상세설명) : Factory method to record dividend payment received (배당금 수령 기록하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created dividend Transaction (새로 생성된 배당금 거래)</returns>
    public static Transaction CreateDividend(
        PortfolioId portfolioId,
        StockCode stockCode,
        Money amount,
        Money balanceAfter)
    {
        var id = new TransactionId(Guid.NewGuid());
        var transaction = new Transaction(
            id, portfolioId, TransactionType.Dividend, amount,
            $"Dividend from {stockCode}", DateTime.UtcNow, balanceAfter);

        transaction.StockCode = stockCode;

        return transaction;
    }

    /// <summary>
    /// description(설명) : Create a fee transaction (수수료 거래 생성)
    /// Details(상세설명) : Factory method to record trading or management fees (거래 또는 관리 수수료 기록하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created fee Transaction (새로 생성된 수수료 거래)</returns>
    public static Transaction CreateFee(
        PortfolioId portfolioId,
        Money amount,
        Money balanceAfter,
        string description = "Trading fee")
    {
        var id = new TransactionId(Guid.NewGuid());
        return new Transaction(id, portfolioId, TransactionType.Fee, amount, description, DateTime.UtcNow, balanceAfter);
    }
}

/// <summary>
/// description(설명) : Transaction identifier value object (거래 식별자 값 객체)
/// Details(상세설명) : Strongly-typed identifier for Transaction entity using GUID (GUID를 사용한 거래 엔티티의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class TransactionId : GuidId
{
    public TransactionId(Guid value) : base(value) { }

    public static TransactionId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Transaction type enumeration (거래 유형 열거형)
/// Details(상세설명) : Categorizes all types of financial transactions in portfolio (포트폴리오의 모든 금융 거래 유형 분류)
/// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum TransactionType
{
    Deposit = 1,                // Cash deposit (현금 입금)
    Withdrawal = 2,             // Cash withdrawal (현금 출금)
    Buy = 3,                    // Stock purchase (주식 매수)
    Sell = 4,                   // Stock sale (주식 매도)
    Dividend = 5,               // Dividend received (배당금 수령)
    Interest = 6,               // Interest received (이자 수령)
    Fee = 7,                    // Trading fee (거래 수수료)
    Tax = 8                     // Tax payment (세금 납부)
}
