# 알고리즘 주식 거래 시스템 - 하이브리드 프로젝트 구조

## 📁 전체 구조 개요

```
AlgoTradingSystem/
├── src/                                    # 소스 코드
│   ├── 1.Core/                            # 도메인 레이어 (DDD)
│   ├── 2.Application/                     # 애플리케이션 레이어
│   ├── 3.Infrastructure/                  # 인프라 레이어
│   ├── 4.Presentation/                    # 프레젠테이션 레이어
│   └── 5.Shared/                          # 공유 라이브러리
├── tests/                                  # 테스트 프로젝트
├── docs/                                   # 문서
├── scripts/                                # 스크립트 및 도구
├── deployment/                             # 배포 관련
└── tools/                                  # 개발 도구
```

---

## 🎯 1. Core Layer (도메인 레이어)

### 📂 AlgoTrading.Core (클래스 라이브러리)

```
src/1.Core/AlgoTrading.Core/
├── AlgoTrading.Core.csproj
├── Entities/                               # 엔티티 (Aggregate Root)
│   ├── Trading/
│   │   ├── Order.cs                       # 주문 엔티티
│   │   ├── Position.cs                    # 포지션 엔티티
│   │   ├── Trade.cs                       # 거래 엔티티
│   │   └── Execution.cs                   # 체결 엔티티
│   ├── Strategy/
│   │   ├── TradingStrategy.cs             # 거래 전략
│   │   ├── StrategyRule.cs                # 전략 규칙
│   │   ├── Backtest.cs                    # 백테스트
│   │   └── StrategySignal.cs              # 전략 신호
│   ├── MarketData/
│   │   ├── Stock.cs                       # 종목 정보
│   │   ├── MarketData.cs                  # 시장 데이터
│   │   ├── PriceChart.cs                  # 가격 차트
│   │   └── TechnicalIndicator.cs          # 기술적 지표
│   ├── Portfolio/
│   │   ├── Portfolio.cs                   # 포트폴리오
│   │   ├── Account.cs                     # 계좌
│   │   └── Transaction.cs                 # 거래 내역
│   └── Risk/
│       ├── RiskProfile.cs                 # 리스크 프로필
│       ├── RiskMonitor.cs                 # 리스크 모니터
│       └── RiskAlert.cs                   # 리스크 알림
│
├── ValueObjects/                           # 값 객체
│   ├── Common/
│   │   ├── Money.cs                       # 금액
│   │   ├── Percentage.cs                  # 퍼센트
│   │   ├── DateRange.cs                   # 날짜 범위
│   │   └── TimeFrame.cs                   # 시간프레임
│   ├── Trading/
│   │   ├── OrderType.cs                   # 주문 타입
│   │   ├── OrderStatus.cs                 # 주문 상태
│   │   ├── OrderSide.cs                   # 매수/매도
│   │   ├── Price.cs                       # 가격
│   │   └── Quantity.cs                    # 수량
│   ├── Strategy/
│   │   ├── StrategyType.cs                # 전략 타입
│   │   ├── SignalType.cs                  # 신호 타입
│   │   └── BacktestResult.cs              # 백테스트 결과
│   └── Market/
│       ├── StockCode.cs                   # 종목 코드
│       ├── MarketType.cs                  # 시장 구분
│       └── TradingSession.cs              # 거래 세션
│
├── Enums/                                  # 열거형
│   ├── OrderType.cs
│   ├── OrderStatus.cs
│   ├── OrderSide.cs
│   ├── MarketType.cs
│   ├── StrategyStatus.cs
│   ├── RiskLevel.cs
│   └── AlertPriority.cs
│
├── Interfaces/                             # 인터페이스
│   ├── Repositories/
│   │   ├── IOrderRepository.cs
│   │   ├── IPositionRepository.cs
│   │   ├── IStrategyRepository.cs
│   │   ├── IMarketDataRepository.cs
│   │   ├── IPortfolioRepository.cs
│   │   └── IAccountRepository.cs
│   ├── Services/
│   │   ├── IOrderService.cs
│   │   ├── IStrategyService.cs
│   │   ├── IMarketDataService.cs
│   │   ├── IRiskService.cs
│   │   └── INotificationService.cs
│   └── External/
│       ├── IBrokerAPI.cs                  # 증권사 API
│       ├── IMarketDataProvider.cs         # 시장 데이터 제공자
│       └── INotificationProvider.cs       # 알림 제공자
│
├── DomainEvents/                           # 도메인 이벤트
│   ├── Trading/
│   │   ├── OrderCreatedEvent.cs
│   │   ├── OrderExecutedEvent.cs
│   │   ├── OrderCancelledEvent.cs
│   │   └── PositionOpenedEvent.cs
│   ├── Strategy/
│   │   ├── SignalGeneratedEvent.cs
│   │   ├── StrategyActivatedEvent.cs
│   │   └── BacktestCompletedEvent.cs
│   └── Risk/
│       ├── RiskLimitExceededEvent.cs
│       └── RiskAlertTriggeredEvent.cs
│
├── Exceptions/                             # 도메인 예외
│   ├── OrderException.cs
│   ├── StrategyException.cs
│   ├── MarketDataException.cs
│   ├── InsufficientFundsException.cs
│   └── RiskLimitException.cs
│
└── Specifications/                         # 스펙 패턴
    ├── OrderSpecifications/
    │   ├── ActiveOrdersSpecification.cs
    │   └── PendingOrdersSpecification.cs
    └── StrategySpecifications/
        ├── ProfitableStrategySpecification.cs
        └── ActiveStrategySpecification.cs
```

