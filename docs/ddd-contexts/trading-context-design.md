# Trading Context - DDD 상세 설계

## 📋 목차
1. [Context Overview](#1-context-overview)
2. [Domain Model 상세 설계](#2-domain-model-상세-설계)
3. [Business Rules & Invariants](#3-business-rules--invariants)
4. [Domain Events](#4-domain-events)
5. [Domain Services](#5-domain-services)
6. [Application Services](#6-application-services)
7. [Infrastructure Layer](#7-infrastructure-layer)
8. [API/Interface Specifications](#8-apiinterface-specifications)
9. [Order Execution Flow](#9-order-execution-flow)
10. [Error Handling & Validation](#10-error-handling--validation)
11. [External System Integration](#11-external-system-integration)
12. [Performance Considerations](#12-performance-considerations)

---

## 1. Context Overview

### 1.1 책임과 범위

**Trading Context**는 주문 생성, 관리, 실행, 포지션 관리, 거래 기록 관리 등 모든 거래 관련 업무를 담당하는 핵심 Bounded Context입니다.

#### 주요 책임
- ✅ 주문 생성 및 라이프사이클 관리
- ✅ 주문 검증 및 위험 검토
- ✅ 거래 실행 (시장가, 지정가, 조건부)
- ✅ 거래 체결 처리
- ✅ 포지션 추적 및 관리
- ✅ 거래 히스토리 기록
- ✅ 손절/익절 관리
- ✅ 거래 보상 및 롤백

#### 핵심 원칙
- **정확성**: 모든 거래는 정확하게 기록되어야 함
- **원자성**: 거래는 전체 또는 전무 원칙 적용
- **감시**: 모든 거래 전에 위험 검토
- **추적성**: 모든 거래의 완전한 감사 기록
- **회복력**: 네트워크 장애 시 자동 재시도
- **성능**: 저지연 주문 처리 (<100ms)

### 1.2 Context Map

```
┌────────────────────────────────────────────┐
│          Trading Context                   │
│        (거래 실행 관리)                    │
└────────────────────────────────────────────┘
    ↓ 주문 검증      ↑ 거래 결과      ↓ 포지션
    │                │               │
    ↓                ↑               ↓
Account Context   Market Data    Strategy Context
(잔고 차감)       Context        (신호 기반)
                  (시세 확인)
    ↓                               ↑
    │ 포지션 손익                    │
    │ 실현 손익                     │ 거래 기록
    ↓                               │
Risk Management ←────────────────┘
(거래 전 검증)
```

### 1.3 바운더리

**이 Context가 담당하는 것:**
- 주문 모델 및 라이프사이클
- 거래 실행 로직
- 포지션 관리
- 거래 기록
- 거래 체결 처리

**이 Context가 담당하지 않는 것:**
- 시세 데이터 (Market Data Context)
- 계좌 관리 (Account Context)
- 거래 전략 (Strategy Context)
- 리스크 검증 (Risk Management Context)
- 모니터링 (Monitoring Context)

---

## 2. Domain Model 상세 설계

### 2.1 Aggregates

#### 2.1.1 Order Aggregate (주문)

**Aggregate Root**

```csharp
/// <summary>
/// 주문 Aggregate Root
/// 주문의 전체 라이프사이클을 관리
/// </summary>
public class Order : AggregateRoot<OrderId>
{
    private readonly List<OrderExecution> _executions = new();
    private readonly List<OrderModification> _modifications = new();
    
    // ======================== Identity ========================
    /// <summary>주문 고유 ID</summary>
    public OrderId Id { get; private set; }
    
    /// <summary>브로커 주문 ID (키움)</summary>
    public string BrokerOrderId { get; private set; }
    
    // ======================== Order Information ========================
    /// <summary>계좌번호</summary>
    public AccountNumber AccountNumber { get; private set; }
    
    /// <summary>종목 코드</summary>
    public StockCode StockCode { get; private set; }
    
    /// <summary>종목명</summary>
    public string StockName { get; private set; }
    
    /// <summary>주문 유형 (시장가/지정가/조건부)</summary>
    public OrderType OrderType { get; private set; }
    
    /// <summary>주문 방향 (매수/매도)</summary>
    public OrderSide Side { get; private set; }
    
    /// <summary>주문 상태</summary>
    public OrderStatus Status { get; private set; }
    
    /// <summary>주문 수량</summary>
    public OrderQuantity Quantity { get; private set; }
    
    /// <summary>남은 미체결 수량</summary>
    public OrderQuantity RemainingQuantity { get; private set; }
    
    /// <summary>체결된 수량</summary>
    public OrderQuantity ExecutedQuantity { get; private set; }
    
    /// <summary>주문 가격</summary>
    public OrderPrice Price { get; private set; }
    
    /// <summary>평균 체결가</summary>
    public decimal AverageExecutionPrice { get; private set; }
    
    // ======================== Order Conditions ========================
    /// <summary>주문 조건 (조건부 주문의 경우)</summary>
    public OrderCondition Condition { get; private set; }
    
    /// <summary>손절 주문 (OCO: One-Cancels-Other)</summary>
    public OrderId StopLossOrderId { get; private set; }
    
    /// <summary>익절 주문</summary>
    public OrderId TakeProfitOrderId { get; private set; }
    
    /// <summary>유효 기간 (GTC: Good-Till-Canceled, GTD: Good-Till-Date 등)</summary>
    public OrderValidity Validity { get; private set; }
    
    // ======================== Timing ========================
    /// <summary>주문 생성 시간</summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>주문 제출 시간</summary>
    public DateTime SubmittedAt { get; private set; }
    
    /// <summary>첫 체결 시간</summary>
    public DateTime FirstExecutedAt { get; private set; }
    
    /// <summary>주문 완료 시간</summary>
    public DateTime CompletedAt { get; private set; }
    
    /// <summary>주문 취소 시간</summary>
    public DateTime CancelledAt { get; private set; }
    
    /// <summary>마지막 업데이트 시간</summary>
    public DateTime LastUpdatedAt { get; private set; }
    
    // ======================== Strategy/Tagging ========================
    /// <summary>전략 ID (어떤 전략으로부터 생성된 주문)</summary>
    public StrategyId StrategyId { get; private set; }
    
    /// <summary>주문 태그 (사용자 정의)</summary>
    public string Tag { get; private set; }
    
    /// <summary>거래 ID (포지션 ID)</summary>
    public TradeId TradeId { get; private set; }
    
    // ======================== Execution Details ========================
    /// <summary>모든 체결 내역</summary>
    public IReadOnlyList<OrderExecution> Executions => _executions.AsReadOnly();
    
    /// <summary>모든 수정 내역</summary>
    public IReadOnlyList<OrderModification> Modifications => _modifications.AsReadOnly();
    
    /// <summary>총 수수료</summary>
    public Money TotalCommission { get; private set; }
    
    /// <summary>총 세금</summary>
    public Money TotalTax { get; private set; }
    
    // ======================== Error Information ========================
    /// <summary>거부 사유</summary>
    public string RejectionReason { get; private set; }
    
    /// <summary>취소 사유</summary>
    public string CancellationReason { get; private set; }
    
    // ======================== Constructor & Factory Methods ========================
    
    /// <summary>
    /// 시장가 주문 생성
    /// </summary>
    public static Order CreateMarketOrder(
        AccountNumber accountNumber,
        StockCode stockCode,
        string stockName,
        OrderSide side,
        OrderQuantity quantity)
    {
        var order = new Order
        {
            Id = OrderId.New(),
            AccountNumber = accountNumber,
            StockCode = stockCode,
            StockName = stockName,
            OrderType = OrderType.Market,
            Side = side,
            Quantity = quantity,
            RemainingQuantity = quantity,
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            TotalCommission = Money.Zero,
            TotalTax = Money.Zero,
            AverageExecutionPrice = 0m
        };
        
        return order;
    }
    
    /// <summary>
    /// 지정가 주문 생성
    /// </summary>
    public static Order CreateLimitOrder(
        AccountNumber accountNumber,
        StockCode stockCode,
        string stockName,
        OrderSide side,
        OrderQuantity quantity,
        decimal limitPrice)
    {
        var order = new Order
        {
            Id = OrderId.New(),
            AccountNumber = accountNumber,
            StockCode = stockCode,
            StockName = stockName,
            OrderType = OrderType.Limit,
            Side = side,
            Quantity = quantity,
            RemainingQuantity = quantity,
            Price = OrderPrice.LimitPrice(limitPrice),
            Status = OrderStatus.Created,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            TotalCommission = Money.Zero,
            TotalTax = Money.Zero,
            AverageExecutionPrice = 0m
        };
        
        return order;
    }
    
    /// <summary>
    /// 조건부 주문 생성
    /// </summary>
    public static Order CreateConditionalOrder(
        AccountNumber accountNumber,
        StockCode stockCode,
        string stockName,
        OrderSide side,
        OrderQuantity quantity,
        decimal triggerPrice,
        decimal executionPrice,
        ConditionType conditionType)
    {
        var order = new Order
        {
            Id = OrderId.New(),
            AccountNumber = accountNumber,
            StockCode = stockCode,
            StockName = stockName,
            OrderType = OrderType.Conditional,
            Side = side,
            Quantity = quantity,
            RemainingQuantity = quantity,
            Price = OrderPrice.LimitPrice(executionPrice),
            Condition = new OrderCondition(conditionType, triggerPrice),
            Status = OrderStatus.Waiting,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            TotalCommission = Money.Zero,
            TotalTax = Money.Zero,
            AverageExecutionPrice = 0m
        };
        
        return order;
    }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 주문을 브로커에 제출
    /// </summary>
    public void Submit(string brokerOrderId)
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException(
                $"Cannot submit order in {Status} status");
        
        BrokerOrderId = brokerOrderId;
        Status = OrderStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new OrderSubmittedEvent(Id, brokerOrderId, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 주문이 인정됨 (서버 확인)
    /// </summary>
    public void Acknowledge()
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot acknowledge order in {Status} status");
        
        Status = OrderStatus.Acknowledged;
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new OrderAcknowledgedEvent(Id, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 주문 체결 처리
    /// </summary>
    public void Execute(
        OrderQuantity executedQuantity,
        decimal executionPrice,
        Money commission,
        Money tax,
        DateTime executionTime)
    {
        // 상태 검증
        if (Status != OrderStatus.Acknowledged && Status != OrderStatus.PartiallyFilled)
            throw new InvalidOperationException(
                $"Cannot execute order in {Status} status");
        
        // 수량 검증
        if (executedQuantity > RemainingQuantity)
            throw new InvalidOperationException(
                $"Execution quantity {executedQuantity} exceeds remaining {RemainingQuantity}");
        
        // 체결 기록 생성
        var execution = new OrderExecution(
            OrderExecutionId.New(),
            executedQuantity,
            executionPrice,
            commission,
            tax,
            executionTime);
        
        _executions.Add(execution);
        
        // 수량 업데이트
        ExecutedQuantity += executedQuantity;
        RemainingQuantity -= executedQuantity;
        
        // 평균 체결가 계산
        var totalExecutionAmount = _executions.Sum(e => e.ExecutedQuantity.Value * e.ExecutionPrice);
        AverageExecutionPrice = totalExecutionAmount / ExecutedQuantity.Value;
        
        // 수수료/세금 누적
        TotalCommission += commission;
        TotalTax += tax;
        
        // 상태 업데이트
        if (RemainingQuantity.Value == 0)
        {
            Status = OrderStatus.Filled;
            CompletedAt = DateTime.UtcNow;
        }
        else
        {
            Status = OrderStatus.PartiallyFilled;
        }
        
        if (FirstExecutedAt == DateTime.MinValue)
            FirstExecutedAt = executionTime;
        
        LastUpdatedAt = DateTime.UtcNow;
        
        // 이벤트 발행
        AddDomainEvent(new OrderExecutedEvent(
            Id,
            executedQuantity,
            executionPrice,
            commission,
            tax,
            executionTime));
    }
    
    /// <summary>
    /// 주문 수정 (가격/수량 변경)
    /// </summary>
    public void Modify(decimal newPrice, OrderQuantity newQuantity)
    {
        // 수정 가능 상태 확인
        if (Status != OrderStatus.Acknowledged && Status != OrderStatus.PartiallyFilled)
            throw new InvalidOperationException(
                $"Cannot modify order in {Status} status");
        
        // 최소 주문 수량 확인
        if (newQuantity < ExecutedQuantity)
            throw new InvalidOperationException(
                "New quantity cannot be less than executed quantity");
        
        var modification = new OrderModification(
            OrderModificationId.New(),
            OrderPrice.LimitPrice(newPrice),
            newQuantity,
            DateTime.UtcNow);
        
        _modifications.Add(modification);
        
        // 가격 업데이트
        Price = OrderPrice.LimitPrice(newPrice);
        
        // 수량 업데이트
        RemainingQuantity = newQuantity - ExecutedQuantity;
        
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new OrderModifiedEvent(
            Id,
            newPrice,
            newQuantity,
            DateTime.UtcNow));
    }
    
    /// <summary>
    /// 주문 취소
    /// </summary>
    public void Cancel(string reason = null)
    {
        // 취소 가능 상태 확인
        if (Status == OrderStatus.Filled || 
            Status == OrderStatus.Cancelled || 
            Status == OrderStatus.Rejected)
        {
            throw new InvalidOperationException(
                $"Cannot cancel order in {Status} status");
        }
        
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new OrderCancelledEvent(
            Id,
            reason,
            DateTime.UtcNow));
    }
    
    /// <summary>
    /// 주문 거부
    /// </summary>
    public void Reject(string reason)
    {
        if (Status != OrderStatus.Created && Status != OrderStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot reject order in {Status} status");
        
        Status = OrderStatus.Rejected;
        RejectionReason = reason;
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new OrderRejectedEvent(
            Id,
            reason,
            DateTime.UtcNow));
    }
    
    /// <summary>
    /// 손절/익절 주문 설정 (OCO)
    /// </summary>
    public void SetOCOOrders(OrderId stopLossOrderId, OrderId takeProfitOrderId)
    {
        if (Status != OrderStatus.Filled)
            throw new InvalidOperationException(
                "OCO orders can only be set for filled positions");
        
        StopLossOrderId = stopLossOrderId;
        TakeProfitOrderId = takeProfitOrderId;
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new OCOOrdersSetEvent(
            Id,
            stopLossOrderId,
            takeProfitOrderId,
            DateTime.UtcNow));
    }
    
    /// <summary>
    /// 조건 충족 확인 (조건부 주문)
    /// </summary>
    public bool IsConditionMet(decimal currentPrice)
    {
        if (OrderType != OrderType.Conditional || Condition == null)
            return false;
        
        return Condition.IsMet(currentPrice);
    }
    
    /// <summary>
    /// 주문이 유효한지 확인
    /// </summary>
    public bool IsValid()
    {
        // 만료 시간 확인 (GTD의 경우)
        if (Validity?.ExpiryDate < DateTime.UtcNow)
            return false;
        
        return Status != OrderStatus.Cancelled && Status != OrderStatus.Rejected;
    }
}
```

### 2.2 Value Objects

```csharp
/// <summary>주문 ID</summary>
public sealed class OrderId : StronglyTypedId<Guid>
{
    public OrderId(Guid value) : base(value) { }
    public static OrderId New() => new(Guid.NewGuid());
}

/// <summary>거래 ID (포지션)</summary>
public sealed class TradeId : StronglyTypedId<Guid>
{
    public TradeId(Guid value) : base(value) { }
    public static TradeId New() => new(Guid.NewGuid());
}

/// <summary>주문 수량</summary>
public sealed class OrderQuantity : ValueObject
{
    public int Value { get; }
    
    public OrderQuantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Order quantity must be positive");
        
        Value = value;
    }
    
    public static OrderQuantity operator +(OrderQuantity left, OrderQuantity right)
        => new(left.Value + right.Value);
    
    public static OrderQuantity operator -(OrderQuantity left, OrderQuantity right)
        => new(left.Value - right.Value);
    
    public static bool operator >(OrderQuantity left, OrderQuantity right)
        => left.Value > right.Value;
    
    public static bool operator <(OrderQuantity left, OrderQuantity right)
        => left.Value < right.Value;
    
    public static bool operator >=(OrderQuantity left, OrderQuantity right)
        => left.Value >= right.Value;
    
    public static bool operator <=(OrderQuantity left, OrderQuantity right)
        => left.Value <= right.Value;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public override string ToString() => Value.ToString();
}

/// <summary>주문 가격</summary>
public sealed class OrderPrice : ValueObject
{
    public decimal Value { get; }
    public PriceType Type { get; }
    
    private OrderPrice(decimal value, PriceType type)
    {
        Value = value;
        Type = type;
    }
    
    public static OrderPrice LimitPrice(decimal price)
    {
        if (price <= 0)
            throw new ArgumentException("Price must be positive");
        
        return new OrderPrice(price, PriceType.Limit);
    }
    
    public static OrderPrice MarketPrice()
        => new(0, PriceType.Market);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }
}

/// <summary>주문 조건 (조건부 주문)</summary>
public sealed class OrderCondition : ValueObject
{
    public ConditionType Type { get; }
    public decimal TriggerPrice { get; }
    
    public OrderCondition(ConditionType type, decimal triggerPrice)
    {
        Type = type;
        TriggerPrice = triggerPrice;
    }
    
    public bool IsMet(decimal currentPrice)
    {
        return Type switch
        {
            ConditionType.AbovePrice => currentPrice >= TriggerPrice,
            ConditionType.BelowPrice => currentPrice <= TriggerPrice,
            ConditionType.BuyAbove => currentPrice >= TriggerPrice,
            ConditionType.SellBelow => currentPrice <= TriggerPrice,
            _ => false
        };
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return TriggerPrice;
    }
}

/// <summary>주문 유효성 (GTC, GTD, IOC 등)</summary>
public sealed class OrderValidity : ValueObject
{
    public ValidityType Type { get; }
    public DateTime? ExpiryDate { get; }
    
    public OrderValidity(ValidityType type, DateTime? expiryDate = null)
    {
        Type = type;
        ExpiryDate = expiryDate;
    }
    
    public static OrderValidity GTC() => new(ValidityType.GTC);
    public static OrderValidity GTD(DateTime expiryDate) => new(ValidityType.GTD, expiryDate);
    public static OrderValidity IOC() => new(ValidityType.IOC);
    public static OrderValidity FOK() => new(ValidityType.FOK);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return ExpiryDate;
    }
}

/// <summary>주문 체결 내역</summary>
public sealed class OrderExecution : ValueObject
{
    public OrderExecutionId Id { get; }
    public OrderQuantity ExecutedQuantity { get; }
    public decimal ExecutionPrice { get; }
    public Money Commission { get; }
    public Money Tax { get; }
    public DateTime ExecutionTime { get; }
    
    public OrderExecution(
        OrderExecutionId id,
        OrderQuantity executedQuantity,
        decimal executionPrice,
        Money commission,
        Money tax,
        DateTime executionTime)
    {
        Id = id;
        ExecutedQuantity = executedQuantity;
        ExecutionPrice = executionPrice;
        Commission = commission;
        Tax = tax;
        ExecutionTime = executionTime;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return ExecutedQuantity;
        yield return ExecutionPrice;
        yield return ExecutionTime;
    }
}

/// <summary>주문 수정 내역</summary>
public sealed class OrderModification : ValueObject
{
    public OrderModificationId Id { get; }
    public OrderPrice NewPrice { get; }
    public OrderQuantity NewQuantity { get; }
    public DateTime ModifiedAt { get; }
    
    public OrderModification(
        OrderModificationId id,
        OrderPrice newPrice,
        OrderQuantity newQuantity,
        DateTime modifiedAt)
    {
        Id = id;
        NewPrice = newPrice;
        NewQuantity = newQuantity;
        ModifiedAt = modifiedAt;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return ModifiedAt;
    }
}
```

#### 2.2.1 Strong IDs

```csharp
public sealed class OrderExecutionId : StronglyTypedId<Guid>
{
    public OrderExecutionId(Guid value) : base(value) { }
    public static OrderExecutionId New() => new(Guid.NewGuid());
}

public sealed class OrderModificationId : StronglyTypedId<Guid>
{
    public OrderModificationId(Guid value) : base(value) { }
    public static OrderModificationId New() => new(Guid.NewGuid());
}
```

### 2.3 Position Aggregate (포지션)

```csharp
/// <summary>
/// 포지션 Aggregate Root
/// 종목별 보유 포지션을 관리
/// </summary>
public class Position : AggregateRoot<PositionId>
{
    private readonly List<Trade> _trades = new();
    
    // ======================== Identity ========================
    public PositionId Id { get; private set; }
    public AccountNumber AccountNumber { get; private set; }
    public StockCode StockCode { get; private set; }
    
    // ======================== Position Data ========================
    /// <summary>보유 수량</summary>
    public int Quantity { get; private set; }
    
    /// <summary>평균 매입가</summary>
    public decimal AverageCostPrice { get; private set; }
    
    /// <summary>현재가</summary>
    public decimal CurrentPrice { get; private set; }
    
    /// <summary>포지션 평가 금액</summary>
    public Money PositionValue { get; private set; }
    
    /// <summary>미실현 손익</summary>
    public Money UnrealizedPL { get; private set; }
    
    /// <summary>수익률</summary>
    public decimal ReturnRate { get; private set; }
    
    // ======================== Trade History ========================
    public IReadOnlyList<Trade> Trades => _trades.AsReadOnly();
    
    /// <summary>포지션 열린 시간</summary>
    public DateTime OpenedAt { get; private set; }
    
    /// <summary>포지션 닫힌 시간 (모두 청산된 경우)</summary>
    public DateTime? ClosedAt { get; private set; }
    
    // ======================== OCO Orders ========================
    public OrderId StopLossOrderId { get; private set; }
    public OrderId TakeProfitOrderId { get; private set; }
    
    // ======================== Methods ========================
    
    /// <summary>
    /// 매수로 포지션 추가
    /// </summary>
    public void AddTrade(Trade trade)
    {
        if (trade.PositionId != Id)
            throw new InvalidOperationException("Trade does not belong to this position");
        
        _trades.Add(trade);
        RecalculatePosition();
        
        AddDomainEvent(new TradeAddedEvent(Id, trade.Id, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 미실현 손익 계산
    /// </summary>
    public void UpdateCurrentPrice(decimal newPrice)
    {
        CurrentPrice = newPrice;
        PositionValue = new Money(Quantity * newPrice);
        UnrealizedPL = new Money(PositionValue.Amount - (Quantity * AverageCostPrice));
        ReturnRate = AverageCostPrice > 0 ? UnrealizedPL.Amount / (Quantity * AverageCostPrice) : 0;
        
        AddDomainEvent(new PositionRevaluatedEvent(Id, newPrice, UnrealizedPL, DateTime.UtcNow));
    }
    
    private void RecalculatePosition()
    {
        var totalCost = _trades.Where(t => t.Side == OrderSide.Buy).Sum(t => t.Quantity * t.ExecutionPrice);
        var totalSold = _trades.Where(t => t.Side == OrderSide.Sell).Sum(t => t.Quantity * t.ExecutionPrice);
        
        Quantity = _trades.Where(t => t.Side == OrderSide.Buy).Sum(t => t.Quantity) -
                   _trades.Where(t => t.Side == OrderSide.Sell).Sum(t => t.Quantity);
        
        if (Quantity > 0)
        {
            var buyQuantity = _trades.Where(t => t.Side == OrderSide.Buy).Sum(t => t.Quantity);
            AverageCostPrice = totalCost / buyQuantity;
        }
    }
}
```

### 2.4 Enumerations

```csharp
/// <summary>주문 타입</summary>
public enum OrderType
{
    Market = 1,         // 시장가
    Limit = 2,          // 지정가
    Conditional = 3,    // 조건부
    Stop = 4,           // 손절
    TrailingStop = 5    // 트레일링 손절
}

/// <summary>주문 방향</summary>
public enum OrderSide
{
    Buy = 1,
    Sell = 2,
    Short = 3
}

/// <summary>주문 상태</summary>
public enum OrderStatus
{
    Created = 1,        // 생성됨
    Submitted = 2,      // 제출됨
    Acknowledged = 3,   // 인정됨
    Waiting = 4,        // 대기중 (조건부)
    PartiallyFilled = 5,// 부분 체결
    Filled = 6,         // 전부 체결
    Cancelled = 7,      // 취소됨
    Rejected = 8,       // 거부됨
    Expired = 9         // 만료됨
}

/// <summary>가격 타입</summary>
public enum PriceType
{
    Market = 1,
    Limit = 2
}

/// <summary>조건 타입</summary>
public enum ConditionType
{
    AbovePrice = 1,
    BelowPrice = 2,
    BuyAbove = 3,
    SellBelow = 4
}

/// <summary>유효성 타입</summary>
public enum ValidityType
{
    GTC = 1,    // Good-Till-Canceled
    GTD = 2,    // Good-Till-Date
    IOC = 3,    // Immediate-Or-Cancel
    FOK = 4     // Fill-Or-Kill
}
```

---

## 3. Business Rules & Invariants

### 3.1 핵심 불변식

```csharp
/// <summary>
/// 주문의 핵심 비즈니스 규칙을 검증하는 클래스
/// </summary>
public static class OrderInvariants
{
    /// <summary>
    /// Invariant 1: 체결된 수량 + 남은 수량 = 원래 수량
    /// </summary>
    public static void ValidateOrderQuantity(Order order)
    {
        var sum = order.ExecutedQuantity.Value + order.RemainingQuantity.Value;
        if (sum != order.Quantity.Value)
        {
            throw new InvalidOperationException(
                $"Order quantity mismatch. Executed: {order.ExecutedQuantity}, " +
                $"Remaining: {order.RemainingQuantity}, Total: {order.Quantity}");
        }
    }
    
    /// <summary>
    /// Invariant 2: 체결 수량은 음수가 될 수 없음
    /// </summary>
    public static void ValidateExecutedQuantity(Order order)
    {
        if (order.ExecutedQuantity.Value < 0)
            throw new InvalidOperationException("Executed quantity cannot be negative");
    }
    
    /// <summary>
    /// Invariant 3: 남은 수량은 음수가 될 수 없음
    /// </summary>
    public static void ValidateRemainingQuantity(Order order)
    {
        if (order.RemainingQuantity.Value < 0)
            throw new InvalidOperationException("Remaining quantity cannot be negative");
    }
    
    /// <summary>
    /// Invariant 4: 평균 체결가는 0보다 크거나 같아야 함
    /// </summary>
    public static void ValidateAverageExecutionPrice(Order order)
    {
        if (order.ExecutedQuantity.Value > 0 && order.AverageExecutionPrice < 0)
            throw new InvalidOperationException("Average execution price cannot be negative");
    }
    
    /// <summary>
    /// Invariant 5: 수수료와 세금은 양수여야 함
    /// </summary>
    public static void ValidateFeesAndTaxes(Order order)
    {
        if (order.TotalCommission.Amount < 0 || order.TotalTax.Amount < 0)
            throw new InvalidOperationException("Commission and tax cannot be negative");
    }
}

/// <summary>
/// 포지션의 핵심 비즈니스 규칙
/// </summary>
public static class PositionInvariants
{
    /// <summary>
    /// Invariant 1: 포지션 수량 >= 0
    /// </summary>
    public static void ValidateQuantity(Position position)
    {
        if (position.Quantity < 0)
            throw new InvalidOperationException("Position quantity cannot be negative");
    }
    
    /// <summary>
    /// Invariant 2: 닫힌 포지션의 수량은 0
    /// </summary>
    public static void ValidateClosedPosition(Position position)
    {
        if (position.ClosedAt.HasValue && position.Quantity != 0)
            throw new InvalidOperationException("Closed position must have zero quantity");
    }
    
    /// <summary>
    /// Invariant 3: 포지션 값 = 수량 * 현재가
    /// </summary>
    public static void ValidatePositionValue(Position position)
    {
        var expected = position.Quantity * position.CurrentPrice;
        if (Math.Abs(position.PositionValue.Amount - expected) > 1)
            throw new InvalidOperationException("Position value mismatch");
    }
}
```

### 3.2 비즈니스 규칙

```csharp
/// <summary>
/// 거래 관련 비즈니스 규칙
/// </summary>
public static class TradingBusinessRules
{
    /// <summary>
    /// 규칙 1: 최소 주문 수량 확인
    /// </summary>
    public static bool IsMinimumQuantityValid(
        StockCode stockCode,
        OrderQuantity quantity,
        IMinimumQuantityProvider provider)
    {
        var minQty = provider.GetMinimumQuantity(stockCode);
        return quantity.Value >= minQty;
    }
    
    /// <summary>
    /// 규칙 2: 주문 수량 제한 (최대 한도)
    /// </summary>
    public static bool IsMaximumQuantityValid(
        StockCode stockCode,
        OrderQuantity quantity,
        IMaximumQuantityProvider provider)
    {
        var maxQty = provider.GetMaximumQuantity(stockCode);
        return quantity.Value <= maxQty;
    }
    
    /// <summary>
    /// 규칙 3: 호가 변동성 확인
    /// 가격이 과도하게 변동하지 않는지 확인 (오류 방지)
    /// </summary>
    public static bool IsPriceSane(
        decimal newPrice,
        decimal previousPrice,
        decimal maxChangePercent = 30)
    {
        if (previousPrice <= 0) return true;
        
        var changePercent = Math.Abs((newPrice - previousPrice) / previousPrice) * 100;
        return changePercent <= maxChangePercent;
    }
    
    /// <summary>
    /// 규칙 4: 거래 시간 확인
    /// </summary>
    public static bool IsTradingHour(DateTime dateTime, ITradingHourProvider provider)
    {
        return provider.IsTradingHour(dateTime);
    }
    
    /// <summary>
    /// 규칙 5: 거래 정지 종목 확인
    /// </summary>
    public static bool IsStockTradeable(
        StockCode stockCode,
        IStockStatusProvider provider)
    {
        var status = provider.GetStatus(stockCode);
        return status == StockStatus.Normal;
    }
}
```

---

## 4. Domain Events

```csharp
/// <summary>주문 제출 이벤트</summary>
public sealed class OrderSubmittedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public string BrokerOrderId { get; }
    public DateTime SubmittedAt { get; }
    
    public OrderSubmittedEvent(OrderId orderId, string brokerOrderId, DateTime submittedAt)
    {
        OrderId = orderId;
        BrokerOrderId = brokerOrderId;
        SubmittedAt = submittedAt;
    }
}

/// <summary>주문 인정 이벤트</summary>
public sealed class OrderAcknowledgedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public DateTime AcknowledgedAt { get; }
    
    public OrderAcknowledgedEvent(OrderId orderId, DateTime acknowledgedAt)
    {
        OrderId = orderId;
        AcknowledgedAt = acknowledgedAt;
    }
}

/// <summary>주문 체결 이벤트</summary>
public sealed class OrderExecutedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public OrderQuantity ExecutedQuantity { get; }
    public decimal ExecutionPrice { get; }
    public Money Commission { get; }
    public Money Tax { get; }
    public DateTime ExecutionTime { get; }
    
    public OrderExecutedEvent(
        OrderId orderId,
        OrderQuantity executedQuantity,
        decimal executionPrice,
        Money commission,
        Money tax,
        DateTime executionTime)
    {
        OrderId = orderId;
        ExecutedQuantity = executedQuantity;
        ExecutionPrice = executionPrice;
        Commission = commission;
        Tax = tax;
        ExecutionTime = executionTime;
    }
}

/// <summary>주문 취소 이벤트</summary>
public sealed class OrderCancelledEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public string Reason { get; }
    public DateTime CancelledAt { get; }
    
    public OrderCancelledEvent(OrderId orderId, string reason, DateTime cancelledAt)
    {
        OrderId = orderId;
        Reason = reason;
        CancelledAt = cancelledAt;
    }
}

/// <summary>주문 거부 이벤트</summary>
public sealed class OrderRejectedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public string Reason { get; }
    public DateTime RejectedAt { get; }
    
    public OrderRejectedEvent(OrderId orderId, string reason, DateTime rejectedAt)
    {
        OrderId = orderId;
        Reason = reason;
        RejectedAt = rejectedAt;
    }
}

/// <summary>거래 추가 이벤트</summary>
public sealed class TradeAddedEvent : DomainEvent
{
    public PositionId PositionId { get; }
    public TradeId TradeId { get; }
    public DateTime AddedAt { get; }
    
    public TradeAddedEvent(PositionId positionId, TradeId tradeId, DateTime addedAt)
    {
        PositionId = positionId;
        TradeId = tradeId;
        AddedAt = addedAt;
    }
}

/// <summary>포지션 재평가 이벤트</summary>
public sealed class PositionRevaluatedEvent : DomainEvent
{
    public PositionId PositionId { get; }
    public decimal NewPrice { get; }
    public Money UnrealizedPL { get; }
    public DateTime RevaluatedAt { get; }
    
    public PositionRevaluatedEvent(
        PositionId positionId,
        decimal newPrice,
        Money unrealizedPL,
        DateTime revaluatedAt)
    {
        PositionId = positionId;
        NewPrice = newPrice;
        UnrealizedPL = unrealizedPL;
        RevaluatedAt = revaluatedAt;
    }
}

/// <summary>OCO 주문 설정 이벤트</summary>
public sealed class OCOOrdersSetEvent : DomainEvent
{
    public OrderId PositionOrderId { get; }
    public OrderId StopLossOrderId { get; }
    public OrderId TakeProfitOrderId { get; }
    public DateTime SetAt { get; }
    
    public OCOOrdersSetEvent(
        OrderId positionOrderId,
        OrderId stopLossOrderId,
        OrderId takeProfitOrderId,
        DateTime setAt)
    {
        PositionOrderId = positionOrderId;
        StopLossOrderId = stopLossOrderId;
        TakeProfitOrderId = takeProfitOrderId;
        SetAt = setAt;
    }
}
```

---

## 5. Domain Services

```csharp
/// <summary>주문 검증 도메인 서비스</summary>
public interface IOrderValidationService : IDomainService
{
    Task<Result> ValidateOrderAsync(Order order);
    bool ValidateQuantity(StockCode stockCode, OrderQuantity quantity);
    bool ValidatePrice(decimal price, StockCode stockCode, decimal previousClose);
    bool IsTradingHour(DateTime dateTime);
}

/// <summary>주문 실행 도메인 서비스</summary>
public interface IOrderExecutionService : IDomainService
{
    Task<Result<string>> SubmitOrderAsync(Order order);
    Task<Result> CancelOrderAsync(Order order);
    Task<Result> ModifyOrderAsync(Order order, decimal newPrice, int newQuantity);
}

/// <summary>거래 계산 서비스</summary>
public interface ITradeCalculationService : IDomainService
{
    Money CalculateCommission(decimal executionValue, SecurityType securityType);
    Money CalculateTax(decimal gain, int holdingDays, TaxProfile taxProfile);
    decimal CalculateAverageExecutionPrice(
        List<OrderExecution> executions,
        OrderQuantity totalQuantity);
}
```

---

## 6. Application Services

```csharp
/// <summary>주문 애플리케이션 서비스</summary>
public sealed class OrderService : IApplicationService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderValidationService _validationService;
    private readonly IOrderExecutionService _executionService;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;
    
    /// <summary>시장가 주문 생성</summary>
    public async Task<Result<OrderDto>> CreateMarketOrderAsync(
        CreateMarketOrderCommand command)
    {
        try
        {
            var order = Order.CreateMarketOrder(
                new AccountNumber(command.AccountNumber),
                new StockCode(command.StockCode),
                command.StockName,
                command.Side,
                new OrderQuantity(command.Quantity));
            
            var validationResult = await _validationService.ValidateOrderAsync(order);
            if (!validationResult.IsSuccess)
                return Result<OrderDto>.Failure(validationResult.Error);
            
            await _orderRepository.AddAsync(order);
            
            var submissionResult = await _executionService.SubmitOrderAsync(order);
            if (submissionResult.IsSuccess)
            {
                order.Submit(submissionResult.Value);
                await _orderRepository.UpdateAsync(order);
            }
            else
            {
                order.Reject(submissionResult.Error);
                await _orderRepository.UpdateAsync(order);
                return Result<OrderDto>.Failure(submissionResult.Error);
            }
            
            await _unitOfWork.CommitAsync();
            
            foreach (var @event in order.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return Result<OrderDto>.Success(MapToDto(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create market order");
            return Result<OrderDto>.Failure($"Market order creation failed: {ex.Message}");
        }
    }
    
    /// <summary>주문 취소</summary>
    public async Task<Result> CancelOrderAsync(CancelOrderCommand command)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(new OrderId(command.OrderId));
            if (order == null)
                return Result.Failure("Order not found");
            
            var result = await _executionService.CancelOrderAsync(order);
            if (!result.IsSuccess)
                return result;
            
            order.Cancel(command.Reason);
            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.CommitAsync();
            
            foreach (var @event in order.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel order");
            return Result.Failure($"Order cancellation failed: {ex.Message}");
        }
    }
    
    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.Id.Value,
            BrokerOrderId = order.BrokerOrderId,
            StockCode = order.StockCode.Value,
            Status = order.Status.ToString(),
            Quantity = order.Quantity.Value,
            ExecutedQuantity = order.ExecutedQuantity.Value,
            AverageExecutionPrice = order.AverageExecutionPrice,
            CreatedAt = order.CreatedAt
        };
    }
}
```

---

## 7. Infrastructure Layer

```csharp
/// <summary>주문 저장소 구현</summary>
public sealed class OrderRepository : IOrderRepository
{
    private readonly TradingDbContext _context;
    private readonly IDistributedCache _cache;
    
    public async Task<Order> GetByIdAsync(OrderId orderId)
    {
        var cacheKey = $"order:{orderId}";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        if (cachedData != null)
            return JsonSerializer.Deserialize<Order>(cachedData);
        
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);
        
        if (order != null)
        {
            var json = JsonSerializer.Serialize(order);
            await _cache.SetStringAsync(cacheKey, json, TimeSpan.FromMinutes(5));
        }
        
        return order;
    }
    
    public async Task AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        
        var cacheKey = $"order:{order.Id}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

---

## 8. API/Interface Specifications

```csharp
/// <summary>거래 API</summary>
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    /// <summary>POST /api/orders/market - 시장가 주문 생성</summary>
    [HttpPost("market")]
    public async Task<ActionResult<OrderDto>> CreateMarketOrderAsync(
        [FromBody] CreateMarketOrderRequest request)
    {
        var command = new CreateMarketOrderCommand(
            request.AccountNumber,
            request.StockCode,
            request.StockName,
            request.Side,
            request.Quantity);
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Created($"/api/orders/{result.Value.OrderId}", result.Value);
    }
    
    /// <summary>DELETE /api/orders/{orderId} - 주문 취소</summary>
    [HttpDelete("{orderId}")]
    public async Task<IActionResult> CancelOrderAsync(
        Guid orderId,
        [FromBody] CancelOrderRequest request)
    {
        var command = new CancelOrderCommand(orderId, request.Reason);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return NoContent();
    }
    
    /// <summary>GET /api/orders/{orderId} - 주문 조회</summary>
    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDto>> GetOrderAsync(Guid orderId)
    {
        var query = new GetOrderQuery(orderId);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }
}
```

---

## 9. Order Execution Flow

### 주문 라이프사이클

```
Created → Submitted → Acknowledged → PartiallyFilled → Filled
   ↓         ↓            ↓
 Rejected  Cancelled    Cancelled
```

---

## 10. Error Handling & Validation

```csharp
public sealed class OrderNotFoundException : DomainException
{
    public OrderId OrderId { get; }
    
    public OrderNotFoundException(string message, OrderId orderId = null)
        : base(message) { OrderId = orderId; }
}

public sealed class InvalidOrderStatusException : DomainException
{
    public OrderStatus CurrentStatus { get; }
    
    public InvalidOrderStatusException(
        string message,
        OrderStatus currentStatus)
        : base(message) { CurrentStatus = currentStatus; }
}
```

---

## 11. External System Integration

```csharp
public interface IKiwoomApi
{
    Task<KiwoomOrderResponse> SendOrderAsync(
        string accountNumber,
        KiwoomOrderRequest request);
    
    Task<KiwoomOrderResponse> CancelOrderAsync(
        string accountNumber,
        string brokerOrderId);
}
```

---

## 12. Performance Considerations

### 캐싱 전략
- L1: Redis (5분 TTL) - 빠른 조회
- L2: Database - 원본 데이터

### 배치 처리
- 여러 주문 실행 이벤트를 배치로 처리
- 만료된 주문 정기적 정리

---

## 결론

이 Trading Context 설계는 다음 특징을 갖습니다:

✅ **완전성**: 주문 관리의 모든 측면
✅ **안전성**: 모든 거래 검증 및 기록
✅ **정확성**: 체결, 수수료, 세금 정확 계산
✅ **성능**: 캐싱과 배치 처리로 저지연 달성
✅ **확장성**: 다양한 주문 유형 지원
✅ **추적성**: 완전한 감사 기록 제공
