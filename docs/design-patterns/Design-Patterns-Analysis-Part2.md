# 디자인 패턴 분석 및 추천 가이드 (Part 2)

## 📋 목차
1. [2025년 최신 트렌드](#2025년-최신-트렌드)
2. [실전 적용 가이드](#실전-적용-가이드)
3. [패턴 조합 전략](#패턴-조합-전략)
4. [성능 최적화 팁](#성능-최적화-팁)

---

## 2025년 최신 트렌드

### 1️⃣ 의존성 주입(DI)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<IKiwoomApi, KiwoomApi>();

// Scrutor로 Decorator 체이닝
builder.Services.Decorate<IOrderService, CachedOrderService>();
builder.Services.Decorate<IOrderService, LoggingOrderService>();
```

---

### 2️⃣ MediatR + CQRS

```csharp
// Command
public record PlaceOrderCommand(Order Order) : IRequest<OrderResult>;

// Handler
public class PlaceOrderHandler 
    : IRequestHandler<PlaceOrderCommand, OrderResult>
{
    public async Task<OrderResult> Handle(
        PlaceOrderCommand request, 
        CancellationToken ct)
    {
        // 주문 처리 로직
        return new OrderResult { Success = true };
    }
}

// 사용
await _mediator.Send(new PlaceOrderCommand(order));
```

---

### 3️⃣ Reactive Extensions (Rx)

```csharp
using System.Reactive.Linq;

public IObservable<StockPrice> GetPriceStream(string code)
{
    return Observable
        .Interval(TimeSpan.FromSeconds(1))
        .SelectMany(_ => _api.GetPriceAsync(code))
        .DistinctUntilChanged()
        .Retry(3);
}

// 구독
var subscription = priceStream
    .Throttle(TimeSpan.FromMilliseconds(100))
    .Subscribe(price => UpdateUI(price));
```

---

### 4️⃣ Record 타입 + with 표현식

```csharp
public record Order(
    string StockCode, 
    int Quantity, 
    decimal Price)
{
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfit { get; init; }
}

// 불변 수정
var order = new Order("005930", 10, 70000)
    .WithStopLoss(68000)
    .WithTakeProfit(75000);

public static Order WithStopLoss(this Order o, decimal sl)
    => o with { StopLoss = sl };
```

---

### 5️⃣ Pattern Matching

```csharp
// Switch Expression
decimal CalculateFee(Order order) => order switch
{
    { Quantity: > 100, Price: > 100000 } => order.TotalAmount * 0.001m,
    { Quantity: > 50 } => order.TotalAmount * 0.002m,
    _ => order.TotalAmount * 0.003m
};

// Type Pattern
string GetOrderType(IOrder order) => order switch
{
    MarketOrder m => $"시장가: {m.Quantity}주",
    LimitOrder l => $"지정가: {l.Quantity}주 @ {l.Price:N0}",
    StopOrder s => $"손절: {s.StopPrice:N0}",
    _ => "알 수 없음"
};

// Relational Pattern
string GetRiskLevel(decimal amount) => amount switch
{
    < 1000000 => "낮음",
    >= 1000000 and < 5000000 => "보통",
    >= 5000000 => "높음"
};
```

---

## 실전 적용 가이드

### Phase 1: 기본 구조 (1주차)

#### 목표
프로젝트의 기본 뼈대 구성

#### 적용 패턴

```csharp
// ✅ 1. Repository Pattern
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task SaveAsync(Order order);
    Task<List<Order>> GetActiveOrdersAsync();
}

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    
    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Order> GetByIdAsync(Guid id)
    {
        return await _context.Orders.FindAsync(id);
    }
    
    public async Task SaveAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }
}

// ✅ 2. Service Pattern
public interface IOrderService
{
    Task<OrderResult> PlaceOrderAsync(Order order);
    Task<bool> CancelOrderAsync(Guid orderId);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(
        IOrderRepository repository,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<OrderResult> PlaceOrderAsync(Order order)
    {
        _logger.LogInformation("주문 처리 시작: {OrderId}", order.OrderId);
        
        await _repository.SaveAsync(order);
        
        return new OrderResult { Success = true };
    }
}

// ✅ 3. Dependency Injection
// Program.cs
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ✅ 4. Builder Pattern
var order = new OrderBuilder()
    .SetStock("005930")
    .SetQuantity(10)
    .SetPrice(70000)
    .Build();
```

#### 체크리스트

- [ ] DbContext 설정
- [ ] Repository 인터페이스 정의
- [ ] Service 레이어 구현
- [ ] DI 컨테이너 설정
- [ ] 기본 CRUD 테스트

---

### Phase 2: 핵심 기능 (2-3주차)

#### 목표
실시간 데이터 처리 및 전략 시스템 구축

#### 적용 패턴

```csharp
// ✅ 1. Strategy Pattern - 매매 전략
public interface ITradingStrategy
{
    string Name { get; }
    Task<Signal> GenerateSignalAsync(MarketData data);
}

public class MovingAverageStrategy : ITradingStrategy
{
    public string Name => "이동평균선";
    private readonly int _shortPeriod;
    private readonly int _longPeriod;
    
    public MovingAverageStrategy(int shortPeriod, int longPeriod)
    {
        _shortPeriod = shortPeriod;
        _longPeriod = longPeriod;
    }
    
    public async Task<Signal> GenerateSignalAsync(MarketData data)
    {
        var shortMA = data.Prices.TakeLast(_shortPeriod).Average();
        var longMA = data.Prices.TakeLast(_longPeriod).Average();
        
        if (shortMA > longMA)
            return new Signal { Type = SignalType.Buy };
        
        if (shortMA < longMA)
            return new Signal { Type = SignalType.Sell };
        
        return null;
    }
}

// DI 등록
builder.Services.AddKeyedTransient<ITradingStrategy, MovingAverageStrategy>("MA");
builder.Services.AddKeyedTransient<ITradingStrategy, RSIStrategy>("RSI");

// ✅ 2. Observer Pattern - 실시간 시세
public class StockPriceService
{
    public event EventHandler<PriceChangedEventArgs> PriceChanged;
    
    public void UpdatePrice(string stockCode, decimal price)
    {
        OnPriceChanged(new PriceChangedEventArgs 
        { 
            StockCode = stockCode, 
            Price = price 
        });
    }
    
    protected virtual void OnPriceChanged(PriceChangedEventArgs e)
    {
        PriceChanged?.Invoke(this, e);
    }
}

// 또는 Rx 사용
public IObservable<StockPrice> GetPriceStream(string stockCode)
{
    return Observable
        .Interval(TimeSpan.FromSeconds(1))
        .Select(_ => GetCurrentPrice(stockCode));
}

// ✅ 3. Adapter Pattern - API 통합
public interface IMarketDataProvider
{
    Task<StockPrice> GetPriceAsync(string stockCode);
}

public class KiwoomAdapter : IMarketDataProvider
{
    private readonly KiwoomOpenApi _api;
    
    public async Task<StockPrice> GetPriceAsync(string stockCode)
    {
        var rawData = _api.GetStockPrice(stockCode);
        return ParseKiwoomData(rawData);
    }
}

public class KoreaInvestmentAdapter : IMarketDataProvider
{
    private readonly KIRestApi _api;
    
    public async Task<StockPrice> GetPriceAsync(string stockCode)
    {
        var response = await _api.GetQuoteAsync(stockCode);
        return MapToStockPrice(response);
    }
}

// ✅ 4. Chain of Responsibility - 검증
public abstract class OrderValidator
{
    protected OrderValidator _next;
    
    public OrderValidator SetNext(OrderValidator next)
    {
        _next = next;
        return next;
    }
    
    public abstract Task<ValidationResult> ValidateAsync(Order order);
}

public class BasicValidator : OrderValidator
{
    public override async Task<ValidationResult> ValidateAsync(Order order)
    {
        if (order.Quantity <= 0)
            return ValidationResult.Fail("수량은 0보다 커야 합니다");
        
        return _next != null 
            ? await _next.ValidateAsync(order)
            : ValidationResult.Success();
    }
}

public class BalanceValidator : OrderValidator
{
    private readonly IAccountService _account;
    
    public override async Task<ValidationResult> ValidateAsync(Order order)
    {
        var balance = await _account.GetBalanceAsync();
        var required = order.Quantity * order.Price;
        
        if (balance < required)
            return ValidationResult.Fail("잔고 부족");
        
        return _next != null 
            ? await _next.ValidateAsync(order)
            : ValidationResult.Success();
    }
}
```

#### 체크리스트

- [ ] 최소 2개 이상 매매 전략 구현
- [ ] 실시간 시세 업데이트 연동
- [ ] API 어댑터 구현
- [ ] 주문 검증 파이프라인 구축
- [ ] 통합 테스트

---

### Phase 3: 고급 기능 (4주차+)

#### 목표
성능 최적화 및 운영 기능 추가

#### 적용 패턴

```csharp
// ✅ 1. Proxy Pattern - 캐싱 & Rate Limiting
public class CachedMarketDataProxy : IMarketDataProvider
{
    private readonly IMarketDataProvider _inner;
    private readonly IMemoryCache _cache;
    private readonly SemaphoreSlim _rateLimiter;
    
    public CachedMarketDataProxy(
        IMarketDataProvider inner,
        IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
        _rateLimiter = new SemaphoreSlim(5, 5); // 동시 5개 제한
    }
    
    public async Task<StockPrice> GetPriceAsync(string stockCode)
    {
        var key = $"price_{stockCode}";
        
        if (_cache.TryGetValue(key, out StockPrice cached))
            return cached;
        
        await _rateLimiter.WaitAsync();
        try
        {
            var price = await _inner.GetPriceAsync(stockCode);
            
            _cache.Set(key, price, TimeSpan.FromSeconds(5));
            
            return price;
        }
        finally
        {
            _rateLimiter.Release();
        }
    }
}

// DI 등록
builder.Services.AddScoped<IMarketDataProvider, KiwoomAdapter>();
builder.Services.Decorate<IMarketDataProvider, CachedMarketDataProxy>();

// ✅ 2. Command Pattern with MediatR
public record PlaceOrderCommand(Order Order) : IRequest<OrderResult>;
public record CancelOrderCommand(Guid OrderId) : IRequest<bool>;

public class PlaceOrderHandler 
    : IRequestHandler<PlaceOrderCommand, OrderResult>
{
    private readonly IOrderRepository _repository;
    private readonly IEventBus _eventBus;
    
    public async Task<OrderResult> Handle(
        PlaceOrderCommand request,
        CancellationToken ct)
    {
        await _repository.SaveAsync(request.Order);
        
        await _eventBus.PublishAsync(
            new OrderPlacedEvent(request.Order), ct);
        
        return new OrderResult { Success = true };
    }
}

// ✅ 3. State Pattern - 주문 상태
public class OrderStateMachine
{
    private readonly StateMachine<OrderState, OrderTrigger> _machine;
    
    public OrderStateMachine()
    {
        _machine = new StateMachine<OrderState, OrderTrigger>(
            OrderState.Pending);
        
        _machine.Configure(OrderState.Pending)
            .Permit(OrderTrigger.Submit, OrderState.Submitted)
            .Permit(OrderTrigger.Cancel, OrderState.Cancelled);
        
        _machine.Configure(OrderState.Submitted)
            .Permit(OrderTrigger.Execute, OrderState.Executed)
            .Permit(OrderTrigger.Cancel, OrderState.Cancelled);
    }
    
    public async Task TransitionAsync(OrderTrigger trigger)
    {
        await _machine.FireAsync(trigger);
    }
    
    public OrderState CurrentState => _machine.State;
}

// ✅ 4. Composite Pattern - 포트폴리오
public abstract class PortfolioComponent
{
    public string Name { get; set; }
    public abstract decimal GetTotalValue();
}

public class Position : PortfolioComponent
{
    public int Quantity { get; set; }
    public decimal CurrentPrice { get; set; }
    
    public override decimal GetTotalValue()
        => Quantity * CurrentPrice;
}

public class Portfolio : PortfolioComponent
{
    private List<PortfolioComponent> _components = new();
    
    public void Add(PortfolioComponent component)
        => _components.Add(component);
    
    public override decimal GetTotalValue()
        => _components.Sum(c => c.GetTotalValue());
}
```

#### 체크리스트

- [ ] API 캐싱 구현
- [ ] Rate Limiting 적용
- [ ] 주문 상태 머신 구현
- [ ] 포트폴리오 계층 구조 구축
- [ ] 성능 테스트 (부하 테스트)

---

## 패턴 조합 전략

### 조합 1: Repository + Unit of Work

```csharp
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    IPositionRepository Positions { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Orders = new OrderRepository(_context);
        Positions = new PositionRepository(_context);
    }
    
    public IOrderRepository Orders { get; }
    public IPositionRepository Positions { get; }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}

// 사용
public class TradingService
{
    private readonly IUnitOfWork _uow;
    
    public async Task ExecuteTradeAsync(Order order)
    {
        await _uow.Orders.SaveAsync(order);
        await _uow.Positions.UpdateAsync(position);
        
        await _uow.SaveChangesAsync(); // 트랜잭션
    }
}
```

---

### 조합 2: Strategy + Factory

```csharp
public interface IStrategyFactory
{
    ITradingStrategy Create(string strategyName);
}

public class StrategyFactory : IStrategyFactory
{
    private readonly IServiceProvider _provider;
    
    public StrategyFactory(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public ITradingStrategy Create(string strategyName)
    {
        return strategyName switch
        {
            "MA" => _provider.GetRequiredKeyedService<ITradingStrategy>("MA"),
            "RSI" => _provider.GetRequiredKeyedService<ITradingStrategy>("RSI"),
            "MACD" => _provider.GetRequiredKeyedService<ITradingStrategy>("MACD"),
            _ => throw new ArgumentException($"Unknown strategy: {strategyName}")
        };
    }
}
```

---

### 조합 3: Observer + Mediator

```csharp
// Domain Event
public record OrderPlacedEvent(Order Order) : INotification;

// Event Handler
public class OrderPlacedHandler : INotificationHandler<OrderPlacedEvent>
{
    private readonly IPositionService _positions;
    private readonly INotificationService _notifications;
    
    public async Task Handle(
        OrderPlacedEvent notification,
        CancellationToken ct)
    {
        // 포지션 업데이트
        await _positions.UpdateAsync(notification.Order);
        
        // 알림 발송
        await _notifications.SendAsync(
            $"주문 완료: {notification.Order.StockCode}");
    }
}

// 사용
public class OrderService
{
    private readonly IMediator _mediator;
    
    public async Task PlaceOrderAsync(Order order)
    {
        await _repository.SaveAsync(order);
        
        // 이벤트 발행
        await _mediator.Publish(new OrderPlacedEvent(order));
    }
}
```

---

### 조합 4: Decorator + Chain of Responsibility

```csharp
// Validation Pipeline
public class OrderValidationPipeline
{
    private readonly List<IOrderValidator> _validators;
    
    public OrderValidationPipeline(IEnumerable<IOrderValidator> validators)
    {
        _validators = validators.ToList();
    }
    
    public async Task<ValidationResult> ValidateAsync(Order order)
    {
        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(order);
            if (!result.IsValid)
                return result;
        }
        
        return ValidationResult.Success();
    }
}

// Decorator로 감싸기
public class ValidatedOrderService : IOrderService
{
    private readonly IOrderService _inner;
    private readonly OrderValidationPipeline _pipeline;
    
    public async Task<OrderResult> PlaceOrderAsync(Order order)
    {
        var validation = await _pipeline.ValidateAsync(order);
        
        if (!validation.IsValid)
            return OrderResult.Fail(validation.ErrorMessage);
        
        return await _inner.PlaceOrderAsync(order);
    }
}
```

---

## 성능 최적화 팁

### 1. 비동기 프로그래밍

```csharp
// ❌ 나쁜 예: 동기 호출
public List<StockPrice> GetPrices(List<string> codes)
{
    var prices = new List<StockPrice>();
    foreach (var code in codes)
    {
        prices.Add(_api.GetPrice(code)); // 순차 처리
    }
    return prices;
}

// ✅ 좋은 예: 병렬 비동기
public async Task<List<StockPrice>> GetPricesAsync(List<string> codes)
{
    var tasks = codes.Select(code => _api.GetPriceAsync(code));
    var prices = await Task.WhenAll(tasks); // 병렬 처리
    return prices.ToList();
}
```

---

### 2. 캐싱 전략

```csharp
// Memory Cache
public class CachedService
{
    private readonly IMemoryCache _cache;
    
    public async Task<StockPrice> GetPriceAsync(string code)
    {
        return await _cache.GetOrCreateAsync(
            $"price_{code}",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                return await _api.GetPriceAsync(code);
            });
    }
}