---

## 🔧 2. Application Layer (애플리케이션 레이어)

### 📂 AlgoTrading.Application (클래스 라이브러리)

```
src/2.Application/AlgoTrading.Application/
├── AlgoTrading.Application.csproj
├── Services/                               # 애플리케이션 서비스
│   ├── Trading/
│   │   ├── OrderService.cs                # 주문 서비스
│   │   ├── OrderExecutionService.cs       # 주문 실행 서비스
│   │   ├── PositionManagementService.cs   # 포지션 관리
│   │   └── TradeHistoryService.cs         # 거래 이력
│   ├── Strategy/
│   │   ├── StrategyService.cs             # 전략 서비스
│   │   ├── SignalGenerationService.cs     # 신호 생성
│   │   ├── BacktestService.cs             # 백테스팅
│   │   └── StrategyOptimizationService.cs # 전략 최적화
│   ├── MarketData/
│   │   ├── MarketDataService.cs           # 시장 데이터 서비스
│   │   ├── RealtimeDataService.cs         # 실시간 데이터
│   │   ├── HistoricalDataService.cs       # 과거 데이터
│   │   └── TechnicalIndicatorService.cs   # 기술적 지표
│   ├── Portfolio/
│   │   ├── PortfolioService.cs            # 포트폴리오 서비스
│   │   ├── AccountService.cs              # 계좌 서비스
│   │   └── PerformanceAnalysisService.cs  # 성과 분석
│   └── Risk/
│       ├── RiskManagementService.cs       # 리스크 관리
│       ├── RiskMonitoringService.cs       # 리스크 모니터링
│       └── RiskAlertService.cs            # 리스크 알림
│
├── Commands/                               # CQRS Commands
│   ├── Trading/
│   │   ├── CreateOrderCommand.cs
│   │   ├── CancelOrderCommand.cs
│   │   ├── ModifyOrderCommand.cs
│   │   └── ClosePositionCommand.cs
│   ├── Strategy/
│   │   ├── CreateStrategyCommand.cs
│   │   ├── ActivateStrategyCommand.cs
│   │   ├── RunBacktestCommand.cs
│   │   └── OptimizeStrategyCommand.cs
│   └── Handlers/
│       ├── CreateOrderCommandHandler.cs
│       ├── CancelOrderCommandHandler.cs
│       └── CreateStrategyCommandHandler.cs
│
├── Queries/                                # CQRS Queries
│   ├── Trading/
│   │   ├── GetOrderByIdQuery.cs
│   │   ├── GetActiveOrdersQuery.cs
│   │   ├── GetPositionsQuery.cs
│   │   └── GetTradeHistoryQuery.cs
│   ├── Strategy/
│   │   ├── GetStrategyByIdQuery.cs
│   │   ├── GetActiveStrategiesQuery.cs
│   │   ├── GetBacktestResultsQuery.cs
│   │   └── GetStrategySignalsQuery.cs
│   ├── Portfolio/
│   │   ├── GetPortfolioQuery.cs
│   │   ├── GetAccountBalanceQuery.cs
│   │   └── GetPerformanceQuery.cs
│   └── Handlers/
│       ├── GetOrderByIdQueryHandler.cs
│       ├── GetActiveOrdersQueryHandler.cs
│       └── GetPortfolioQueryHandler.cs
│
├── DTOs/                                   # Data Transfer Objects
│   ├── Trading/
│   │   ├── OrderDto.cs
│   │   ├── PositionDto.cs
│   │   ├── TradeDto.cs
│   │   └── ExecutionDto.cs
│   ├── Strategy/
│   │   ├── StrategyDto.cs
│   │   ├── SignalDto.cs
│   │   ├── BacktestDto.cs
│   │   └── BacktestResultDto.cs
│   ├── MarketData/
│   │   ├── StockDto.cs
│   │   ├── MarketDataDto.cs
│   │   ├── ChartDto.cs
│   │   └── IndicatorDto.cs
│   └── Portfolio/
│       ├── PortfolioDto.cs
│       ├── AccountDto.cs
│       └── PerformanceDto.cs
│
├── Validators/                             # 유효성 검증 (FluentValidation)
│   ├── OrderValidator.cs
│   ├── StrategyValidator.cs
│   └── BacktestConfigValidator.cs
│
├── Mappers/                                # AutoMapper Profiles
│   ├── TradingProfile.cs
│   ├── StrategyProfile.cs
│   ├── MarketDataProfile.cs
│   └── PortfolioProfile.cs
│
└── Behaviors/                              # MediatR Behaviors
    ├── ValidationBehavior.cs
    ├── LoggingBehavior.cs
    ├── PerformanceBehavior.cs
    └── TransactionBehavior.cs
```

