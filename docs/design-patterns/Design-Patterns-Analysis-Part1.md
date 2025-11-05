# 디자인 패턴 분석 및 추천 가이드 (Part 1)

## 📋 목차
1. [유사/중복 패턴 그룹 분석](#유사중복-패턴-그룹-분석)
2. [주식 자동매매 시스템 추천 패턴](#주식-자동매매-시스템-추천-패턴)
3. [사용하지 말아야 할 패턴](#사용하지-말아야-할-패턴)

---

## 유사/중복 패턴 그룹 분석

### 그룹 1: 객체 생성 관련 (높은 유사도)

#### 패턴 비교

| 패턴 | 특징 | 사용 시점 | 복잡도 |
|------|------|----------|--------|
| **Factory Method** | 하나의 객체 타입 생성 | 주문 타입별 생성 | ⭐ |
| **Abstract Factory** | 관련된 객체 그룹 생성 | 증권사별 API 세트 | ⭐⭐⭐ |
| **Builder** | 복잡한 객체 단계적 생성 | 많은 옵션이 있는 주문 | ⭐⭐ |

#### 추천 순위

1. ⭐⭐⭐ **Builder** (현대적, 가독성 최고)
2. ⭐⭐ **Factory Method** (단순한 경우)
3. ⭐ **Abstract Factory** (복잡한 제품군)

#### 최신 코드 예시

```csharp
// 🆕 Record 타입 + with 표현식
public record Order(string StockCode, int Quantity, decimal Price)
{
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfit { get; init; }
    
    public Order WithStopLoss(decimal sl) => this with { StopLoss = sl };
    public Order WithTakeProfit(decimal tp) => this with { TakeProfit = tp };
}

// 사용
var order = new Order("005930", 10, 70000)
    .WithStopLoss(68000)
    .WithTakeProfit(75000);
```

---

### 그룹 2: 인터페이스 변환/단순화

#### 패턴 비교

| 패턴 | 목적 | 차이점 |
|------|------|--------|
| **Adapter** | 인터페이스 연결 | 기존 코드 변경 없이 사용 |
| **Facade** | 시스템 단순화 | 여러 서브시스템을 하나로 |
| **Proxy** | 접근 제어 | 원본 객체 대리 + 추가 기능 |

#### 추천 순위

1. ⭐⭐⭐ **Adapter** (API 통합 필수)
2. ⭐⭐⭐ **Proxy** (캐싱, Rate Limiting)
3. ⭐⭐ **Facade** (레거시 통합)

#### 최신 코드 예시

```csharp
// 🆕 Decorator + Proxy 조합
public class CachedOrderService : IOrderService
{
    private readonly IOrderService _inner;
    private readonly IMemoryCache _cache;
    
    public CachedOrderService(IOrderService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }
    
    public async Task<OrderResult> PlaceOrderAsync(Order order)
    {
        var key = $"order_{order.OrderId}";
        if (_cache.TryGetValue(key, out OrderResult cached))
            return cached;
        
        var result = await _inner.PlaceOrderAsync(order);
        _cache.Set(key, result, TimeSpan.FromMinutes(5));
        return result;
    }
}

// DI 등록 (Scrutor)
services.AddScoped<IOrderService, OrderService>();
services.Decorate<IOrderService, CachedOrderService>();
```

---

### 그룹 3: 객체 간 통신

#### 패턴 비교

| 패턴 | 통신 방식 | 사용 케이스 |
|------|----------|-------------|
| **Chain of Responsibility** | 순차 체인 | 주문 검증 파이프라인 |
| **Mediator** | 중앙 집중식 | 복잡한 컴포넌트 통신 |
| **Observer** | 발행-구독 | 실시간 시세 업데이트 |

#### 추천 순위

1. ⭐⭐⭐ **Observer** (이벤트 기반 핵심)
2. ⭐⭐ **Chain of Responsibility** (검증)
3. ⭐ **Mediator** (복잡도 높을 때만)

#### 최신 코드 예시

```csharp
// 🆕 Reactive Extensions
using System.Reactive.Linq;

public IObservable<StockPrice> GetPriceStream(string code)
{
    return Observable
        .Interval(TimeSpan.FromSeconds(1))
        .SelectMany(_ => _api.GetPriceAsync(code))
        .DistinctUntilChanged()
        .Retry(3);
}

// 사용
var stream = marketService.GetPriceStream("005930");
stream
    .Throttle(TimeSpan.FromMilliseconds(100))
    .Subscribe(price => Console.WriteLine($"Price: {price:N0}"));
```

---

### 그룹 4: 알고리즘 캡슐화

#### 패턴 비교

| 패턴 | 차이점 | 유연성 |
|------|--------|--------|
| **Strategy** | 런타임 알고리즘 교체 | ⭐⭐⭐ |
| **Template Method** | 알고리즘 골격 고정 | ⭐⭐ |

#### 추천 순위

1. ⭐⭐⭐ **Strategy** (현대적)
2. ⭐⭐ **Template Method** (레거시)

#### 최신 코드 예시

```csharp
// 🆕 Strategy를 Func로
public class PositionSizer
{
    private Func<decimal, decimal, int> _strategy;
    
    public void SetStrategy(Func<decimal, decimal, int> strategy)
    {
        _strategy = strategy;
    }
    
    public int Calculate(decimal balance, decimal price)
        => _strategy(balance, price);
}

// 사용
sizer.SetStrategy((balance, price) => (int)(balance * 0.1m / price));
```

---

## 주식 자동매매 시스템 추천 패턴

### ⭐⭐⭐ 필수 패턴 TOP 5

#### 1. Repository Pattern

**데이터 접근 추상화**

```csharp
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<List<Order>> FindAsync(ISpecification<Order> spec);
    Task SaveAsync(Order order);
}

// Specification Pattern
public class ActiveOrdersSpec : ISpecification<Order>
{
    public Expression<Func<Order, bool>> Criteria 
        => o => o.Status == "ACTIVE";
}
```

#### 2. Strategy Pattern

**매매 전략 교체**

```csharp
public interface ITradingStrategy
{
    Task<Signal> GenerateSignalAsync(MarketData data);
}

services.AddTransient<ITradingStrategy, MovingAverageStrategy>();
services.AddTransient<ITradingStrategy, RSIStrategy>();
```

#### 3. Observer Pattern

**실시간 시세 업데이트**

```csharp
IObservable<StockPrice> stream = _market
    .GetPriceStream("005930")
    .Where(p => p.CurrentPrice > 70000)
    .Throttle(TimeSpan.FromMilliseconds(100));

stream.Subscribe(price => UpdateUI(price));
```

#### 4. Builder Pattern

**복잡한 주문 생성**

```csharp
var order = new OrderBuilder()
    .SetStock("005930")
    .SetQuantity(10)
    .SetPrice(70000)
    .WithStopLoss(68000)
    .Build();
```

#### 5. Adapter Pattern

**증권사 API 통합**

```csharp
public interface IMarketDataProvider
{
    Task<StockPrice> GetPriceAsync(string code);
}

public class KiwoomAdapter : IMarketDataProvider { }
public class KoreaInvestmentAdapter : IMarketDataProvider { }
```

---

### ⭐⭐ 강력 추천 패턴

#### 6. Proxy Pattern - 캐싱/Rate Limiting

```csharp
public class CachedProxy : IMarketDataProvider
{
    private readonly IMarketDataProvider _inner;
    private readonly IMemoryCache _cache;
    private readonly SemaphoreSlim _limiter;
    
    public async Task<StockPrice> GetPriceAsync(string code)
    {
        if (_cache.TryGetValue(code, out StockPrice cached))
            return cached;
        
        await _limiter.WaitAsync();
        try
        {
            var price = await _inner.GetPriceAsync(code);
            _cache.Set(code, price, TimeSpan.FromSeconds(5));
            return price;
        }
        finally
        {
            _limiter.Release();
        }
    }
}
```

#### 7. Chain of Responsibility - 검증

```csharp
public class ValidationPipeline
{
    private List<Func<Order, Task<ValidationResult>>> _validators = new();
    
    public ValidationPipeline Use(Func<Order, Task<ValidationResult>> v)
    {
        _validators.Add(v);
        return this;
    }
    
    public async Task<ValidationResult> ExecuteAsync(Order order)
    {
        foreach (var validator in _validators)
        {
            var result = await validator(order);
            if (!result.IsValid) return result;
        }
        return ValidationResult.Success();
    }
}
```

#### 8. Command Pattern - MediatR

```csharp
// Command
public record PlaceOrderCommand(Order Order) : IRequest<OrderResult>;

// Handler
public class PlaceOrderHandler 
    : IRequestHandler<PlaceOrderCommand, OrderResult>
{
    public async Task<OrderResult> Handle(
        PlaceOrderCommand request, CancellationToken ct)
    {
        // 주문 처리
    }
}

// 사용
await _mediator.Send(new PlaceOrderCommand(order));
```

---

## 사용하지 말아야 할 패턴

### ❌ Singleton Pattern

**문제점**:
- 테스트 어려움 (Mock 불가)
- 전역 상태
- 멀티스레드 이슈

**대안: 의존성 주입**

```csharp
// ❌ 나쁜 예
var api = KiwoomApi.Instance;

// ✅ 좋은 예
services.AddSingleton<IKiwoomApi, KiwoomApi>();

public class TradingService
{
    public TradingService(IKiwoomApi api) // DI 주입
    {
        _api = api;
    }
}
```

---

### ⚠️ Abstract Factory

**문제점**:
- 과도한 추상화
- 확장성 부족

**대안: Factory Method + DI**

```csharp
// ✅ 간단한 Factory
public interface IClientFactory
{
    IOrderClient Create(string broker);
}

public class ClientFactory : IClientFactory
{
    private readonly IServiceProvider _provider;
    
    public IOrderClient Create(string broker)
    {
        return broker switch
        {
            "Kiwoom" => _provider.GetRequiredService<KiwoomClient>(),
            "KI" => _provider.GetRequiredService<KIClient>(),
            _ => throw new ArgumentException()
        };
    }
}
```

---

### ⚠️ Visitor Pattern

**문제점**:
- 복잡도 높음
- 유지보수 어려움

**대안: LINQ + Pattern Matching**

```csharp
// ✅ 간단한 Pattern Matching
public decimal CalculateValue(Portfolio p)
{
    return p.Elements.Sum(e => e switch
    {
        StockPosition s => s.Quantity * s.Price,
        BondPosition b => b.FaceValue * b.CurrentPrice / 100,
        CashPosition c => c.Amount,
        _ => 0
    });
}
```

---

## 다음 페이지

👉 [Part 2: 2025년 최신 트렌드 및 실전 가이드](Design-Patterns-Analysis-Part2.md)
