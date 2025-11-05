# 주식 자동매매 알고리즘 시스템 (Stock Trading Algorithm with DDD)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com)

> 🚀 **엔터프라이즈급 알고리즘 트레이딩 플랫폼**  
> DDD, Clean Architecture, CQRS 패턴을 적용한 확장 가능하고 유지보수가 용이한 주식 자동매매 시스템

---

## 📋 목차

- [프로젝트 소개](#-프로젝트-소개)
- [주요 특징](#-주요-특징)
- [시스템 아키텍처](#-시스템-아키텍처)
- [기술 스택](#-기술-스택)
- [프로젝트 구조](#-프로젝트-구조)
- [개발 로드맵](#-개발-로드맵)
- [시작하기](#-시작하기)
- [개발 가이드](#-개발-가이드)
- [Bounded Context](#-bounded-context)
- [기여하기](#-기여하기)
- [라이센스](#-라이센스)

---

## 🎯 프로젝트 소개

본 프로젝트는 **Domain-Driven Design (DDD)** 원칙을 기반으로 설계된 전문가급 알고리즘 트레이딩 시스템입니다. 실시간 시장 데이터 수신, 자동 거래 전략 실행, 백테스팅, 리스크 관리 등 체계적인 알고리즘 트레이딩에 필요한 모든 기능을 제공합니다.

### 왜 이 프로젝트인가?

- ✅ **엔터프라이즈급 아키텍처**: Clean Architecture + DDD로 설계된 확장 가능한 구조
- ✅ **실전 검증된 패턴**: CQRS, Event Sourcing, Repository 등 최신 디자인 패턴 적용
- ✅ **하이브리드 UI**: WPF(데스크톱) + Blazor(웹) 동시 지원
- ✅ **실시간 처리**: SignalR 기반 실시간 데이터 스트리밍
- ✅ **완벽한 테스트**: 단위 테스트, 통합 테스트, E2E 테스트 포함

---

## ✨ 주요 특징

### 1️⃣ 시장 데이터 관리 (Market Data Context)
- 📊 **실시간 시세 수신**: 키움 OpenAPI를 통한 실시간 현재가/호가/체결 데이터
- 📈 **과거 데이터 수집**: 일봉/분봉 데이터 자동 수집 및 TimescaleDB 저장
- 🔍 **종목 검색/필터링**: 기술적 지표, 재무 지표 기반 종목 스크리닝
- 💾 **효율적인 데이터 관리**: Redis 캐싱 + PostgreSQL로 고성능 데이터 처리

### 2️⃣ 전략 개발 및 관리 (Strategy Context)
- 🧠 **다양한 전략 작성 방법**
  - C# 코드 기반 전략 개발
  - 노코드 빌더 UI (드래그 앤 드롭)
  - 사전 정의된 템플릿 활용
- 🎯 **신호 생성**: 실시간 매매 신호 자동 생성
- ⚙️ **파라미터 최적화**: 유전자 알고리즘, 그리드 서치
- 📊 **Walk-Forward 분석**: 과최적화 방지

### 3️⃣ 주문 실행 관리 (Trading Context)
- 🎯 **다양한 주문 타입**: 시장가, 지정가, 조건부 주문
- ⚡ **실시간 체결 처리**: 이벤트 소싱 기반 체결 추적
- 🔄 **주문 수정/취소**: 상태 머신 패턴으로 안전한 주문 관리
- 📝 **감사 추적**: 모든 주문 이력 완벽 보관

### 4️⃣ 포트폴리오 관리 (Account Context)
- 💼 **실시간 포지션 추적**: 평균 단가, 미실현/실현 손익
- 🏦 **다중 계좌 지원**: 여러 증권 계좌 통합 관리
- 📊 **자산 배분**: 자동 리밸런싱, 리스크 패리티
- 📈 **성과 분석**: 샤프 비율, MDD, 승률 등 상세 지표

### 5️⃣ 백테스팅 및 최적화 (Strategy Context)
- 🔄 **과거 데이터 시뮬레이션**: 슬리피지, 수수료 반영
- 📊 **성과 분석**: 30개 이상의 성과 지표 제공
- 📈 **Equity Curve**: 시각적 수익 곡선 생성
- 📄 **자동 리포트**: PDF/HTML 백테스트 리포트

### 6️⃣ 리스크 관리 (Risk Management Context)
- ⚠️ **실시간 리스크 모니터링**: VaR, CVaR 계산
- 🛑 **손절/익절 자동 실행**: 트레일링 스탑 지원
- 🚨 **킬 스위치**: 긴급 상황 전체 포지션 청산
- 📏 **포지션 사이징**: 켈리 기준, 고정 비율 등

### 7️⃣ 모니터링 및 알림 (Monitoring Context)
- 📊 **실시간 대시보드**: 커스터마이징 가능한 위젯 구성
- 🔔 **다채널 알림**: 이메일, SMS, Telegram, Slack
- 📝 **로그 관리**: Serilog + ELK Stack
- 🏥 **헬스 체크**: 시스템 상태 실시간 모니터링

---

## 🏗️ 시스템 아키텍처

### Clean Architecture + DDD

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────────┐ │
│  │  WPF Desktop│  │ Blazor Web   │  │  ASP.NET API   │ │
│  └─────────────┘  └──────────────┘  └────────────────┘ │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────┴────────────────────────────────┐
│                 Application Layer (CQRS)                 │
│  ┌─────────────────────┐   ┌─────────────────────────┐  │
│  │  Commands/Queries   │   │    MediatR Handlers     │  │
│  └─────────────────────┘   └─────────────────────────┘  │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────┴────────────────────────────────┐
│                     Domain Layer (DDD)                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │  Aggregates  │  │    Entities  │  │Value Objects │  │
│  │   (Roots)    │  │              │  │              │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │Domain Events │  │Domain Service│  │ Repositories │  │
│  │              │  │              │  │ (Interfaces) │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────┴────────────────────────────────┐
│                  Infrastructure Layer                    │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────────┐ │
│  │ PostgreSQL  │  │    Redis     │  │   RabbitMQ     │ │
│  │TimescaleDB  │  │              │  │                │ │
│  └─────────────┘  └──────────────┘  └────────────────┘ │
│  ┌─────────────┐  ┌──────────────┐  ┌────────────────┐ │
│  │  키움 API   │  │   SignalR    │  │ Background Svc │ │
│  └─────────────┘  └──────────────┘  └────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 주요 설계 원칙

1. **의존성 역전 원칙 (DIP)**: 모든 의존성은 안쪽(Core)을 향함
2. **관심사의 분리**: 각 레이어는 명확한 책임 보유
3. **Bounded Context**: 도메인별 명확한 경계 설정
4. **이벤트 주도**: Domain Events를 통한 느슨한 결합

---

## 🛠️ 기술 스택

### Backend
- **런타임**: .NET 8.0 / C# 12.0
- **웹 프레임워크**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **CQRS**: MediatR 12.x
- **검증**: FluentValidation 11.x
- **매핑**: AutoMapper 12.x

### Database
- **관계형 DB**: PostgreSQL 17
- **시계열 DB**: TimescaleDB (PostgreSQL Extension)
- **캐싱**: Redis 7.x
- **메시지 큐**: RabbitMQ 3.x

### Frontend
- **데스크톱**: WPF (.NET 8) + MVVM Toolkit
- **웹**: Blazor Server (.NET 8)
- **실시간 통신**: SignalR
- **차트**: Chart.js, LiveCharts2

### DevOps & Infrastructure
- **컨테이너**: Docker + Docker Compose
- **오케스트레이션**: Kubernetes (Optional)
- **CI/CD**: GitHub Actions / Azure Pipelines
- **클라우드**: Azure (App Service, Blob Storage, Key Vault)
- **모니터링**: Prometheus + Grafana
- **로깅**: Serilog + Elasticsearch (ELK Stack)

### External APIs
- **한국 증권**: 키움 OpenAPI (ActiveX)
- **대체 API**: eBest API (향후 지원 예정)

### Testing
- **단위 테스트**: xUnit, NUnit
- **모킹**: Moq, NSubstitute
- **통합 테스트**: WebApplicationFactory
- **E2E 테스트**: Playwright (for Blazor)

---

## 📁 프로젝트 구조

```
AlgoTradingSystem/
│
├── src/                                    # 소스 코드
│   ├── 1.Core/                            # 도메인 레이어
│   │   └── AlgoTrading.Core/
│   │       ├── Entities/                  # Aggregate Roots & Entities
│   │       │   ├── Trading/              # Order, Position, Trade
│   │       │   ├── Strategy/             # TradingStrategy, Signal
│   │       │   ├── MarketData/           # Stock, MarketData
│   │       │   ├── Portfolio/            # Portfolio, Account
│   │       │   └── Risk/                 # RiskProfile, RiskAlert
│   │       ├── ValueObjects/             # Money, Percentage, Price
│   │       ├── DomainEvents/             # OrderFilledEvent 등
│   │       ├── Interfaces/               # Repository Interfaces
│   │       └── Exceptions/               # Domain Exceptions
│   │
│   ├── 2.Application/                     # 애플리케이션 레이어
│   │   └── AlgoTrading.Application/
│   │       ├── Commands/                 # CQRS Commands
│   │       ├── Queries/                  # CQRS Queries
│   │       ├── Handlers/                 # MediatR Handlers
│   │       ├── DTOs/                     # Data Transfer Objects
│   │       ├── Services/                 # Application Services
│   │       ├── Validators/               # FluentValidation
│   │       ├── Mappers/                  # AutoMapper Profiles
│   │       └── Behaviors/                # MediatR Behaviors
│   │
│   ├── 3.Infrastructure/                  # 인프라 레이어
│   │   └── AlgoTrading.Infrastructure/
│   │       ├── Persistence/              # EF Core, DbContext
│   │       ├── ExternalServices/         # 키움 API 통합
│   │       ├── Caching/                  # Redis 구현
│   │       ├── MessageBus/               # RabbitMQ 구현
│   │       ├── BackgroundServices/       # 백그라운드 작업
│   │       ├── Notifications/            # 알림 서비스
│   │       └── Logging/                  # Serilog 설정
│   │
│   ├── 4.Presentation/                    # 프레젠테이션 레이어
│   │   ├── AlgoTrading.Desktop/          # WPF 데스크톱 앱
│   │   │   ├── Views/                    # XAML Views
│   │   │   ├── ViewModels/               # MVVM ViewModels
│   │   │   ├── Controls/                 # Custom Controls
│   │   │   └── Services/                 # UI Services
│   │   │
│   │   ├── AlgoTrading.Web/              # Blazor 웹 앱
│   │   │   ├── Pages/                    # Blazor Pages
│   │   │   ├── Components/               # Razor Components
│   │   │   ├── Shared/                   # Shared Components
│   │   │   ├── Services/                 # Blazor Services
│   │   │   └── Hubs/                     # SignalR Hubs
│   │   │
│   │   └── AlgoTrading.API/              # Web API
│   │       ├── Controllers/              # API Controllers
│   │       ├── Middleware/               # Custom Middleware
│   │       └── Filters/                  # Action Filters
│   │
│   └── 5.Shared/                          # 공유 라이브러리
│       └── AlgoTrading.Shared/
│           ├── Constants/                # 상수 정의
│           ├── Extensions/               # 확장 메서드
│           └── Utilities/                # 유틸리티 클래스
│
├── tests/                                 # 테스트 프로젝트
│   ├── AlgoTrading.UnitTests/            # 단위 테스트
│   ├── AlgoTrading.IntegrationTests/     # 통합 테스트
│   └── AlgoTrading.E2ETests/             # E2E 테스트
│
├── docs/                                  # 문서
│   ├── architecture/                      # 아키텍처 문서
│   ├── design/                            # 설계 문서
│   ├── development/                       # 개발 가이드
│   └── user-manual/                       # 사용자 매뉴얼
│
├── scripts/                               # 스크립트
│   ├── database/                          # DB 스크립트
│   ├── deployment/                        # 배포 스크립트
│   └── development/                       # 개발 환경 설정
│
├── deployment/                            # 배포 설정
│   ├── docker/                            # Docker 설정
│   ├── kubernetes/                        # K8s 설정
│   └── ci-cd/                             # CI/CD 파이프라인
│
├── AlgoTradingSystem.sln                 # 솔루션 파일
└── README.md                              # 본 문서
```

---

## 🗺️ 개발 로드맵

### Phase 1: 핵심 거래 기능 (1-2개월) 🔴

**목표**: 기본적인 거래 실행 시스템 완성

#### 구현 기능
- ✅ 키움 API 연동 및 실시간 시세 수신
- ✅ 과거 데이터 수집 및 저장
- ✅ 주문 생성/전송/체결 처리
- ✅ 포지션 및 계좌 관리
- ✅ 기본 모니터링 및 로깅
- ✅ 사용자 인증/권한

#### 검증 기준
- 실제 계좌 연동 가능
- 수동 주문 실행 가능
- 실시간 데이터 정상 수신 (지연 < 50ms)
- 주문 실행 지연 < 100ms

---

### Phase 2: 전략 실행 (2-3개월) 🟡

**목표**: 자동 트레이딩 전략 실행 가능

#### 구현 기능
- ✅ 종목 검색/필터링
- ✅ 전략 개발 프레임워크 (코드 기반 + 빌더 UI)
- ✅ 실시간 신호 생성
- ✅ 백테스팅 엔진
- ✅ 리스크 관리 규칙
- ✅ 실시간 대시보드
- ✅ 주문 이력 및 손익 분석

#### 검증 기준
- 최소 3개 이상의 전략 정상 동작
- 백테스팅과 실전 결과 일치도 > 95%
- 대시보드 응답 속도 < 200ms

---

### Phase 3: 고급 기능 (3-4개월) 🟢

**목표**: 전문 트레이더 수준의 분석 도구

#### 구현 기능
- ✅ 전략 파라미터 최적화 (유전자 알고리즘, 그리드 서치)
- ✅ Walk-Forward 분석
- ✅ 전략 비교 및 성과 분석
- ✅ 자동 리포트 생성
- ✅ 포트폴리오 최적화

#### 검증 기준
- 최적화 수렴 시간 < 30분
- 리포트 생성 시간 < 10초

---

### Phase 4: 확장 기능 (4-5개월) 🔵

**목표**: 엔터프라이즈급 완성도

#### 구현 기능
- ✅ 자산 배분 및 자동 리밸런싱
- ✅ 다채널 알림 시스템
- ✅ 뉴스 수집 및 감성 분석
- ✅ 완전한 백업/복구 시스템
- ✅ 모바일 앱 (향후)

#### 검증 기준
- 시스템 가동률 > 99.9%
- 알림 발송 성공률 > 99%
- 백업 성공률 100%

---

## 🚀 시작하기

### 필수 요구사항

#### 소프트웨어
- **운영체제**: Windows 10/11 (키움 API는 Windows 전용)
- **.NET SDK**: 8.0 이상
- **IDE**: Visual Studio 2022 또는 JetBrains Rider
- **Docker Desktop**: 20.10 이상 (선택 사항)

#### 외부 계정
- **키움증권 계좌**: 모의투자 또는 실제 계좌
- **키움 OpenAPI**: 신청 및 설치 필요

### 설치 및 실행

#### 1. 리포지토리 클론

```bash
git clone https://github.com/LeeJooHyung98/AlgoTradingSystem.git
cd AlgoTradingSystem
```

#### 2. 환경 설정

**appsettings.json 설정**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=algotrading;Username=postgres;Password=your_password",
    "Redis": "localhost:6379",
    "RabbitMQ": "amqp://guest:guest@localhost:5672"
  },
  "Kiwoom": {
    "AccountNumber": "your_account_number",
    "MockMode": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**User Secrets 설정 (민감 정보)**

```bash
dotnet user-secrets init --project src/4.Presentation/AlgoTrading.API
dotnet user-secrets set "Kiwoom:Password" "your_password" --project src/4.Presentation/AlgoTrading.API
```

#### 3. 데이터베이스 설정

**Docker로 PostgreSQL, Redis, RabbitMQ 실행**

```bash
cd deployment/docker
docker-compose up -d
```

**마이그레이션 실행**

```bash
cd src/3.Infrastructure/AlgoTrading.Infrastructure
dotnet ef database update
```

#### 4. 프로젝트 빌드 및 실행

**솔루션 빌드**

```bash
dotnet build AlgoTradingSystem.sln
```

**WPF 데스크톱 앱 실행**

```bash
cd src/4.Presentation/AlgoTrading.Desktop
dotnet run
```

**Web API 실행**

```bash
cd src/4.Presentation/AlgoTrading.API
dotnet run
```

**Blazor 웹 앱 실행**

```bash
cd src/4.Presentation/AlgoTrading.Web
dotnet run
```

#### 5. 접속 확인

- **WPF 앱**: 실행된 데스크톱 애플리케이션
- **Web API**: https://localhost:5001/swagger
- **Blazor 웹**: https://localhost:7001

---

## 📚 개발 가이드

### Bounded Context 개요

본 시스템은 6개의 주요 Bounded Context로 구성됩니다:

#### 1. Market Data Context
**책임**: 시장 데이터 수집, 저장, 제공

```csharp
// Aggregate Root
public class Stock : AggregateRoot
{
    public StockCode Code { get; private set; }
    public string Name { get; private set; }
    public Market Market { get; private set; }
    
    // 도메인 로직
    public void UpdatePrice(Price newPrice)
    {
        if (!IsTradingHours())
            throw new MarketClosedException();
            
        CurrentPrice = newPrice;
        AddDomainEvent(new PriceUpdatedEvent(Code, newPrice));
    }
}
```

#### 2. Strategy Context
**책임**: 전략 정의, 신호 생성, 백테스팅

```csharp
// Strategy Interface
public interface ITradingStrategy
{
    string StrategyName { get; }
    StrategyType Type { get; }
    
    Task<Signal> GenerateSignalAsync(MarketData data);
    Task<BacktestResult> BacktestAsync(BacktestConfig config);
}

// 전략 구현 예시
public class MovingAverageCrossStrategy : ITradingStrategy
{
    public async Task<Signal> GenerateSignalAsync(MarketData data)
    {
        var shortMA = CalculateMA(data, 5);
        var longMA = CalculateMA(data, 20);
        
        if (shortMA > longMA)
            return Signal.Buy;
        else if (shortMA < longMA)
            return Signal.Sell;
            
        return Signal.Hold;
    }
}
```

#### 3. Trading Context
**책임**: 주문 생성, 실행, 체결 처리

```csharp
// Order Aggregate
public class Order : AggregateRoot
{
    public OrderId Id { get; private set; }
    public StockCode StockCode { get; private set; }
    public OrderType Type { get; private set; }
    public OrderStatus Status { get; private set; }
    public Quantity Quantity { get; private set; }
    public Price Price { get; private set; }
    
    // 도메인 로직
    public void Execute()
    {
        ValidateCanExecute();
        Status = OrderStatus.Pending;
        AddDomainEvent(new OrderExecutedEvent(Id));
    }
    
    public void MarkAsFilled(Execution execution)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException();
            
        Status = OrderStatus.Filled;
        AddDomainEvent(new OrderFilledEvent(Id, execution));
    }
}
```

#### 4. Account Context
**책임**: 계좌 관리, 포지션 추적, 손익 계산

```csharp
// Portfolio Aggregate
public class Portfolio : AggregateRoot
{
    private readonly List<Position> _positions = new();
    public AccountId AccountId { get; private set; }
    public Money TotalEquity { get; private set; }
    
    public void AddPosition(Position position)
    {
        ValidateRiskLimits(position);
        _positions.Add(position);
        RecalculateEquity();
    }
    
    public Money CalculateUnrealizedPnL()
    {
        return _positions.Sum(p => p.UnrealizedPnL);
    }
}
```

#### 5. Risk Management Context
**책임**: 리스크 모니터링, 한도 관리, 긴급 통제

```csharp
// Risk Manager Domain Service
public class RiskManager
{
    public async Task<RiskCheckResult> ValidateOrderAsync(Order order)
    {
        var checks = new[]
        {
            CheckPositionLimit(order),
            CheckExposureLimit(order),
            CheckDrawdownLimit(order),
            CheckVolatilityLimit(order)
        };
        
        var results = await Task.WhenAll(checks);
        return RiskCheckResult.Aggregate(results);
    }
}
```

#### 6. Monitoring Context
**책임**: 시스템 모니터링, 알림, 리포트

```csharp
// 알림 발송 예시
public class NotificationService : INotificationService
{
    public async Task SendAlertAsync(Alert alert)
    {
        await Task.WhenAll(
            SendEmailAsync(alert),
            SendSmsAsync(alert),
            SendTelegramAsync(alert)
        );
    }
}
```

---

### CQRS 패턴 적용

#### Command 예시

```csharp
// Command
public record CreateOrderCommand(
    StockCode StockCode,
    OrderType Type,
    Quantity Quantity,
    Price Price
) : IRequest<OrderId>;

// Command Handler
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderId>
{
    private readonly IOrderRepository _repository;
    private readonly IRiskManager _riskManager;
    
    public async Task<OrderId> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // 1. 리스크 검증
        await _riskManager.ValidateOrderAsync(request);
        
        // 2. 주문 생성
        var order = Order.Create(
            request.StockCode,
            request.Type,
            request.Quantity,
            request.Price
        );
        
        // 3. 저장
        await _repository.AddAsync(order);
        await _repository.UnitOfWork.SaveChangesAsync(ct);
        
        return order.Id;
    }
}
```

#### Query 예시

```csharp
// Query
public record GetPortfolioQuery(AccountId AccountId) : IRequest<PortfolioDto>;

// Query Handler
public class GetPortfolioQueryHandler : IRequestHandler<GetPortfolioQuery, PortfolioDto>
{
    private readonly IReadDbContext _context;
    
    public async Task<PortfolioDto> Handle(GetPortfolioQuery request, CancellationToken ct)
    {
        // 읽기 전용 컨텍스트에서 조회 (성능 최적화)
        return await _context.Portfolios
            .AsNoTracking()
            .Where(p => p.AccountId == request.AccountId)
            .ProjectTo<PortfolioDto>()
            .FirstOrDefaultAsync(ct);
    }
}
```

---

### Domain Events 처리

```csharp
// Domain Event
public record OrderFilledEvent(
    OrderId OrderId,
    Execution Execution
) : IDomainEvent;

// Event Handler
public class OrderFilledEventHandler : INotificationHandler<OrderFilledEvent>
{
    private readonly IPortfolioRepository _portfolio;
    private readonly INotificationService _notification;
    
    public async Task Handle(OrderFilledEvent @event, CancellationToken ct)
    {
        // 1. 포트폴리오 업데이트
        var portfolio = await _portfolio.GetByOrderIdAsync(@event.OrderId);
        portfolio.AddExecution(@event.Execution);
        
        // 2. 알림 발송
        await _notification.SendAsync(
            new Alert($"주문 체결: {@event.Execution.StockCode}")
        );
    }
}
```

---

### 테스트 작성 가이드

#### 단위 테스트 예시

```csharp
public class OrderTests
{
    [Fact]
    public void Execute_WhenValid_ShouldChangeStatusToPending()
    {
        // Arrange
        var order = Order.Create(
            new StockCode("005930"),
            OrderType.Market,
            new Quantity(10),
            new Price(70000)
        );
        
        // Act
        order.Execute();
        
        // Assert
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Contains(order.DomainEvents, e => e is OrderExecutedEvent);
    }
    
    [Fact]
    public void Execute_WhenAlreadyExecuted_ShouldThrowException()
    {
        // Arrange
        var order = Order.Create(/*...*/);
        order.Execute();
        
        // Act & Assert
        Assert.Throws<InvalidOrderStateException>(() => order.Execute());
    }
}
```

#### 통합 테스트 예시

```csharp
public class OrderIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public OrderIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateOrder_WhenValid_ShouldReturn201()
    {
        // Arrange
        var command = new CreateOrderCommand(/*...*/);
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", command);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var orderId = await response.Content.ReadFromJsonAsync<OrderId>();
        Assert.NotNull(orderId);
    }
}
```

---

### 코딩 컨벤션

#### 네이밍
- **클래스**: PascalCase (예: `OrderService`, `MarketDataRepository`)
- **메서드**: PascalCase (예: `ExecuteOrder()`, `GetPortfolio()`)
- **변수**: camelCase (예: `orderRepository`, `currentPrice`)
- **상수**: UPPER_SNAKE_CASE (예: `MAX_ORDER_SIZE`)
- **인터페이스**: I + PascalCase (예: `IOrderRepository`)

#### 파일 구조
- 1 파일 = 1 클래스 원칙
- 네임스페이스는 폴더 구조와 일치
- 테스트 파일은 `{ClassName}Tests.cs` 형식

#### 비동기 코드
- 모든 I/O 작업은 `async/await` 사용
- 메서드 이름은 `Async` 접미사 사용 (예: `GetOrderAsync()`)

```csharp
// ✅ Good
public async Task<Order> GetOrderAsync(OrderId id)
{
    return await _context.Orders.FindAsync(id);
}

// ❌ Bad
public Order GetOrder(OrderId id)
{
    return _context.Orders.Find(id);  // 동기 호출
}
```

#### XML 문서 주석 형식

##### 함수 및 Class 
- 가능하면 자세하게 설명한다.
- 주석은 아래와 같은 형식으로 작성한다.
```
    /// <summary>
    /// description(설명) : 영문내용(한글내용)
    /// Details(상세설명) : 영문내용(한글내용)
    /// Applied technology patterns(적용기술패턴) : 영문내용(한글내용)
    /// </summary>
    /// <returns></returns>
```
##### 속성 
- 가능하면 자세하게 설명한다.
- 주석은 아래와 같은 형식으로 작성한다.
```
    /// <summary>
    /// 설명
    /// </summary>
```
##### 변수 
- 가능하면 자세하게 설명한다.
- 변수는 Line 뒷 부분에 일정 공백을 두고 "//"를 추가하고 변수의 내용을 설명한다. 
```
    ThreeMinutes = 3;   // 3분봉(한글내용)
```
#### 개발 코드 수준
- 가능한 고급개발자 기준으로 코드를 작성한다.
---

## 🧪 테스트 실행

### 전체 테스트 실행

```bash
dotnet test AlgoTradingSystem.sln
```

### 단위 테스트만 실행

```bash
dotnet test tests/AlgoTrading.UnitTests/AlgoTrading.UnitTests.csproj
```

### 커버리지 리포트 생성

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport
```

---

## 🔐 보안 고려사항

### 민감 정보 관리
- ✅ **User Secrets**: 개발 환경에서 비밀번호, API 키 저장
- ✅ **Azure Key Vault**: 프로덕션 환경에서 비밀 관리
- ❌ **절대 금지**: appsettings.json에 비밀번호 직접 저장

### 인증 및 권한
- **JWT 토큰**: API 인증
- **RBAC**: 역할 기반 접근 제어 (Admin, Trader, Viewer, Developer)
- **2FA**: 2단계 인증 지원

### 데이터 암호화
- **전송 중**: HTTPS/TLS 1.3
- **저장 시**: PostgreSQL Transparent Data Encryption (TDE)

---

## 📊 성능 최적화

### 캐싱 전략
- **L1 Cache**: In-Memory (MemoryCache)
- **L2 Cache**: Redis (분산 캐싱)
- **캐시 무효화**: 도메인 이벤트 기반

### 데이터베이스 최적화
- **인덱싱**: 자주 조회하는 컬럼에 인덱스
- **읽기 전용 복제본**: CQRS Query용
- **파티셔닝**: TimescaleDB로 시계열 데이터 자동 파티션

### 비동기 처리
- **백그라운드 작업**: Hangfire 또는 Quartz.NET
- **메시지 큐**: RabbitMQ로 느슨한 결합

---

## 🐛 트러블슈팅

### 키움 API 연결 실패
```
문제: "OpenAPI 로그인 실패"
해결: 
1. 키움증권 계좌가 활성화되어 있는지 확인
2. OpenAPI 신청이 승인되었는지 확인
3. MockMode를 true로 설정하여 테스트
```

### 데이터베이스 마이그레이션 오류
```
문제: "Migration failed"
해결:
1. PostgreSQL이 실행 중인지 확인
2. ConnectionString이 올바른지 확인
3. 마이그레이션 파일 삭제 후 재생성
   dotnet ef migrations remove
   dotnet ef migrations add InitialCreate
```

### SignalR 연결 끊김
```
문제: "WebSocket connection failed"
해결:
1. 방화벽 설정 확인
2. CORS 설정 확인
3. Long Polling fallback 활성화
```

---

## 📖 추가 문서

### 아키텍처
- [시스템 아키텍처 상세](docs/architecture/system-architecture.md)
- [데이터베이스 스키마](docs/architecture/database-schema.md)
- [API 문서](docs/architecture/api-documentation.md)

### 설계
- [도메인 모델](docs/design/domain-model.md)
- [디자인 패턴 가이드](docs/design/design-patterns.md)
- [DDD Bounded Context](docs/design/ddd-contexts.md)

### 개발
- [개발 환경 설정](docs/development/setup-guide.md)
- [코딩 표준](docs/development/coding-standards.md)
- [Git 워크플로우](docs/development/git-workflow.md)

### 배포
- [배포 가이드](docs/deployment/deployment-guide.md)
- [환경 설정](docs/deployment/configuration.md)

---

## 🤝 기여하기

### 기여 방법

1. **Fork** 본 리포지토리
2. **Feature Branch** 생성 (`git checkout -b feature/AmazingFeature`)
3. **Commit** 변경사항 (`git commit -m 'Add some AmazingFeature'`)
4. **Push** to the Branch (`git push origin feature/AmazingFeature`)
5. **Pull Request** 생성

### 코드 리뷰 프로세스

- 모든 PR은 최소 1명의 승인 필요
- CI/CD 파이프라인 통과 필수
- 코드 커버리지 80% 이상 유지

### 이슈 리포팅

- **버그 리포트**: [Bug Report Template](.github/ISSUE_TEMPLATE/bug_report.md)
- **기능 제안**: [Feature Request Template](.github/ISSUE_TEMPLATE/feature_request.md)

---

## 📜 라이센스

본 프로젝트는 **MIT License** 하에 배포됩니다. 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

```
MIT License

Copyright (c) 2025 AlgoTradingSystem Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction...
```

---

## 👥 팀 및 연락처

### 프로젝트 메인테이너
- **이름**: [Your Name]
- **이메일**: your.email@example.com
- **GitHub**: [@yourusername](https://github.com/yourusername)

### 기여자
프로젝트에 기여해주신 모든 분들께 감사드립니다! 🙏

[![Contributors](https://contrib.rocks/image?repo=yourusername/AlgoTradingSystem)](https://github.com/yourusername/AlgoTradingSystem/graphs/contributors)

### 커뮤니티
- **Discord**: [Join our Discord](https://discord.gg/your-invite)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/AlgoTradingSystem/discussions)

---

## 🌟 스타 히스토리

[![Star History Chart](https://api.star-history.com/svg?repos=yourusername/AlgoTradingSystem&type=Date)](https://star-history.com/#yourusername/AlgoTradingSystem&Date)

---

## 📌 로드맵 및 향후 계획

### 2025 Q1
- [ ] Phase 1 완료 (핵심 거래 기능)
- [ ] Phase 2 시작 (전략 실행)

### 2025 Q2
- [ ] Phase 2 완료
- [ ] Phase 3 시작 (고급 기능)
- [ ] 모바일 앱 개발 시작

### 2025 Q3-Q4
- [ ] Phase 3-4 완료
- [ ] 해외 주식 지원 (미국 주식)
- [ ] 머신러닝 기반 전략 추가

---

## ⚠️ 면책 조항

본 소프트웨어는 **교육 및 연구 목적**으로 제공됩니다. 실제 투자에 사용할 경우 발생하는 모든 손실에 대해 개발자는 책임지지 않습니다. 투자 결정은 본인의 판단과 책임 하에 이루어져야 합니다.

**주의사항:**
- 알고리즘 트레이딩은 높은 리스크를 수반합니다
- 충분한 테스트 없이 실전 투자하지 마세요
- 손실 가능한 범위 내에서만 투자하세요
- 법적 규제를 준수하세요

---

<p align="center">
  Made with ❤️ by AlgoTradingSystem Team
</p>

<p align="center">
  <a href="#-목차">⬆️ Back to Top</a>
</p>