---

## 🔌 3. Infrastructure Layer (인프라 레이어)

### 📂 AlgoTrading.Infrastructure (클래스 라이브러리)

```
src/3.Infrastructure/AlgoTrading.Infrastructure/
├── AlgoTrading.Infrastructure.csproj
├── Persistence/                            # 데이터 영속성
│   ├── AppDbContext.cs                    # EF Core DbContext
│   ├── Configurations/                     # Entity Configurations
│   │   ├── OrderConfiguration.cs
│   │   ├── PositionConfiguration.cs
│   │   ├── StrategyConfiguration.cs
│   │   └── MarketDataConfiguration.cs
│   ├── Repositories/                       # Repository 구현
│   │   ├── OrderRepository.cs
│   │   ├── PositionRepository.cs
│   │   ├── StrategyRepository.cs
│   │   ├── MarketDataRepository.cs
│   │   ├── PortfolioRepository.cs
│   │   └── AccountRepository.cs
│   ├── Migrations/                         # EF Core 마이그레이션
│   │   └── (자동 생성 파일들)
│   └── Seeds/                              # 초기 데이터
│       └── DataSeeder.cs
│
├── ExternalServices/                       # 외부 서비스 통합
│   ├── Kiwoom/                            # 키움 증권 API
│   │   ├── KiwoomAPIService.cs
│   │   ├── KiwoomConnectionManager.cs
│   │   ├── KiwoomOrderService.cs
│   │   ├── KiwoomMarketDataService.cs
│   │   └── KiwoomEventHandlers.cs
│   ├── eBest/                             # eBest 증권 API (선택)
│   │   └── eBestAPIService.cs
│   └── Adapters/                          # Adapter 패턴
│       ├── BrokerAPIAdapter.cs
│       └── MarketDataAdapter.cs
│
├── Caching/                                # 캐싱
│   ├── RedisCacheService.cs
│   ├── MemoryCacheService.cs
│   └── CacheKeys.cs
│
├── MessageBus/                             # 메시지 버스
│   ├── RabbitMQ/
│   │   ├── RabbitMQConnection.cs
│   │   ├── RabbitMQPublisher.cs
│   │   └── RabbitMQConsumer.cs
│   └── EventBus/
│       ├── EventBusService.cs
│       └── EventHandlers/
│           ├── OrderEventHandler.cs
│           └── SignalEventHandler.cs
│
├── BackgroundServices/                     # 백그라운드 서비스
│   ├── MarketDataCollectorService.cs      # 시장 데이터 수집
│   ├── StrategyExecutionService.cs        # 전략 실행
│   ├── RiskMonitoringService.cs           # 리스크 모니터링
│   └── OrderMonitoringService.cs          # 주문 모니터링
│
├── Notifications/                          # 알림 서비스
│   ├── EmailNotificationService.cs
│   ├── SmsNotificationService.cs
│   ├── TelegramNotificationService.cs
│   └── PushNotificationService.cs
│
├── FileStorage/                            # 파일 저장
│   ├── LocalFileStorage.cs
│   ├── AzureBlobStorage.cs
│   └── BackupService.cs
│
├── Logging/                                # 로깅
│   ├── SerilogConfiguration.cs
│   └── CustomLogEnrichers.cs
│
└── Configuration/                          # 설정
    ├── DatabaseConfiguration.cs
    ├── RedisConfiguration.cs
    ├── RabbitMQConfiguration.cs
    └── BrokerAPIConfiguration.cs
```