// Distributed Cache (Redis)
public class RedisCache
{
    private readonly IDistributedCache _cache;
    
    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan expiration)
    {
        var cached = await _cache.GetStringAsync(key);
        if (cached != null)
            return JsonSerializer.Deserialize<T>(cached);
        
        var value = await factory();
        var json = JsonSerializer.Serialize(value);
        
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });
        
        return value;
    }
}
```

---

### 3. 배치 처리

```csharp
// Channel을 이용한 Producer-Consumer
public class OrderProcessor
{
    private readonly Channel<Order> _channel;
    
    public OrderProcessor()
    {
        _channel = Channel.CreateUnbounded<Order>();
        
        // Consumer 시작
        _ = ProcessOrdersAsync();
    }
    
    // Producer
    public async Task QueueOrderAsync(Order order)
    {
        await _channel.Writer.WriteAsync(order);
    }
    
    // Consumer
    private async Task ProcessOrdersAsync()
    {
        await foreach (var order in _channel.Reader.ReadAllAsync())
        {
            try
            {
                await ProcessOrderAsync(order);
            }
            catch (Exception ex)
            {
                // 에러 처리
            }
        }
    }
}
```

---

### 4. Connection Pooling

```csharp
// DbContext 설정
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MinPoolSize(5);
        npgsqlOptions.MaxPoolSize(100);
        npgsqlOptions.ConnectionLifetime(300);
    });
});

