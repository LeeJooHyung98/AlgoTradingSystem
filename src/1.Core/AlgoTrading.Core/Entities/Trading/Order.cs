using AlgoTrading.Core.ValueObjects;
using AlgoTrading.Core.DomainEvents;

namespace AlgoTrading.Core.Entities.Trading;

/// <summary>
/// description(설명) : Order Aggregate Root representing a trading order (매매 주문을 나타내는 주문 집합 루트)
/// Details(상세설명) : Manages buy/sell instructions with state machine pattern for order lifecycle (상태 머신 패턴을 사용하여 매수/매도 지시를 관리하고 주문 생명주기를 처리)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern, State Machine Pattern, Domain Events (집합 루트 패턴, 상태 머신 패턴, 도메인 이벤트)
/// </summary>
public sealed class Order : AggregateRoot<OrderId>
{
    /// <summary>
    /// 주문 종목 코드
    /// </summary>
    public StockCode StockCode { get; private set; }

    /// <summary>
    /// 주문 방향 (매수/매도)
    /// </summary>
    public OrderSide Side { get; private set; }

    /// <summary>
    /// 주문 유형 (시장가/지정가/손절)
    /// </summary>
    public OrderType Type { get; private set; }

    /// <summary>
    /// 주문 수량
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// 체결된 수량
    /// </summary>
    public int FilledQuantity { get; private set; }

    /// <summary>
    /// 남은 수량
    /// </summary>
    public int RemainingQuantity => Quantity - FilledQuantity;

    /// <summary>
    /// 지정가 (지정가 주문용)
    /// </summary>
    public decimal? LimitPrice { get; private set; }

    /// <summary>
    /// 손절가 (손절 주문용)
    /// </summary>
    public decimal? StopPrice { get; private set; }

    /// <summary>
    /// 평균 체결가
    /// </summary>
    public decimal? AverageFillPrice { get; private set; }

    /// <summary>
    /// 주문 상태
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// 주문 유효기간
    /// </summary>
    public TimeInForce TimeInForce { get; private set; }

    /// <summary>
    /// 주문 제출 시간
    /// </summary>
    public DateTime SubmittedAt { get; private set; }

    /// <summary>
    /// 주문 승인 시간 (증권사 승인)
    /// </summary>
    public DateTime? AcceptedAt { get; private set; }

    /// <summary>
    /// 주문 완료 시간
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// 주문 취소 시간
    /// </summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>
    /// 증권사 주문 ID
    /// </summary>
    public string? BrokerOrderId { get; private set; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; private set; }

    /// <summary>
    /// 주문 출처 (수동/전략/API)
    /// </summary>
    public OrderSource Source { get; private set; }

    /// <summary>
    /// 전략 ID (전략에서 발생한 주문인 경우)
    /// </summary>
    public Guid? StrategyId { get; private set; }

    /// <summary>
    /// 취소 사유
    /// </summary>
    public string? CancellationReason { get; private set; }

    /// <summary>
    /// 거부 사유
    /// </summary>
    public string? RejectionReason { get; private set; }

    private Order() { } // EF Core

