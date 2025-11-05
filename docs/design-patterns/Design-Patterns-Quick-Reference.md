# 디자인 패턴 빠른 참조 가이드

## 📋 패턴 선택 플로우차트

```
주문 생성이 복잡한가?
├─ YES → Builder Pattern
└─ NO → 단순 생성자

여러 증권사 API를 통합하는가?
├─ YES → Adapter Pattern
└─ NO → 직접 구현

실시간 데이터를 여러 곳에 전달하는가?
├─ YES → Observer Pattern (Rx 추천)
└─ NO → 단순 이벤트

매매 전략을 자주 바꾸는가?
├─ YES → Strategy Pattern
└─ NO → 단일 구현

API 호출 비용이 높은가?
├─ YES → Proxy Pattern (캐싱)
└─ NO → 직접 호출

여러 검증 단계가 필요한가?
├─ YES → Chain of Responsibility
└─ NO → 단일 검증

주문 상태 관리가 복잡한가?
├─ YES → State Pattern (Stateless 라이브러리)
└─ NO → Enum으로 관리
```

---

## 🎯 상황별 패턴 추천

### 상황 1: 주문 시스템 구축

**필수**:
- ✅ Builder - 주문 생성
- ✅ Chain of Responsibility - 검증
- ✅ Command (MediatR) - 실행/취소
- ✅ State - 상태 관리

**선택**:
- ⭐ Memento - 백테스트 상태 저장

---

### 상황 2: 실시간 시세 처리

**필수**:
- ✅ Observer (Rx) - 데이터 스트림
- ✅ Flyweight - 종목 정보 캐싱
- ✅ Proxy - Rate Limiting

**선택**:
- ⭐ Mediator - 컴포넌트 간 통신

---

### 상황 3: 매매 전략 시스템

**필수**:
- ✅ Strategy - 전략 교체
- ✅ Template Method - 백테스트 프로세스
- ✅ Prototype - 전략 복제

**선택**:
- ⭐ Memento - 상태 스냅샷

---

### 상황 4: API 통합

**필수**:
- ✅ Adapter - 인터페이스 변환
- ✅ Proxy - 캐싱, Rate Limiting
- ✅ Facade - 복잡한 API 단순화

**선택**:
- ⭐ Bridge - 알림 시스템

---

## 📊 패턴 비교표

### 생성 패턴

| 패턴 | 복잡도 | 유연성 | 추천도 | 사용 시점 |
|------|--------|--------|--------|----------|
| Factory Method | ⭐ | ⭐⭐ | ⭐⭐ | 단순한 객체 생성 |
| Abstract Factory | ⭐⭐⭐ | ⭐⭐⭐ | ⭐ | 관련 객체 그룹 |
| Builder | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | 복잡한 객체 |
| Prototype | ⭐⭐ | ⭐⭐ | ⭐⭐ | 객체 복제 |
| Singleton | ⭐ | ⭐ | ❌ | 사용 금지 (DI) |

---

### 구조 패턴

| 패턴 | 복잡도 | 유연성 | 추천도 | 사용 시점 |
|------|--------|--------|--------|----------|
| Adapter | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | API 통합 |
| Bridge | ⭐⭐ | ⭐⭐⭐ | ⭐⭐ | 구현 분리 |
| Composite | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | 계층 구조 |
| Decorator | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | 동적 기능 |
| Facade | ⭐ | ⭐⭐ | ⭐⭐ | 단순화 |
| Flyweight | ⭐⭐ | ⭐⭐ | ⭐⭐ | 메모리 절약 |
| Proxy | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | 접근 제어 |

---

### 행위 패턴

| 패턴 | 복잡도 | 유연성 | 추천도 | 사용 시점 |
|------|--------|--------|--------|----------|
| Chain of Responsibility | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | 검증 파이프라인 |
| Command | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | 실행/취소 |
| Iterator | ⭐ | ⭐⭐ | ⭐⭐ | C# 기본 제공 |
| Mediator | ⭐⭐⭐ | ⭐⭐ | ⭐⭐ | 복잡한 통신 |
| Memento | ⭐⭐ | ⭐⭐ | ⭐⭐ | 상태 저장 |
| Observer | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | 이벤트 처리 |
| State | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | 상태 관리 |
| Strategy | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | 알고리즘 교체 |
| Template Method | ⭐⭐ | ⭐⭐ | ⭐⭐ | 프로세스 정의 |
| Visitor | ⭐⭐⭐ | ⭐⭐ | ❌ | Pattern Matching |

---

## 🚀 코드 스니펫 모음

### 1. Builder Pattern

```csharp
var order = new OrderBuilder()
    .SetStock("005930")
    .SetQuantity(10)
    .SetPrice(70000)
    .WithStopLoss(68000)
    .WithTakeProfit(75000)
    .Build();
```

### 2. Strategy Pattern

```csharp
// 인터페이스
public interface ITradingStrategy
{
    Task<Signal> GenerateAsync(MarketData data);
}

// 전략 교체
_service.SetStrategy(new MovingAverageStrategy());
```

### 3. Observer Pattern (Rx)

```csharp
var stream = _market.GetPriceStream("005930");
stream
    .Throttle(TimeSpan.FromMilliseconds(100))
    .Subscribe(price => UpdateUI(price));
```

### 4. Adapter Pattern

```csharp
public class KiwoomAdapter : IMarketDataProvider
{
    public async Task<StockPrice> GetPriceAsync(string code)
    {
        var raw = _api.GetStockPrice(code);
        return ParseData(raw);
    }
}
```

### 5. Proxy Pattern

```csharp
public class CachedProxy : IMarketDataProvider
{
    public async Task<StockPrice> GetPriceAsync(string code)
    {
        if (_cache.TryGetValue(code, out var cached))
            return cached;
        
        var price = await _inner.GetPriceAsync(code);
        _cache.Set(code, price, TimeSpan.FromSeconds(5));
        return price;
    }
}
```