// HTTP Client Factory
builder.Services.AddHttpClient<IKiwoomApi, KiwoomApi>(client =>
{
    client.BaseAddress = new Uri("https://api.kiwoom.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

---

## 요약 체크리스트

### 필수 패턴 (반드시 적용)

- [ ] **Repository**: 데이터 접근 추상화
- [ ] **Strategy**: 매매 전략 교체
- [ ] **Observer**: 실시간 시세
- [ ] **Builder**: 복잡한 객체 생성
- [ ] **Adapter**: API 통합

### 강력 추천 패턴

- [ ] **Proxy**: 캐싱, Rate Limiting
- [ ] **Chain of Responsibility**: 검증
- [ ] **Command (MediatR)**: CQRS
- [ ] **State**: 주문 상태 관리
- [ ] **Composite**: 포트폴리오 계층

### 피해야 할 패턴

- [ ] ❌ **Singleton** → DI 사용
- [ ] ❌ **Abstract Factory** → 단순 Factory + DI
- [ ] ❌ **Visitor** → LINQ + Pattern Matching

### 최신 기술 스택

- [ ] **의존성 주입 (DI)**
- [ ] **MediatR + CQRS**
- [ ] **Reactive Extensions (Rx)**
- [ ] **Record 타입**
- [ ] **Pattern Matching**

---

## 참고 자료

- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [Reactive Extensions](https://github.com/dotnet/reactive)
- [Scrutor (Decorator)](https://github.com/khellang/Scrutor)
- [Stateless (State Machine)](https://github.com/dotnet-state-machine/stateless)
- [Microsoft Architecture Guide](https://learn.microsoft.com/en-us/dotnet/architecture/)

---

**작성일**: 2025-01-XX  
**버전**: 1.0  
**작성자**: [이름]
