using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Trading;

/// <summary>
/// description(설명) : Execution Entity - Represents a single order fill or partial fill (체결 엔티티 - 단일 주문 체결 또는 부분 체결을 나타냄)
/// Details(상세설명) : Records each individual execution event from broker with price, quantity, and commission details (가격, 수량, 수수료 상세정보와 함께 증권사로부터의 각 체결 이벤트 기록)
/// Applied technology patterns(적용기술패턴) : Entity Pattern (엔티티 패턴), Domain-Driven Design (도메인 주도 설계), Factory Pattern (팩토리 패턴)
/// </summary>
public sealed class Execution : Entity<ExecutionId>
{
    /// <summary>
    /// 체결된 주문 ID
    /// </summary>
    public OrderId OrderId { get; private set; }

    /// <summary>
    /// 종목 코드
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// 체결 방향 (매수/매도)
    /// </summary>
    public OrderSide Side { get; private set; }

    /// <summary>
    /// 체결 수량
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// 체결 가격
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// 체결 금액 (가격 * 수량)
    /// </summary>
    public Money Value => new Money(Price * Quantity);

    /// <summary>
    /// 체결 수수료
    /// </summary>
    public Money Commission { get; private set; }

    /// <summary>
    /// 체결 시간
    /// </summary>
    public DateTime ExecutionTime { get; private set; }

    /// <summary>
    /// 증권사 체결 ID
    /// </summary>
    public string? BrokerExecutionId { get; private set; }

    /// <summary>
    /// 체결 거래소
    /// </summary>
    public string? ExecutionVenue { get; private set; }

    /// <summary>
    /// 부분 체결 여부
    /// </summary>
    public bool IsPartialFill { get; private set; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; private set; }

    private Execution() { } // EF Core

    private Execution(
        ExecutionId id,
        OrderId orderId,
        StockCode stockCode,
        OrderSide side,
        int quantity,
        decimal price,
        Money commission,
        DateTime executionTime,
        string accountNumber,
        bool isPartialFill = false,
        string? brokerExecutionId = null,
        string? executionVenue = null)
    {
        Id = id;
        OrderId = orderId;
        StockCode = stockCode;
        Side = side;
        Quantity = quantity;
        Price = price;
        Commission = commission;
        ExecutionTime = executionTime;
        AccountNumber = accountNumber;
        IsPartialFill = isPartialFill;
        BrokerExecutionId = brokerExecutionId;
        ExecutionVenue = executionVenue;
    }

    /// <summary>
    /// description(설명) : Create a new execution record (새 체결 기록 생성)
    /// Details(상세설명) : Factory method to create an execution record when order is filled by broker (증권사가 주문을 체결할 때 체결 기록을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created Execution instance (새로 생성된 체결 인스턴스)</returns>
    public static Execution Create(
        OrderId orderId,
        StockCode stockCode,
        OrderSide side,
        int quantity,
        decimal price,
        Money commission,
        DateTime executionTime,
        string accountNumber,
        bool isPartialFill = false,
        string? brokerExecutionId = null,
        string? executionVenue = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (price <= 0)
            throw new ArgumentException("Price must be positive", nameof(price));

        var id = new ExecutionId(Guid.NewGuid());
        return new Execution(
            id,
            orderId,
            stockCode,
            side,
            quantity,
            price,
            commission,
            executionTime,
            accountNumber,
            isPartialFill,
            brokerExecutionId,
            executionVenue);
    }

    /// <summary>
    /// description(설명) : Get net execution value after commission (수수료 차감 후 순 체결 금액 조회)
    /// Details(상세설명) : Calculates total cost including commission based on buy or sell side (매수/매도에 따라 수수료를 포함한 총 비용 계산)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Net value after commission (수수료 차감 후 순 금액)</returns>
    public Money GetNetValue()
    {
        return Side == OrderSide.Buy
            ? Value + Commission
            : Value - Commission;
    }

    /// <summary>
    /// description(설명) : Get effective price including commission (수수료를 포함한 실효 가격 조회)
    /// Details(상세설명) : Calculates per-share price including commission costs (수수료 비용을 포함한 주당 가격 계산)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Effective price per share (주당 실효 가격)</returns>
    public decimal GetEffectivePrice()
    {
        var totalCost = GetNetValue().Amount;
        return totalCost / Quantity;
    }
}

/// <summary>
/// description(설명) : Execution identifier value object (체결 식별자 값 객체)
/// Details(상세설명) : Strongly-typed identifier for Execution entity using GUID (GUID를 사용한 체결 엔티티의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class ExecutionId : GuidId
{
    public ExecutionId(Guid value) : base(value) { }

    public static ExecutionId New() => new(Guid.NewGuid());
}