---

## 🖥️ 4. Presentation Layer (프레젠테이션 레이어)

### 📂 4.1 WPF Desktop Application (메인 트레이딩 클라이언트)

```
src/4.Presentation/AlgoTrading.Desktop/
├── AlgoTrading.Desktop.csproj
├── App.xaml                                # 애플리케이션 진입점
├── App.xaml.cs
├── MainWindow.xaml                         # 메인 윈도우
├── MainWindow.xaml.cs
│
├── Views/                                  # XAML 뷰
│   ├── Trading/
│   │   ├── OrderEntryView.xaml            # 주문 입력
│   │   ├── OrderBookView.xaml             # 호가창
│   │   ├── PositionsView.xaml             # 포지션 현황
│   │   ├── TradeHistoryView.xaml          # 거래 내역
│   │   └── ChartView.xaml                 # 실시간 차트
│   ├── Strategy/
│   │   ├── StrategyListView.xaml          # 전략 목록
│   │   ├── StrategyEditorView.xaml        # 전략 편집기
│   │   ├── BacktestView.xaml              # 백테스팅
│   │   └── StrategyMonitorView.xaml       # 전략 모니터링
│   ├── MarketData/
│   │   ├── WatchListView.xaml             # 관심종목
│   │   ├── MarketOverviewView.xaml        # 시장 현황
│   │   └── StockSearchView.xaml           # 종목 검색
│   ├── Portfolio/
│   │   ├── PortfolioView.xaml             # 포트폴리오
│   │   ├── AccountView.xaml               # 계좌 정보
│   │   └── PerformanceView.xaml           # 성과 분석
│   ├── Risk/
│   │   ├── RiskDashboardView.xaml         # 리스크 대시보드
│   │   └── RiskAlertsView.xaml            # 리스크 알림
│   └── Settings/
│       ├── SettingsView.xaml              # 설정
│       └── ConnectionView.xaml            # API 연결 설정
│
├── ViewModels/                             # MVVM ViewModel
│   ├── Trading/
│   │   ├── OrderEntryViewModel.cs
│   │   ├── OrderBookViewModel.cs
│   │   ├── PositionsViewModel.cs
│   │   └── ChartViewModel.cs
│   ├── Strategy/
│   │   ├── StrategyListViewModel.cs
│   │   ├── StrategyEditorViewModel.cs
│   │   ├── BacktestViewModel.cs
│   │   └── StrategyMonitorViewModel.cs
│   ├── MarketData/
│   │   ├── WatchListViewModel.cs
│   │   └── MarketOverviewViewModel.cs
│   ├── Portfolio/
│   │   ├── PortfolioViewModel.cs
│   │   └── PerformanceViewModel.cs
│   └── MainViewModel.cs
│
├── Controls/                               # 커스텀 컨트롤
│   ├── RealtimeChartControl.xaml          # 실시간 차트 컨트롤
│   ├── OrderBookControl.xaml              # 호가창 컨트롤
│   ├── TickerControl.xaml                 # 티커 컨트롤
│   └── CandlestickChartControl.xaml       # 캔들차트 컨트롤
│
├── Converters/                             # Value Converters
│   ├── PriceColorConverter.cs             # 가격 색상 변환
│   ├── PercentageConverter.cs             # 퍼센트 변환
│   ├── BoolToVisibilityConverter.cs       # Bool to Visibility
│   └── OrderSideToColorConverter.cs       # 매수/매도 색상
│
├── Behaviors/                              # Attached Behaviors
│   ├── DataGridBehavior.cs
│   └── ChartBehavior.cs
│
├── Resources/                              # 리소스
│   ├── Styles/
│   │   ├── Colors.xaml                    # 색상 정의
│   │   ├── Buttons.xaml                   # 버튼 스타일
│   │   ├── DataGrids.xaml                 # DataGrid 스타일
│   │   └── Charts.xaml                    # 차트 스타일
│   ├── Images/                            # 이미지
│   │   └── Icons/
│   └── Fonts/                             # 폰트
│
├── Services/                               # WPF 전용 서비스
│   ├── DialogService.cs                   # 다이얼로그 서비스
│   ├── WindowService.cs                   # 윈도우 관리
│   └── ClipboardService.cs                # 클립보드
│
└── Helpers/                                # 헬퍼 클래스
    ├── RelayCommand.cs                    # Command 구현
    ├── ViewModelBase.cs                   # ViewModel 베이스
    └── NotifyPropertyChanged.cs           # INotifyPropertyChanged
```