### 6. Chain of Responsibility

```csharp
var pipeline = new ValidationPipeline()
    .Use(ValidateBasic)
    .Use(ValidateBalance)
    .Use(ValidateRisk);

var result = await pipeline.ExecuteAsync(order);
```

### 7. Command Pattern (MediatR)

```csharp
// Command
public record PlaceOrderCommand(Order Order) 
    : IRequest<OrderResult>;

// 사용
await _mediator.Send(new PlaceOrderCommand(order));
```

### 8. State Pattern (Stateless)

```csharp
var machine = new StateMachine<OrderState, OrderTrigger>(
    OrderState.Pending);

machine.Configure(OrderState.Pending)
    .Permit(OrderTrigger.Submit, OrderState.Submitted);

await machine.FireAsync(OrderTrigger.Submit);
```

---

## 🔥 최신 기술 스택

### 1. 의존성 주입

```csharp
// Program.cs
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.Decorate<IOrderService, CachedOrderService>();
```

### 2. MediatR

```csharp
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
```

### 3. Reactive Extensions

```csharp
dotnet add package System.Reactive
```

### 4. Record 타입

```csharp
public record Order(string Code, int Qty, decimal Price);
```

### 5. Pattern Matching

```csharp
decimal fee = order switch
{
    { Quantity: > 100 } => 0.001m,
    { Price: > 100000 } => 0.002m,
    _ => 0.003m
};
```

---

## ⚡ 성능 최적화

### 비동기 병렬 처리

```csharp
var tasks = codes.Select(code => GetPriceAsync(code));
var prices = await Task.WhenAll(tasks);
```

### 캐싱

```csharp
await _cache.GetOrCreateAsync(key, async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
    return await FetchDataAsync();
});
```

### Connection Pooling

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(conn, npgsql =>
    {
        npgsql.MinPoolSize(5);
        npgsql.MaxPoolSize(100);
    });
});
```

---

## 📚 추천 라이브러리

| 라이브러리 | 용도 | 설치 |
|-----------|------|------|
| **MediatR** | CQRS, Command | `dotnet add package MediatR` |
| **Scrutor** | Decorator DI | `dotnet add package Scrutor` |
| **System.Reactive** | Rx | `dotnet add package System.Reactive` |
| **Stateless** | State Machine | `dotnet add package Stateless` |
| **FluentValidation** | Validation | `dotnet add package FluentValidation` |
| **AutoMapper** | Object Mapping | `dotnet add package AutoMapper` |
| **Polly** | Resilience | `dotnet add package Polly` |

---

## ❌ 안티패턴

### 1. God Object

```csharp
// ❌ 나쁜 예
public class TradingSystem
{
    public void PlaceOrder() { }
    public void CancelOrder() { }
    public void UpdatePrice() { }
    public void CalculateProfit() { }
    public void SendNotification() { }
    // 너무 많은 책임!
}

// ✅ 좋은 예: 단일 책임 원칙
public class OrderService { }
public class MarketDataService { }
public class PortfolioService { }
public class NotificationService { }
```

### 2. Singleton 남용

```csharp
// ❌ 나쁜 예
var api = KiwoomApi.Instance;

// ✅ 좋은 예: DI
public TradingService(IKiwoomApi api) { }
```

### 3. 과도한 상속

```csharp
// ❌ 나쁜 예
public class BaseStrategy { }
public class TechnicalStrategy : BaseStrategy { }
public class MAStrategy : TechnicalStrategy { }
public class GoldenCrossStrategy : MAStrategy { }

// ✅ 좋은 예: Composition
public class Strategy
{
    private readonly IIndicator _indicator;
}
```

---

## 🎓 학습 로드맵

### Week 1: 기본 패턴
- [ ] Repository Pattern
- [ ] Service Pattern
- [ ] Dependency Injection
- [ ] Builder Pattern

### Week 2: 핵심 패턴
- [ ] Strategy Pattern
- [ ] Observer Pattern
- [ ] Adapter Pattern
- [ ] Chain of Responsibility

### Week 3: 고급 패턴
- [ ] Proxy Pattern
- [ ] Command Pattern (MediatR)
- [ ] State Pattern
- [ ] Composite Pattern

### Week 4: 최신 기술
- [ ] Reactive Extensions
- [ ] CQRS + MediatR
- [ ] Record 타입
- [ ] Pattern Matching

---

## 🔗 참고 자료

### 공식 문서
- [C# Design Patterns](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [.NET Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/)

### 라이브러리
- [MediatR](https://github.com/jbogard/MediatR)
- [Reactive Extensions](https://github.com/dotnet/reactive)
- [Scrutor](https://github.com/khellang/Scrutor)
- [Stateless](https://github.com/dotnet-state-machine/stateless)

### 추가 학습
- [Refactoring Guru](https://refactoring.guru/design-patterns)
- [Source Making](https://sourcemaking.com/design_patterns)

---

## 📝 체크리스트

### 프로젝트 시작 전
- [ ] 어떤 패턴이 필요한지 분석
- [ ] 과도한 패턴 사용 지양
- [ ] 최신 C# 기능 활용 계획
- [ ] 테스트 가능한 구조 설계

### 개발 중
- [ ] SOLID 원칙 준수
- [ ] 단일 책임 원칙
- [ ] 의존성 주입 활용
- [ ] 비동기 프로그래밍

### 코드 리뷰
- [ ] 불필요한 패턴 제거
- [ ] 성능 병목 확인
- [ ] 테스트 커버리지 확인
- [ ] 문서화

---

**버전**: 1.0  
**최종 수정**: 2025-01-XX