    private Order(
        OrderId id,
        StockCode stockCode,
        OrderSide side,
        OrderType type,
        int quantity,
        string accountNumber,
        OrderSource source,
        decimal? limitPrice = null,
        decimal? stopPrice = null,
        Guid? strategyId = null)
    {
        Id = id;
        StockCode = stockCode;
        Side = side;
        Type = type;
        Quantity = quantity;
        AccountNumber = accountNumber;
        Source = source;
        LimitPrice = limitPrice;
        StopPrice = stopPrice;
        StrategyId = strategyId;

        Status = OrderStatus.PendingSubmit;
        TimeInForce = TimeInForce.Day;
        FilledQuantity = 0;
        SubmittedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// description(설명) : Create a market order (시장가 주문 생성)
    /// Details(상세설명) : Factory method to create a new market order with validation (검증을 포함한 새로운 시장가 주문을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern, Domain-Driven Design (팩토리 패턴, 도메인 주도 설계)
    /// </summary>
    /// <returns>Created market order instance (생성된 시장가 주문 인스턴스)</returns>
    public static Order CreateMarketOrder(
        StockCode stockCode,
        OrderSide side,
        int quantity,
        string accountNumber,
        OrderSource source = OrderSource.Manual,
        Guid? strategyId = null)
    {
        ValidateQuantity(quantity);

        var id = new OrderId(Guid.NewGuid());
        return new Order(id, stockCode, side, OrderType.Market, quantity, accountNumber, source, strategyId: strategyId);
    }

    /// <summary>
    /// description(설명) : Create a limit order (지정가 주문 생성)
    /// Details(상세설명) : Factory method to create a new limit order with price validation (가격 검증을 포함한 새로운 지정가 주문을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern, Domain-Driven Design (팩토리 패턴, 도메인 주도 설계)
    /// </summary>
    /// <returns>Created limit order instance (생성된 지정가 주문 인스턴스)</returns>
    public static Order CreateLimitOrder(
        StockCode stockCode,
        OrderSide side,
        int quantity,
        decimal limitPrice,
        string accountNumber,
        OrderSource source = OrderSource.Manual,
        Guid? strategyId = null)
    {
        ValidateQuantity(quantity);
        ValidatePrice(limitPrice);

        var id = new OrderId(Guid.NewGuid());
        return new Order(id, stockCode, side, OrderType.Limit, quantity, accountNumber, source, limitPrice: limitPrice, strategyId: strategyId);
    }

    /// <summary>
    /// description(설명) : Create a stop order (손절 주문 생성)
    /// Details(상세설명) : Factory method to create a new stop order with stop price validation (손절가 검증을 포함한 새로운 손절 주문을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern, Domain-Driven Design (팩토리 패턴, 도메인 주도 설계)
    /// </summary>
    /// <returns>Created stop order instance (생성된 손절 주문 인스턴스)</returns>
    public static Order CreateStopOrder(
        StockCode stockCode,
        OrderSide side,
        int quantity,
        decimal stopPrice,
        string accountNumber,
        OrderSource source = OrderSource.Manual,
        Guid? strategyId = null)
    {
        ValidateQuantity(quantity);
        ValidatePrice(stopPrice);

        var id = new OrderId(Guid.NewGuid());
        return new Order(id, stockCode, side, OrderType.Stop, quantity, accountNumber, source, stopPrice: stopPrice, strategyId: strategyId);
    }

    /// <summary>
    /// description(설명) : Submit order to broker (증권사로 주문 제출)
    /// Details(상세설명) : Changes order status to Submitted and raises domain event (주문 상태를 제출됨으로 변경하고 도메인 이벤트를 발생시킴)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern, Domain Events (상태 머신 패턴, 도메인 이벤트)
    /// </summary>
    /// <returns></returns>
    public void Submit()
    {
        if (Status != OrderStatus.PendingSubmit)
            throw new InvalidOperationException($"Cannot submit order in {Status} status");

        Status = OrderStatus.Submitted;
        AddDomainEvent(new OrderSubmittedEvent(Id, StockCode, Side, Type, Quantity, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Accept order by broker (증권사에 의한 주문 승인)
    /// Details(상세설명) : Updates order status to Accepted and stores broker order ID (주문 상태를 승인됨으로 업데이트하고 증권사 주문 ID를 저장)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern, Domain Events (상태 머신 패턴, 도메인 이벤트)
    /// </summary>
    /// <returns></returns>
    public void Accept(string brokerOrderId)
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException($"Cannot accept order in {Status} status");

        Status = OrderStatus.Accepted;
        BrokerOrderId = brokerOrderId;
        AcceptedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderAcceptedEvent(Id, BrokerOrderId, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Fill order partially or fully (주문 부분 또는 전체 체결)
    /// Details(상세설명) : Updates filled quantity, calculates average fill price, and changes status accordingly (체결 수량을 업데이트하고 평균 체결가를 계산하며 상태를 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern, Domain Events (상태 머신 패턴, 도메인 이벤트)
    /// </summary>
    /// <returns></returns>
    public void Fill(int quantity, decimal price, DateTime fillTime)
    {
        if (Status != OrderStatus.Accepted && Status != OrderStatus.PartiallyFilled)
            throw new InvalidOperationException($"Cannot fill order in {Status} status");

        if (quantity <= 0 || quantity > RemainingQuantity)
            throw new ArgumentException($"Invalid fill quantity: {quantity}. Remaining: {RemainingQuantity}", nameof(quantity));

        FilledQuantity += quantity;

        // Update average fill price
        if (AverageFillPrice.HasValue)
        {
            var totalCost = (AverageFillPrice.Value * (FilledQuantity - quantity)) + (price * quantity);
            AverageFillPrice = totalCost / FilledQuantity;
        }
        else
        {
            AverageFillPrice = price;
        }

        // Update status
        if (FilledQuantity == Quantity)
        {
            Status = OrderStatus.Filled;
            CompletedAt = fillTime;
            AddDomainEvent(new OrderFilledEvent(Id, Quantity, AverageFillPrice.Value, fillTime));
        }
        else
        {
            Status = OrderStatus.PartiallyFilled;
            AddDomainEvent(new OrderPartiallyFilledEvent(Id, quantity, price, FilledQuantity, RemainingQuantity, fillTime));
        }

        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Cancel order (주문 취소)
    /// Details(상세설명) : Sets order status to Cancelled and records cancellation reason (주문 상태를 취소됨으로 설정하고 취소 사유를 기록)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern, Domain Events (상태 머신 패턴, 도메인 이벤트)
    /// </summary>
    /// <returns></returns>
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Filled || Status == OrderStatus.Cancelled || Status == OrderStatus.Rejected)
            throw new InvalidOperationException($"Cannot cancel order in {Status} status");

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledEvent(Id, reason, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Reject order (주문 거부)
    /// Details(상세설명) : Sets order status to Rejected and records rejection reason (주문 상태를 거부됨으로 설정하고 거부 사유를 기록)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern, Domain Events (상태 머신 패턴, 도메인 이벤트)
    /// </summary>
    /// <returns></returns>
    public void Reject(string reason)
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException($"Cannot reject order in {Status} status");

        Status = OrderStatus.Rejected;
        RejectionReason = reason;

        AddDomainEvent(new OrderRejectedEvent(Id, reason, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if order is active (주문이 활성 상태인지 확인)
    /// Details(상세설명) : Returns true if order status is Submitted, Accepted, or PartiallyFilled (주문 상태가 제출됨, 승인됨, 또는 부분체결인 경우 true 반환)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>True if order is active (주문이 활성 상태이면 true)</returns>
    public bool IsActive()
    {
        return Status == OrderStatus.Submitted ||
               Status == OrderStatus.Accepted ||
               Status == OrderStatus.PartiallyFilled;
    }

    /// <summary>
    /// description(설명) : Check if order is completed (주문이 완료 상태인지 확인)
    /// Details(상세설명) : Returns true if order status is Filled, Cancelled, or Rejected (주문 상태가 체결됨, 취소됨, 또는 거부됨인 경우 true 반환)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>True if order is completed (주문이 완료 상태이면 true)</returns>
    public bool IsCompleted()
    {
        return Status == OrderStatus.Filled ||
               Status == OrderStatus.Cancelled ||
               Status == OrderStatus.Rejected;
    }

    /// <summary>
    /// description(설명) : Get order fill rate (주문 체결률 조회)
    /// Details(상세설명) : Calculates and returns the percentage of filled quantity (체결된 수량의 백분율을 계산하여 반환)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>Fill rate as decimal (0.0 to 1.0) (체결률, 0.0~1.0)</returns>
    public decimal GetFillRate()
    {
        return Quantity > 0 ? (decimal)FilledQuantity / Quantity : 0;
    }

    /// <summary>
    /// description(설명) : Get estimated order value (예상 주문 금액 조회)
    /// Details(상세설명) : Calculates estimated order value using limit price or average fill price (지정가 또는 평균 체결가를 사용하여 예상 주문 금액을 계산)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern, Value Object Pattern (쿼리 메서드 패턴, 값 객체 패턴)
    /// </summary>
    /// <returns>Estimated order value as Money (Money 타입의 예상 주문 금액)</returns>
    public Money GetEstimatedValue()
    {
        var price = LimitPrice ?? AverageFillPrice ?? 0;
        return new Money(price * Quantity);
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
            throw new ArgumentException("Price must be positive", nameof(price));
    }
}

/// <summary>
/// description(설명) : Order ID value object (주문 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for Order aggregate (Order 집합의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class OrderId : GuidId
{
    public OrderId(Guid value) : base(value) { }

    public static OrderId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Order Side enumeration (주문 방향 열거형)
/// Details(상세설명) : Defines whether order is a buy or sell (주문이 매수인지 매도인지 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum OrderSide
{
    Buy = 1,    // 매수(Buy)
    Sell = 2    // 매도(Sell)
}

/// <summary>
/// description(설명) : Order Type enumeration (주문 유형 열거형)
/// Details(상세설명) : Defines the type of order execution (주문 실행 유형을 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum OrderType
{
    Market = 1,     // 시장가(Market Order)
    Limit = 2,      // 지정가(Limit Order)
    Stop = 3,       // 손절(Stop Order)
    StopLimit = 4   // 손절지정가(Stop Limit Order)
}

/// <summary>
/// description(설명) : Order Status enumeration (주문 상태 열거형)
/// Details(상세설명) : Represents the current lifecycle state of an order (주문의 현재 생명주기 상태를 나타냄)
/// Applied technology patterns(적용기술패턴) : State Machine Pattern, Enumeration Pattern (상태 머신 패턴, 열거형 패턴)
/// </summary>
public enum OrderStatus
{
    PendingSubmit = 1,    // 제출 대기(Pending Submit)
    Submitted = 2,        // 제출됨(Submitted)
    Accepted = 3,         // 승인됨(Accepted)
    PartiallyFilled = 4,  // 부분 체결(Partially Filled)
    Filled = 5,           // 완전 체결(Filled)
    Cancelled = 6,        // 취소됨(Cancelled)
    Rejected = 7          // 거부됨(Rejected)
}

/// <summary>
/// description(설명) : Time in Force enumeration (주문 유효기간 열거형)
/// Details(상세설명) : Specifies how long an order remains active (주문이 얼마나 오래 활성 상태로 유지되는지 지정)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum TimeInForce
{
    Day = 1,           // 당일 유효(Day Order)
    GTC = 2,           // 취소 시까지 유효(Good Till Cancelled)
    IOC = 3,           // 즉시 체결 또는 취소(Immediate or Cancel)
    FOK = 4            // 전량 체결 또는 취소(Fill or Kill)
}

/// <summary>
/// description(설명) : Order Source enumeration (주문 출처 열거형)
/// Details(상세설명) : Identifies the origin of the order (주문의 출처를 식별)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum OrderSource
{
    Manual = 1,        // 수동 주문(Manual Order)
    Strategy = 2,      // 전략 자동 주문(Strategy Automated Order)
    API = 3            // API 주문(API Order)
}

// Domain Events
public sealed class OrderSubmittedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public StockCode StockCode { get; }
    public OrderSide Side { get; }
    public OrderType Type { get; }
    public int Quantity { get; }

    public OrderSubmittedEvent(OrderId orderId, StockCode stockCode, OrderSide side, OrderType type, int quantity, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        OrderId = orderId;
        StockCode = stockCode;
        Side = side;
        Type = type;
        Quantity = quantity;
    }
}

public sealed class OrderAcceptedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public string BrokerOrderId { get; }

    public OrderAcceptedEvent(OrderId orderId, string brokerOrderId, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        OrderId = orderId;
        BrokerOrderId = brokerOrderId;
    }
}

public sealed class OrderFilledEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public int Quantity { get; }
    public decimal AveragePrice { get; }

    public OrderFilledEvent(OrderId orderId, int quantity, decimal averagePrice, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        OrderId = orderId;
        Quantity = quantity;
        AveragePrice = averagePrice;
    }
}

public sealed class OrderPartiallyFilledEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public int FilledQuantity { get; }
    public decimal Price { get; }
    public int TotalFilled { get; }
    public int Remaining { get; }

    public OrderPartiallyFilledEvent(OrderId orderId, int filledQuantity, decimal price, int totalFilled, int remaining, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        OrderId = orderId;
        FilledQuantity = filledQuantity;
        Price = price;
        TotalFilled = totalFilled;
        Remaining = remaining;
    }
}

public sealed class OrderCancelledEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public string Reason { get; }

    public OrderCancelledEvent(OrderId orderId, string reason, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        OrderId = orderId;
        Reason = reason;
    }
}

public sealed class OrderRejectedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public string Reason { get; }

    public OrderRejectedEvent(OrderId orderId, string reason, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        OrderId = orderId;
        Reason = reason;
    }
}