### 📂 4.2 Blazor Web Application (원격 모니터링)

```
src/4.Presentation/AlgoTrading.Web/
├── AlgoTrading.Web.csproj
├── Program.cs                              # 진입점
├── App.razor                               # 루트 컴포넌트
├── _Imports.razor                          # 전역 using
│
├── Pages/                                  # Razor 페이지
│   ├── Index.razor                        # 홈 대시보드
│   ├── Trading/
│   │   ├── Orders.razor                   # 주문 관리
│   │   ├── Positions.razor                # 포지션 현황
│   │   └── History.razor                  # 거래 내역
│   ├── Strategy/
│   │   ├── StrategyList.razor             # 전략 목록
│   │   ├── StrategyDetails.razor          # 전략 상세
│   │   └── Backtest.razor                 # 백테스팅
│   ├── Portfolio/
│   │   ├── Dashboard.razor                # 포트폴리오 대시보드
│   │   ├── Performance.razor              # 성과 분석
│   │   └── Reports.razor                  # 리포트
│   ├── Risk/
│   │   └── RiskMonitor.razor              # 리스크 모니터
│   ├── Settings/
│   │   └── Settings.razor                 # 설정
│   ├── Login.razor                        # 로그인
│   └── Error.razor                        # 에러 페이지
│
├── Components/                             # Razor 컴포넌트
│   ├── Trading/
│   │   ├── OrderCard.razor
│   │   ├── PositionCard.razor
│   │   └── PriceTickerComponent.razor
│   ├── Strategy/
│   │   ├── StrategyCard.razor
│   │   └── SignalIndicator.razor
│   ├── Portfolio/
│   │   ├── PortfolioSummary.razor
│   │   ├── PerformanceChart.razor
│   │   └── AssetAllocation.razor
│   ├── Charts/
│   │   ├── LineChartComponent.razor
│   │   ├── BarChartComponent.razor
│   │   └── PieChartComponent.razor
│   └── Common/
│       ├── LoadingSpinner.razor
│       ├── AlertComponent.razor
│       └── ConfirmDialog.razor
│
├── Shared/                                 # 공유 컴포넌트
│   ├── MainLayout.razor                   # 메인 레이아웃
│   ├── NavMenu.razor                      # 네비게이션 메뉴
│   ├── TopBar.razor                       # 상단 바
│   └── Footer.razor                       # 푸터
│
├── Services/                               # Blazor 서비스
│   ├── StateManagement/
│   │   ├── AppState.cs                    # 애플리케이션 상태
│   │   └── PortfolioState.cs              # 포트폴리오 상태
│   └── SignalRService.cs                  # SignalR 클라이언트
│
├── Hubs/                                   # SignalR Hubs
│   ├── TradingHub.cs                      # 거래 허브
│   ├── MarketDataHub.cs                   # 시장 데이터 허브
│   └── NotificationHub.cs                 # 알림 허브
│
├── wwwroot/                                # 정적 파일
│   ├── css/
│   │   ├── app.css                        # 메인 스타일
│   │   ├── bootstrap/                     # Bootstrap
│   │   └── custom/
│   │       ├── trading.css
│   │       └── dashboard.css
│   ├── js/
│   │   ├── app.js
│   │   ├── chart.js                       # 차트 라이브러리
│   │   └── signalr.js                     # SignalR 클라이언트
│   ├── images/
│   └── favicon.ico
│
└── appsettings.json                        # 설정 파일
```

### 📂 4.3 Web API (공통 백엔드)

```
src/4.Presentation/AlgoTrading.API/
├── AlgoTrading.API.csproj
├── Program.cs                              # 진입점
├── Controllers/                            # API 컨트롤러
│   ├── Trading/
│   │   ├── OrdersController.cs            # 주문 API
│   │   ├── PositionsController.cs         # 포지션 API
│   │   └── TradesController.cs            # 거래 API
│   ├── Strategy/
│   │   ├── StrategiesController.cs        # 전략 API
│   │   ├── BacktestsController.cs         # 백테스트 API
│   │   └── SignalsController.cs           # 신호 API
│   ├── MarketData/
│   │   ├── StocksController.cs            # 종목 API
│   │   ├── MarketDataController.cs        # 시장 데이터 API
│   │   └── ChartsController.cs            # 차트 API
│   ├── Portfolio/
│   │   ├── PortfoliosController.cs        # 포트폴리오 API
│   │   ├── AccountsController.cs          # 계좌 API
│   │   └── PerformanceController.cs       # 성과 API
│   └── Auth/
│       └── AuthenticationController.cs     # 인증 API
│
├── Middleware/                             # 미들웨어
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   ├── RateLimitingMiddleware.cs
│   └── AuthenticationMiddleware.cs
│
├── Filters/                                # 필터
│   ├── ValidationFilter.cs
│   └── AuthorizationFilter.cs
│
├── Hubs/                                   # SignalR Hubs (API용)
│   ├── TradingHub.cs
│   └── MarketDataHub.cs
│
└── Extensions/                             # 확장 메서드
    ├── ServiceCollectionExtensions.cs
    └── ApplicationBuilderExtensions.cs
```

---

## 🔄 5. Shared Layer (공유 레이어)

```
src/5.Shared/
├── AlgoTrading.Shared/
│   ├── AlgoTrading.Shared.csproj
│   ├── Constants/                          # 상수
│   │   ├── AppConstants.cs
│   │   ├── ApiRoutes.cs
│   │   └── ErrorCodes.cs
│   ├── Extensions/                         # 확장 메서드
│   │   ├── DateTimeExtensions.cs
│   │   ├── DecimalExtensions.cs
│   │   └── CollectionExtensions.cs
│   ├── Helpers/                            # 헬퍼 클래스
│   │   ├── CryptographyHelper.cs
│   │   ├── JsonHelper.cs
│   │   └── ValidationHelper.cs
│   └── Results/                            # Result 패턴
│       ├── Result.cs
│       ├── Result{T}.cs
│       └── Error.cs
```

---

## 🧪 6. Tests Layer (테스트 레이어)

```
tests/
├── AlgoTrading.UnitTests/
│   ├── Core/
│   │   ├── Entities/
│   │   │   ├── OrderTests.cs
│   │   │   ├── StrategyTests.cs
│   │   │   └── PositionTests.cs
│   │   └── ValueObjects/
│   │       └── MoneyTests.cs
│   ├── Application/
│   │   ├── Services/
│   │   │   ├── OrderServiceTests.cs
│   │   │   └── StrategyServiceTests.cs
│   │   └── Handlers/
│   │       └── CreateOrderCommandHandlerTests.cs
│   └── Infrastructure/
│       └── Repositories/
│           └── OrderRepositoryTests.cs
│
├── AlgoTrading.IntegrationTests/
│   ├── API/
│   │   ├── OrdersControllerTests.cs
│   │   └── StrategiesControllerTests.cs
│   ├── Database/
│   │   └── RepositoryIntegrationTests.cs
│   └── ExternalServices/
│       └── KiwoomAPITests.cs
│
└── AlgoTrading.E2ETests/
    ├── TradingFlowTests.cs
    └── StrategyExecutionTests.cs
```

---

## 📚 7. Documentation Layer (문서 레이어)

```
docs/
├── architecture/
│   ├── system-architecture.md
│   ├── database-schema.md
│   └── api-documentation.md
├── design/
│   ├── domain-model.md
│   ├── design-patterns.md
│   └── ddd-contexts.md
├── development/
│   ├── setup-guide.md
│   ├── coding-standards.md
│   └── git-workflow.md
├── deployment/
│   ├── deployment-guide.md
│   └── configuration.md
└── user-manual/
    ├── wpf-user-guide.md
    └── web-user-guide.md
```

---

## 🛠️ 8. Scripts & Tools Layer

```
scripts/
├── database/
│   ├── create-schema.sql
│   ├── seed-data.sql
│   └── migrations/
├── deployment/
│   ├── deploy-api.ps1
│   ├── deploy-web.ps1
│   └── deploy-desktop.ps1
└── development/
    ├── setup-dev-env.ps1
    └── generate-certificates.ps1

tools/
├── code-generators/
│   └── entity-generator.ps1
└── data-migration/
    └── migrate-legacy-data.ps1
```

---

## 🚀 9. Deployment Layer

```
deployment/
├── docker/
│   ├── Dockerfile.api
│   ├── Dockerfile.web
│   ├── docker-compose.yml
│   └── docker-compose.override.yml
├── kubernetes/
│   ├── api-deployment.yaml
│   ├── web-deployment.yaml
│   ├── redis-deployment.yaml
│   └── rabbitmq-deployment.yaml
├── azure/
│   ├── arm-templates/
│   └── pipeline.yml
└── ci-cd/
    ├── .github/
    │   └── workflows/
    │       ├── build.yml
    │       ├── test.yml
    │       └── deploy.yml
    └── azure-pipelines.yml
```

---

## 📦 10. Solution 파일 구조

```
AlgoTradingSystem.sln                       # 메인 솔루션

# Solution Folders
├── 📁 1.Core
│   └── AlgoTrading.Core.csproj
├── 📁 2.Application
│   └── AlgoTrading.Application.csproj
├── 📁 3.Infrastructure
│   └── AlgoTrading.Infrastructure.csproj
├── 📁 4.Presentation
│   ├── AlgoTrading.Desktop.csproj         # WPF
│   ├── AlgoTrading.Web.csproj             # Blazor
│   └── AlgoTrading.API.csproj             # Web API
├── 📁 5.Shared
│   └── AlgoTrading.Shared.csproj
└── 📁 6.Tests
    ├── AlgoTrading.UnitTests.csproj
    ├── AlgoTrading.IntegrationTests.csproj
    └── AlgoTrading.E2ETests.csproj
```

---

## 🔗 프로젝트 간 참조 관계

```
AlgoTrading.Desktop (WPF)
├── → AlgoTrading.Application
├── → AlgoTrading.Infrastructure
└── → AlgoTrading.Shared

AlgoTrading.Web (Blazor)
├── → AlgoTrading.Application
└── → AlgoTrading.Shared

AlgoTrading.API
├── → AlgoTrading.Application
├── → AlgoTrading.Infrastructure
└── → AlgoTrading.Shared

AlgoTrading.Application
├── → AlgoTrading.Core
└── → AlgoTrading.Shared

AlgoTrading.Infrastructure
├── → AlgoTrading.Core
├── → AlgoTrading.Application
└── → AlgoTrading.Shared

AlgoTrading.Core
└── (독립적, 참조 없음)
```

---

## 🎯 개발 우선순위별 폴더

### Phase 1 (핵심 기능)
```
✅ AlgoTrading.Core (전체)
✅ AlgoTrading.Application/Services/Trading
✅ AlgoTrading.Infrastructure/ExternalServices/Kiwoom
✅ AlgoTrading.Infrastructure/Persistence
✅ AlgoTrading.API/Controllers/Trading
✅ AlgoTrading.Desktop/Views/Trading
```

### Phase 2 (전략 실행)
```
✅ AlgoTrading.Application/Services/Strategy
✅ AlgoTrading.Desktop/Views/Strategy
✅ AlgoTrading.Web (전체 시작)
```

### Phase 3 (고급 기능)
```
✅ AlgoTrading.Application/Services/Strategy/Optimization
✅ AlgoTrading.Infrastructure/BackgroundServices
```

### Phase 4 (확장)
```
✅ AlgoTrading.Infrastructure/Notifications
✅ AlgoTrading.Web/Pages (전체 완성)
```

---

## 📝 주요 특징

### ✨ Clean Architecture
- 의존성 역전 원칙
- 레이어 간 명확한 분리
- 테스트 용이성

### ✨ DDD (Domain-Driven Design)
- Bounded Context 분리
- Aggregate 패턴
- Domain Events

### ✨ CQRS (Command Query Responsibility Segregation)
- 읽기/쓰기 분리
- 성능 최적화
- 확장성

### ✨ 하이브리드 UI
- WPF: 고성능 트레이딩
- Blazor: 원격 모니터링
- 공통 백엔드 공유

---

이 구조는 확장 가능하고 유지보수가 용이하며, 엔터프라이즈급 품질을 보장합니다! 🚀
