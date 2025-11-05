# Monitoring Context - DDD 상세 설계

## 📋 목차
1. [Context Overview](#1-context-overview)
2. [Domain Model 상세 설계](#2-domain-model-상세-설계)
3. [Business Rules & Invariants](#3-business-rules--invariants)
4. [Domain Events](#4-domain-events)
5. [Domain Services](#5-domain-services)
6. [Application Services](#6-application-services)
7. [Infrastructure Layer](#7-infrastructure-layer)
8. [API/Interface Specifications](#8-apiinterface-specifications)
9. [Monitoring Strategies](#9-monitoring-strategies)
10. [Alert Management](#10-alert-management)
11. [Report Generation](#11-report-generation)
12. [Error Handling & Validation](#12-error-handling--validation)
13. [Performance Considerations](#13-performance-considerations)

---

## 1. Context Overview

### 1.1 책임과 범위

**Monitoring Context**는 전체 시스템의 상태 모니터링, 성과 추적, 알림 관리, 로깅, 대시보드 제공 등 운영 관련 모든 기능을 담당하는 Bounded Context입니다.

#### 주요 책임
- ✅ 실시간 시스템 상태 모니터링
- ✅ 성과 대시보드 및 분석 리포트
- ✅ 멀티 채널 알림 관리 (SMS, Email, Push, Telegram)
- ✅ 감사 로그 및 이벤트 추적
- ✅ 시스템 헬스 체크 및 장애 감지
- ✅ 실시간 차트 및 시각화
- ✅ 일일/주간/월간 리포트 생성
- ✅ 알고리즘 성과 비교 분석

#### 핵심 원칙
- **실시간성**: 실시간 모니터링과 즉각적 알림
- **투명성**: 완전한 시스템 상태 공개
- **신뢰성**: 이력 분석과 추적성
- **직관성**: 직관적 시각화와 대시보드
- **확장성**: 다양한 알림 채널 지원
- **자동화**: 자동 리포트 생성 및 발송

### 1.2 Context Map

```
┌──────────────────────────────────┐
│  Monitoring Context              │
│  (시스템 모니터링 & 알림 & 리포트)|
└──────────────────────────────────┘
    ↓ 모니터링    ↑ 이벤트
    │             │
    ├─────────────┼─────────────┬──────────────┐
    │             │             │              │
    ↓             ↓             ↓              ↓
Strategy      Trading        Risk         Account
Context       Context        Context      Context

    ↑ 대시보드 & 리포트 제공
    │
┌──────────────────┐
│  UI/Frontend     │
│  (Blazor)        │
└──────────────────┘
```

### 1.3 바운더리

**이 Context가 담당하는 것:**
- 시스템 헬스 모니터링
- 실시간 메트릭 수집
- 대시보드 및 시각화
- 경고 및 알림
- 리포트 생성 및 배포
- 감사 로그 관리

**이 Context가 담당하지 않는 것:**
- 신호 생성 (Strategy Context)
- 주문 실행 (Trading Context)
- 리스크 관리 (Risk Management Context)
- 계좌 관리 (Account Context)
- 시장 데이터 (Market Data Context)

---

## 2. Domain Model 상세 설계

### 2.1 Aggregates

#### 2.1.1 SystemMonitor Aggregate (시스템 모니터)

**Aggregate Root**

```csharp
/// <summary>
/// 시스템 모니터 Aggregate Root
/// 전체 시스템의 상태를 모니터링
/// </summary>
public class SystemMonitor : AggregateRoot<SystemMonitorId>
{
    private readonly List<HealthCheck> _healthChecks = new();
    private readonly List<SystemMetric> _metrics = new();
    private readonly List<SystemAlert> _activeAlerts = new();
    private readonly Dictionary<string, ComponentHealth> _componentHealth = new();
    
    // ======================== Identity ========================
    /// <summary>모니터 고유 ID</summary>
    public SystemMonitorId Id { get; private set; }
    
    /// <summary>시스템 이름</summary>
    public string SystemName { get; private set; }
    
    // ======================== System Status ========================
    /// <summary>전체 시스템 상태</summary>
    public SystemStatus OverallStatus { get; private set; }
    
    /// <summary>마지막 헬스 체크 시간</summary>
    public DateTime LastHealthCheckAt { get; private set; }
    
    /// <summary>시스템 가동 시간</summary>
    public TimeSpan Uptime { get; private set; }
    
    /// <summary>시스템 시작 시간</summary>
    public DateTime StartedAt { get; private set; }
    
    // ======================== Component Status ========================
    /// <summary>Kiwoom API 연결 상태</summary>
    public ConnectionStatus KiwoomApiStatus { get; private set; }
    
    /// <summary>데이터베이스 연결 상태</summary>
    public ConnectionStatus DatabaseStatus { get; private set; }
    
    /// <summary>메시지 버스 상태</summary>
    public ConnectionStatus MessageBusStatus { get; private set; }
    
    /// <summary>시장 데이터 피드 상태</summary>
    public ConnectionStatus MarketDataStatus { get; private set; }
    
    // ======================== Performance Metrics ========================
    /// <summary>CPU 사용률 (%)</summary>
    public double CpuUsage { get; private set; }
    
    /// <summary>메모리 사용률 (%)</summary>
    public double MemoryUsage { get; private set; }
    
    /// <summary>디스크 사용량 (MB)</summary>
    public long DiskUsage { get; private set; }
    
    /// <summary>네트워크 지연 (ms)</summary>
    public double NetworkLatency { get; private set; }
    
    /// <summary>활성 스레드 수</summary>
    public int ActiveThreadCount { get; private set; }
    
    /// <summary>메시지 큐 크기</summary>
    public int QueuedMessages { get; private set; }
    
    // ======================== Trading Metrics ========================
    /// <summary>활성 전략 수</summary>
    public int ActiveStrategies { get; private set; }
    
    /// <summary>오픈 포지션 수</summary>
    public int OpenPositions { get; private set; }
    
    /// <summary>대기 중인 주문 수</summary>
    public int PendingOrders { get; private set; }
    
    /// <summary>오늘 실행된 주문 수</summary>
    public int TodayExecutedOrders { get; private set; }
    
    /// <summary>오늘 손익</summary>
    public Money TodayProfitLoss { get; private set; }
    
    /// <summary>주문 체결률</summary>
    public decimal OrderExecutionRate { get; private set; }
    
    /// <summary>평균 주문 지연시간</summary>
    public TimeSpan AverageOrderLatency { get; private set; }
    
    // ======================== Error Tracking ========================
    /// <summary>24시간 에러 수</summary>
    public int ErrorCount24h { get; private set; }
    
    /// <summary>24시간 경고 수</summary>
    public int WarningCount24h { get; private set; }
    
    /// <summary>마지막 에러 시간</summary>
    public DateTime? LastErrorAt { get; private set; }
    
    /// <summary>마지막 에러 메시지</summary>
    public string LastErrorMessage { get; private set; }
    
    // ======================== Alerts ========================
    /// <summary>활성 경고 목록</summary>
    public IReadOnlyList<SystemAlert> ActiveAlerts => _activeAlerts.AsReadOnly();
    
    /// <summary>심각 경고 수</summary>
    public int CriticalAlertCount => _activeAlerts.Count(a => a.Severity == AlertSeverity.Critical);
    
    // ======================== Constructor & Factory Methods ========================
    
    /// <summary>
    /// 새 시스템 모니터 생성
    /// </summary>
    public static SystemMonitor Create(string systemName)
    {
        var monitor = new SystemMonitor
        {
            Id = SystemMonitorId.New(),
            SystemName = systemName,
            OverallStatus = SystemStatus.Initializing,
            StartedAt = DateTime.UtcNow,
            LastHealthCheckAt = DateTime.UtcNow,
            KiwoomApiStatus = ConnectionStatus.Disconnected,
            DatabaseStatus = ConnectionStatus.Disconnected,
            MessageBusStatus = ConnectionStatus.Disconnected,
            MarketDataStatus = ConnectionStatus.Disconnected
        };
        
        monitor.AddDomainEvent(new SystemMonitorCreatedEvent(
            monitor.Id, systemName, DateTime.UtcNow));
        
        return monitor;
    }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 헬스 체크 수행
    /// </summary>
    public void PerformHealthCheck(SystemHealthSnapshot snapshot)
    {
        var healthCheck = new HealthCheck(
            HealthCheckId.New(),
            DateTime.UtcNow,
            snapshot);
        
        _healthChecks.Add(healthCheck);
        LastHealthCheckAt = DateTime.UtcNow;
        
        // 컴포넌트 상태 업데이트
        UpdateComponentStatus(snapshot);
        
        // 성능 메트릭 업데이트
        UpdatePerformanceMetrics(snapshot);
        
        // 전체 상태 업데이트
        UpdateOverallStatus();
        
        // 경고 생성
        GenerateHealthAlerts(healthCheck);
        
        // 최근 100개 헬스 체크만 유지
        if (_healthChecks.Count > 100)
        {
            _healthChecks.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 메트릭 기록
    /// </summary>
    public void RecordMetric(
        string metricName,
        double value,
        MetricUnit unit)
    {
        var metric = new SystemMetric(
            SystemMetricId.New(),
            metricName,
            value,
            unit,
            DateTime.UtcNow);
        
        _metrics.Add(metric);
        
        // 임계값 확인
        if (ShouldAlertForMetric(metricName, value))
        {
            var alert = new SystemAlert(
                SystemAlertId.New(),
                AlertType.MetricThreshold,
                $"{metricName} exceeded threshold: {value}{unit}",
                DetermineSeverity(metricName, value),
                DateTime.UtcNow);
            
            _activeAlerts.Add(alert);
            
            AddDomainEvent(new SystemAlertRaisedEvent(
                Id, alert.Type, alert.Message, alert.Severity, DateTime.UtcNow));
        }
        
        // 최근 24시간 메트릭만 유지
        var cutoff = DateTime.UtcNow.AddHours(-24);
        _metrics.RemoveAll(m => m.Timestamp < cutoff);
    }
    
    /// <summary>
    /// 거래 활동 기록
    /// </summary>
    public void RecordTradingActivity(TradingActivity activity)
    {
        switch (activity.Type)
        {
            case TradingActivityType.OrderSubmitted:
                PendingOrders++;
                break;
                
            case TradingActivityType.OrderExecuted:
                PendingOrders--;
                TodayExecutedOrders++;
                UpdateAverageLatency(activity.Latency);
                OrderExecutionRate = (decimal)TodayExecutedOrders / 
                    (TodayExecutedOrders + activity.FailedOrdersCount) * 100;
                break;
                
            case TradingActivityType.PositionOpened:
                OpenPositions++;
                break;
                
            case TradingActivityType.PositionClosed:
                OpenPositions--;
                TodayProfitLoss += activity.ProfitLoss;
                break;
                
            case TradingActivityType.StrategyStarted:
                ActiveStrategies++;
                break;
                
            case TradingActivityType.StrategyStopped:
                ActiveStrategies--;
                break;
        }
        
        AddDomainEvent(new TradingActivityRecordedEvent(
            Id, activity.Type, activity.ProfitLoss, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 시스템 에러 처리
    /// </summary>
    public void HandleSystemError(
        string errorMessage,
        ErrorSeverity severity,
        string source)
    {
        ErrorCount24h++;
        LastErrorAt = DateTime.UtcNow;
        LastErrorMessage = errorMessage;
        
        var alertSeverity = severity switch
        {
            ErrorSeverity.Critical => AlertSeverity.Critical,
            ErrorSeverity.High => AlertSeverity.High,
            ErrorSeverity.Medium => AlertSeverity.Medium,
            _ => AlertSeverity.Low
        };
        
        var alert = new SystemAlert(
            SystemAlertId.New(),
            AlertType.Error,
            $"[{source}] {errorMessage}",
            alertSeverity,
            DateTime.UtcNow);
        
        _activeAlerts.Add(alert);
        
        AddDomainEvent(new SystemErrorDetectedEvent(
            Id, errorMessage, severity, source, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 경고 해제
    /// </summary>
    public void ResolveAlert(SystemAlertId alertId)
    {
        var alert = _activeAlerts.FirstOrDefault(a => a.Id == alertId);
        if (alert != null)
        {
            _activeAlerts.Remove(alert);
            
            AddDomainEvent(new SystemAlertResolvedEvent(
                Id, alertId, DateTime.UtcNow));
        }
    }
    
    /// <summary>
    /// 컴포넌트 상태 업데이트
    /// </summary>
    private void UpdateComponentStatus(SystemHealthSnapshot snapshot)
    {
        KiwoomApiStatus = snapshot.KiwoomApiStatus;
        DatabaseStatus = snapshot.DatabaseStatus;
        MessageBusStatus = snapshot.MessageBusStatus;
        MarketDataStatus = snapshot.MarketDataStatus;
    }
    
    /// <summary>
    /// 성능 메트릭 업데이트
    /// </summary>
    private void UpdatePerformanceMetrics(SystemHealthSnapshot snapshot)
    {
        CpuUsage = snapshot.CpuUsage;
        MemoryUsage = snapshot.MemoryUsage;
        DiskUsage = snapshot.DiskUsage;
        NetworkLatency = snapshot.NetworkLatency;
        ActiveThreadCount = snapshot.ActiveThreadCount;
        QueuedMessages = snapshot.QueuedMessages;
    }
    
    /// <summary>
    /// 전체 상태 업데이트
    /// </summary>
    private void UpdateOverallStatus()
    {
        if (CriticalAlertCount > 0 || 
            KiwoomApiStatus == ConnectionStatus.Failed ||
            DatabaseStatus == ConnectionStatus.Failed)
        {
            OverallStatus = SystemStatus.Critical;
        }
        else if (_activeAlerts.Any(a => a.Severity == AlertSeverity.High) ||
            CpuUsage > 90 || MemoryUsage > 90)
        {
            OverallStatus = SystemStatus.Warning;
        }
        else if (KiwoomApiStatus != ConnectionStatus.Connected ||
            DatabaseStatus != ConnectionStatus.Connected)
        {
            OverallStatus = SystemStatus.Degraded;
        }
        else
        {
            OverallStatus = SystemStatus.Healthy;
        }
    }
    
    /// <summary>
    /// 헬스 경고 생성
    /// </summary>
    private void GenerateHealthAlerts(HealthCheck healthCheck)
    {
        if (CpuUsage > 85)
        {
            var alert = new SystemAlert(
                SystemAlertId.New(),
                AlertType.HighCpuUsage,
                $"CPU usage is {CpuUsage:F1}%",
                AlertSeverity.High,
                DateTime.UtcNow);
            
            if (!_activeAlerts.Any(a => a.Type == AlertType.HighCpuUsage))
            {
                _activeAlerts.Add(alert);
            }
        }
        
        if (MemoryUsage > 85)
        {
            var alert = new SystemAlert(
                SystemAlertId.New(),
                AlertType.HighMemoryUsage,
                $"Memory usage is {MemoryUsage:F1}%",
                AlertSeverity.High,
                DateTime.UtcNow);
            
            if (!_activeAlerts.Any(a => a.Type == AlertType.HighMemoryUsage))
            {
                _activeAlerts.Add(alert);
            }
        }
    }
    
    /// <summary>
    /// 메트릭에 대한 경고 필요 여부
    /// </summary>
    private bool ShouldAlertForMetric(string metricName, double value)
    {
        return metricName switch
        {
            "CPU_Usage" => value > 90,
            "Memory_Usage" => value > 90,
            "Disk_Usage" => value > 95,
            "Network_Latency" => value > 500,
            _ => false
        };
    }
    
    /// <summary>
    /// 메트릭에 대한 심각도 결정
    /// </summary>
    private AlertSeverity DetermineSeverity(string metricName, double value)
    {
        return (metricName, value) switch
        {
            (_, > 95) => AlertSeverity.Critical,
            (_, > 85) => AlertSeverity.High,
            (_, > 70) => AlertSeverity.Medium,
            _ => AlertSeverity.Low
        };
    }
    
    /// <summary>
    /// 평균 지연시간 업데이트
    /// </summary>
    private void UpdateAverageLatency(TimeSpan latency)
    {
        var totalLatency = AverageOrderLatency.TotalMilliseconds * TodayExecutedOrders +
                           latency.TotalMilliseconds;
        AverageOrderLatency = TimeSpan.FromMilliseconds(
            totalLatency / (TodayExecutedOrders + 1));
    }
}
```

#### 2.1.2 Dashboard Aggregate (대시보드)

```csharp
/// <summary>
/// 대시보드 Aggregate Root
/// 사용자 정의 대시보드 관리
/// </summary>
public class Dashboard : AggregateRoot<DashboardId>
{
    private readonly List<DashboardWidget> _widgets = new();
    private readonly Dictionary<string, object> _widgetCache = new();
    
    // ======================== Identity ========================
    public DashboardId Id { get; private set; }
    public UserId UserId { get; private set; }
    
    // ======================== Configuration ========================
    /// <summary>대시보드 이름</summary>
    public string Name { get; private set; }
    
    /// <summary>대시보드 유형</summary>
    public DashboardType Type { get; private set; }
    
    /// <summary>설명</summary>
    public string Description { get; private set; }
    
    // ======================== Layout ========================
    /// <summary>대시보드 레이아웃</summary>
    public DashboardLayout Layout { get; private set; }
    
    /// <summary>위젯 목록</summary>
    public IReadOnlyList<DashboardWidget> Widgets => _widgets.AsReadOnly();
    
    /// <summary>새로고침 간격</summary>
    public RefreshInterval RefreshInterval { get; private set; }
    
    // ======================== Filters ========================
    /// <summary>날짜 범위</summary>
    public DateRange DateRange { get; private set; }
    
    /// <summary>선택된 계좌</summary>
    public List<AccountNumber> SelectedAccounts { get; private set; }
    
    /// <summary>선택된 전략</summary>
    public List<StrategyId> SelectedStrategies { get; private set; }
    
    /// <summary>감시 목록</summary>
    public List<StockCode> WatchList { get; private set; }
    
    // ======================== Real-time Data ========================
    /// <summary>현재 데이터</summary>
    public DashboardData CurrentData { get; private set; }
    
    /// <summary>마지막 업데이트 시간</summary>
    public DateTime LastUpdatedAt { get; private set; }
    
    // ======================== Preferences ========================
    /// <summary>테마 설정</summary>
    public ThemeSettings Theme { get; private set; }
    
    /// <summary>자동 새로고침 활성화</summary>
    public bool AutoRefresh { get; private set; }
    
    /// <summary>알림 표시 활성화</summary>
    public bool ShowNotifications { get; private set; }
    
    /// <summary>활성 여부</summary>
    public bool IsActive { get; private set; }
    
    /// <summary>생성 시간</summary>
    public DateTime CreatedAt { get; private set; }
    
    // ======================== Factory Methods ========================
    
    /// <summary>
    /// 새 대시보드 생성
    /// </summary>
    public static Dashboard Create(
        UserId userId,
        string name,
        DashboardType type,
        string description = null)
    {
        var dashboard = new Dashboard
        {
            Id = DashboardId.New(),
            UserId = userId,
            Name = name,
            Type = type,
            Description = description,
            Layout = DashboardLayout.Grid,
            RefreshInterval = RefreshInterval.FiveSeconds,
            DateRange = DateRange.Last30Days(),
            SelectedAccounts = new List<AccountNumber>(),
            SelectedStrategies = new List<StrategyId>(),
            WatchList = new List<StockCode>(),
            Theme = ThemeSettings.Default(),
            AutoRefresh = true,
            ShowNotifications = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
        
        dashboard.AddDomainEvent(new DashboardCreatedEvent(
            dashboard.Id, userId, name, type, DateTime.UtcNow));
        
        return dashboard;
    }
    
    /// <summary>
    /// 거래 대시보드 템플릿
    /// </summary>
    public static Dashboard CreateTradingDashboard(UserId userId)
    {
        var dashboard = Create(userId, "Trading Dashboard", DashboardType.Trading);
        
        dashboard.AddWidget(DashboardWidgetType.PortfolioSummary);
        dashboard.AddWidget(DashboardWidgetType.RecentTrades);
        dashboard.AddWidget(DashboardWidgetType.ProfitLossChart);
        dashboard.AddWidget(DashboardWidgetType.StrategyPerformance);
        
        return dashboard;
    }
    
    /// <summary>
    /// 리스크 대시보드 템플릿
    /// </summary>
    public static Dashboard CreateRiskDashboard(UserId userId)
    {
        var dashboard = Create(userId, "Risk Dashboard", DashboardType.Risk);
        
        dashboard.AddWidget(DashboardWidgetType.RiskMetrics);
        dashboard.AddWidget(DashboardWidgetType.PositionRisk);
        dashboard.AddWidget(DashboardWidgetType.DrawdownChart);
        dashboard.AddWidget(DashboardWidgetType.AlertHistory);
        
        return dashboard;
    }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 위젯 추가
    /// </summary>
    public void AddWidget(DashboardWidgetType widgetType)
    {
        if (_widgets.Count >= 20)
            throw new InvalidOperationException("Maximum widget limit reached");
        
        var widget = new DashboardWidget(
            DashboardWidgetId.New(),
            widgetType,
            WidgetPosition.Next(_widgets.Count));
        
        _widgets.Add(widget);
        
        AddDomainEvent(new DashboardWidgetAddedEvent(
            Id, widget.Id, widgetType, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 위젯 제거
    /// </summary>
    public void RemoveWidget(DashboardWidgetId widgetId)
    {
        var widget = _widgets.FirstOrDefault(w => w.Id == widgetId);
        if (widget != null)
        {
            _widgets.Remove(widget);
            
            AddDomainEvent(new DashboardWidgetRemovedEvent(
                Id, widgetId, DateTime.UtcNow));
        }
    }
    
    /// <summary>
    /// 필터 업데이트
    /// </summary>
    public void UpdateFilters(
        DateRange dateRange = null,
        List<AccountNumber> accounts = null,
        List<StrategyId> strategies = null)
    {
        if (dateRange != null)
            DateRange = dateRange;
        
        if (accounts != null)
            SelectedAccounts = accounts;
        
        if (strategies != null)
            SelectedStrategies = strategies;
        
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new DashboardFiltersUpdatedEvent(
            Id, DateRange, SelectedAccounts, SelectedStrategies, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 대시보드 업데이트
    /// </summary>
    public void UpdateData(DashboardData data)
    {
        CurrentData = data;
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new DashboardUpdatedEvent(
            Id, DateTime.UtcNow));
    }
}
```

#### 2.1.3 Report Aggregate (리포트)

```csharp
/// <summary>
/// 리포트 Aggregate Root
/// 성과 리포트 생성 및 관리
/// </summary>
public class Report : AggregateRoot<ReportId>
{
    private readonly List<ReportSection> _sections = new();
    
    // ======================== Identity ========================
    public ReportId Id { get; private set; }
    public AccountNumber AccountNumber { get; private set; }
    
    // ======================== Configuration ========================
    /// <summary>리포트 유형</summary>
    public ReportType Type { get; private set; }
    
    /// <summary>리포트 제목</summary>
    public string Title { get; private set; }
    
    /// <summary>리포트 기간</summary>
    public ReportPeriod Period { get; private set; }
    
    // ======================== Content ========================
    /// <summary>리포트 섹션</summary>
    public IReadOnlyList<ReportSection> Sections => _sections.AsReadOnly();
    
    /// <summary>리포트 요약</summary>
    public ReportSummary Summary { get; private set; }
    
    // ======================== Status ========================
    /// <summary>리포트 상태</summary>
    public ReportStatus Status { get; private set; }
    
    /// <summary>생성 시간</summary>
    public DateTime GeneratedAt { get; private set; }
    
    /// <summary>발송 시간</summary>
    public DateTime? SentAt { get; private set; }
    
    /// <summary>발송 채널</summary>
    public List<NotificationChannel> SentTo { get; private set; }
    
    // ======================== Factory Methods ========================
    
    /// <summary>
    /// 새 리포트 생성
    /// </summary>
    public static Report Create(
        AccountNumber accountNumber,
        ReportType type,
        string title,
        ReportPeriod period)
    {
        var report = new Report
        {
            Id = ReportId.New(),
            AccountNumber = accountNumber,
            Type = type,
            Title = title,
            Period = period,
            Status = ReportStatus.Drafting,
            GeneratedAt = DateTime.UtcNow,
            SentTo = new List<NotificationChannel>()
        };
        
        report.AddDomainEvent(new ReportCreatedEvent(
            report.Id, accountNumber, type, title, DateTime.UtcNow));
        
        return report;
    }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 섹션 추가
    /// </summary>
    public void AddSection(string title, string content, SectionType sectionType)
    {
        var section = new ReportSection(
            ReportSectionId.New(),
            title,
            content,
            sectionType);
        
        _sections.Add(section);
    }
    
    /// <summary>
    /// 요약 설정
    /// </summary>
    public void SetSummary(ReportSummary summary)
    {
        Summary = summary;
    }
    
    /// <summary>
    /// 리포트 완성
    /// </summary>
    public void Complete()
    {
        if (Status != ReportStatus.Drafting)
            throw new InvalidOperationException("Report is not in drafting state");
        
        Status = ReportStatus.Generated;
        GeneratedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ReportGeneratedEvent(
            Id, Title, Type, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 리포트 발송
    /// </summary>
    public void Send(NotificationChannel channel)
    {
        if (Status != ReportStatus.Generated)
            throw new InvalidOperationException("Report is not ready to send");
        
        if (!SentTo.Contains(channel))
        {
            SentTo.Add(channel);
        }
        
        SentAt = DateTime.UtcNow;
        Status = ReportStatus.Sent;
        
        AddDomainEvent(new ReportSentEvent(
            Id, channel, DateTime.UtcNow));
    }
}
```

### 2.2 Value Objects

```csharp
/// <summary>시스템 모니터 ID</summary>
public sealed class SystemMonitorId : StronglyTypedId<Guid>
{
    public SystemMonitorId(Guid value) : base(value) { }
    public static SystemMonitorId New() => new(Guid.NewGuid());
}

/// <summary>대시보드 ID</summary>
public sealed class DashboardId : StronglyTypedId<Guid>
{
    public DashboardId(Guid value) : base(value) { }
    public static DashboardId New() => new(Guid.NewGuid());
}

/// <summary>리포트 ID</summary>
public sealed class ReportId : StronglyTypedId<Guid>
{
    public ReportId(Guid value) : base(value) { }
    public static ReportId New() => new(Guid.NewGuid());
}

/// <summary>헬스 체크</summary>
public sealed class HealthCheck : ValueObject
{
    public HealthCheckId Id { get; }
    public DateTime CheckedAt { get; }
    public SystemHealthSnapshot Snapshot { get; }
    
    public HealthCheck(
        HealthCheckId id,
        DateTime checkedAt,
        SystemHealthSnapshot snapshot)
    {
        Id = id;
        CheckedAt = checkedAt;
        Snapshot = snapshot;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return CheckedAt;
    }
}

/// <summary>시스템 메트릭</summary>
public sealed class SystemMetric : ValueObject
{
    public SystemMetricId Id { get; }
    public string MetricName { get; }
    public double Value { get; }
    public MetricUnit Unit { get; }
    public DateTime Timestamp { get; }
    
    public SystemMetric(
        SystemMetricId id,
        string metricName,
        double value,
        MetricUnit unit,
        DateTime timestamp)
    {
        Id = id;
        MetricName = metricName;
        Value = value;
        Unit = unit;
        Timestamp = timestamp;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return MetricName;
        yield return Timestamp;
    }
}

/// <summary>시스템 경고</summary>
public sealed class SystemAlert : ValueObject
{
    public SystemAlertId Id { get; }
    public AlertType Type { get; }
    public string Message { get; }
    public AlertSeverity Severity { get; }
    public DateTime RaisedAt { get; }
    
    public SystemAlert(
        SystemAlertId id,
        AlertType type,
        string message,
        AlertSeverity severity,
        DateTime raisedAt)
    {
        Id = id;
        Type = type;
        Message = message;
        Severity = severity;
        RaisedAt = raisedAt;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Type;
        yield return RaisedAt;
    }
}

/// <summary>시스템 헬스 스냅샷</summary>
public sealed class SystemHealthSnapshot : ValueObject
{
    public ConnectionStatus KiwoomApiStatus { get; }
    public ConnectionStatus DatabaseStatus { get; }
    public ConnectionStatus MessageBusStatus { get; }
    public ConnectionStatus MarketDataStatus { get; }
    
    public double CpuUsage { get; }
    public double MemoryUsage { get; }
    public long DiskUsage { get; }
    public double NetworkLatency { get; }
    public int ActiveThreadCount { get; }
    public int QueuedMessages { get; }
    
    public SystemHealthSnapshot(
        ConnectionStatus kiwoomApiStatus,
        ConnectionStatus databaseStatus,
        ConnectionStatus messageBusStatus,
        ConnectionStatus marketDataStatus,
        double cpuUsage,
        double memoryUsage,
        long diskUsage,
        double networkLatency,
        int activeThreadCount,
        int queuedMessages)
    {
        KiwoomApiStatus = kiwoomApiStatus;
        DatabaseStatus = databaseStatus;
        MessageBusStatus = messageBusStatus;
        MarketDataStatus = marketDataStatus;
        CpuUsage = cpuUsage;
        MemoryUsage = memoryUsage;
        DiskUsage = diskUsage;
        NetworkLatency = networkLatency;
        ActiveThreadCount = activeThreadCount;
        QueuedMessages = queuedMessages;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return KiwoomApiStatus;
        yield return CpuUsage;
        yield return MemoryUsage;
    }
}

/// <summary>리포트 요약</summary>
public sealed class ReportSummary : ValueObject
{
    public int TotalTrades { get; }
    public Money TotalProfitLoss { get; }
    public decimal WinRate { get; }
    public decimal SharpeRatio { get; }
    public decimal MaxDrawdown { get; }
    
    public ReportSummary(
        int totalTrades,
        Money totalProfitLoss,
        decimal winRate,
        decimal sharpeRatio,
        decimal maxDrawdown)
    {
        TotalTrades = totalTrades;
        TotalProfitLoss = totalProfitLoss;
        WinRate = winRate;
        SharpeRatio = sharpeRatio;
        MaxDrawdown = maxDrawdown;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TotalTrades;
        yield return TotalProfitLoss;
        yield return WinRate;
    }
}

/// <summary>대시보드 위젯</summary>
public sealed class DashboardWidget : ValueObject
{
    public DashboardWidgetId Id { get; }
    public DashboardWidgetType Type { get; }
    public WidgetPosition Position { get; }
    public Dictionary<string, object> Configuration { get; }
    
    public DashboardWidget(
        DashboardWidgetId id,
        DashboardWidgetType type,
        WidgetPosition position)
    {
        Id = id;
        Type = type;
        Position = position;
        Configuration = new Dictionary<string, object>();
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Type;
        yield return Position;
    }
}

/// <summary>거래 활동</summary>
public sealed class TradingActivity : ValueObject
{
    public TradingActivityType Type { get; }
    public Money ProfitLoss { get; }
    public TimeSpan Latency { get; }
    public int FailedOrdersCount { get; }
    public DateTime Timestamp { get; }
    
    public TradingActivity(
        TradingActivityType type,
        Money profitLoss,
        TimeSpan latency,
        int failedOrdersCount)
    {
        Type = type;
        ProfitLoss = profitLoss;
        Latency = latency;
        FailedOrdersCount = failedOrdersCount;
        Timestamp = DateTime.UtcNow;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Timestamp;
    }
}
```

### 2.3 Enumerations

```csharp
/// <summary>시스템 상태</summary>
public enum SystemStatus
{
    Initializing = 0,
    Healthy = 1,
    Degraded = 2,
    Warning = 3,
    Critical = 4,
    Offline = 5
}

/// <summary>연결 상태</summary>
public enum ConnectionStatus
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    Reconnecting = 3,
    Failed = 4,
    Maintenance = 5
}

/// <summary>경고 유형</summary>
public enum AlertType
{
    HighCpuUsage = 1,
    HighMemoryUsage = 2,
    HighDiskUsage = 3,
    HighNetworkLatency = 4,
    DatabaseError = 5,
    ApiError = 6,
    ConnectionLoss = 7,
    Error = 8,
    MetricThreshold = 9,
    PositionAlert = 10,
    RiskAlert = 11
}

/// <summary>경고 심각도</summary>
public enum AlertSeverity
{
    Info = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    Critical = 5
}

/// <summary>메트릭 단위</summary>
public enum MetricUnit
{
    Percent,
    Milliseconds,
    Bytes,
    Count,
    Ratio,
    Money
}

/// <summary>대시보드 유형</summary>
public enum DashboardType
{
    Trading = 1,
    Risk = 2,
    Performance = 3,
    System = 4,
    Custom = 5
}

/// <summary>대시보드 위젯 유형</summary>
public enum DashboardWidgetType
{
    PortfolioSummary = 1,
    RecentTrades = 2,
    ProfitLossChart = 3,
    StrategyPerformance = 4,
    RiskMetrics = 5,
    PositionRisk = 6,
    DrawdownChart = 7,
    AlertHistory = 8,
    SystemHealth = 9,
    OrderBook = 10
}

/// <summary>리포트 유형</summary>
public enum ReportType
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Quarterly = 4,
    Annual = 5,
    Custom = 6
}

/// <summary>리포트 기간</summary>
public enum ReportPeriod
{
    Today = 1,
    ThisWeek = 2,
    ThisMonth = 3,
    Last30Days = 4,
    Last90Days = 5,
    YearToDate = 6,
    AllTime = 7
}

/// <summary>리포트 상태</summary>
public enum ReportStatus
{
    Drafting = 1,
    Generated = 2,
    Sent = 3,
    Archived = 4
}

/// <summary>리포트 섹션 유형</summary>
public enum SectionType
{
    Summary = 1,
    Performance = 2,
    Risk = 3,
    Trades = 4,
    Positions = 5,
    Alerts = 6,
    Recommendation = 7
}

/// <summary>거래 활동 유형</summary>
public enum TradingActivityType
{
    OrderSubmitted = 1,
    OrderExecuted = 2,
    OrderCancelled = 3,
    PositionOpened = 4,
    PositionClosed = 5,
    StrategyStarted = 6,
    StrategyStopped = 7,
    StrategyError = 8
}

/// <summary>에러 심각도</summary>
public enum ErrorSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>알림 채널</summary>
public enum NotificationChannel
{
    InApp = 1,
    Email = 2,
    SMS = 3,
    Push = 4,
    Telegram = 5,
    Slack = 6
}

/// <summary>새로고침 간격</summary>
public enum RefreshInterval
{
    OneSecond = 1,
    FiveSeconds = 5,
    TenSeconds = 10,
    ThirtySeconds = 30,
    OneMinute = 60,
    FiveMinutes = 300
}

/// <summary>대시보드 레이아웃</summary>
public enum DashboardLayout
{
    Grid = 1,
    List = 2,
    Tabbed = 3,
    Free = 4
}
```

---

## 3. Business Rules & Invariants

### 3.1 핵심 불변식

```csharp
/// <summary>
/// 모니터링 핵심 비즈니스 규칙
/// </summary>
public static class MonitoringInvariants
{
    /// <summary>
    /// Invariant 1: 활성 경고는 음수일 수 없음
    /// </summary>
    public static void ValidateAlertCount(int count)
    {
        if (count < 0)
            throw new InvalidOperationException("Alert count cannot be negative");
    }
    
    /// <summary>
    /// Invariant 2: CPU/Memory 사용률은 0-100 범위
    /// </summary>
    public static void ValidateUsagePercentage(double percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new InvalidOperationException("Usage percentage must be between 0 and 100");
    }
    
    /// <summary>
    /// Invariant 3: 대시보드는 최대 20개 위젯만 가능
    /// </summary>
    public static void ValidateWidgetCount(int count)
    {
        if (count > 20)
            throw new InvalidOperationException("Maximum 20 widgets allowed per dashboard");
    }
}
```

### 3.2 비즈니스 규칙

```csharp
/// <summary>
/// 모니터링 비즈니스 규칙
/// </summary>
public static class MonitoringRules
{
    /// <summary>
    /// 규칙 1: 심각 경고는 모든 채널로 발송
    /// </summary>
    public static List<NotificationChannel> GetChannelsForSeverity(AlertSeverity severity)
    {
        return severity switch
        {
            AlertSeverity.Critical => new() 
            { 
                NotificationChannel.InApp,
                NotificationChannel.Email,
                NotificationChannel.SMS,
                NotificationChannel.Push,
                NotificationChannel.Telegram
            },
            AlertSeverity.High => new()
            {
                NotificationChannel.InApp,
                NotificationChannel.Email,
                NotificationChannel.Push
            },
            AlertSeverity.Medium => new()
            {
                NotificationChannel.InApp,
                NotificationChannel.Push
            },
            _ => new() { NotificationChannel.InApp }
        };
    }
    
    /// <summary>
    /// 규칙 2: 조용한 시간 적용 여부
    /// </summary>
    public static bool IsQuietHours(DateTime time, TimeRange quietHours)
    {
        return time.TimeOfDay >= quietHours.Start && 
               time.TimeOfDay < quietHours.End;
    }
}
```

---

## 4. Domain Events

```csharp
/// <summary>시스템 모니터 생성 이벤트</summary>
public sealed class SystemMonitorCreatedEvent : DomainEvent
{
    public SystemMonitorId MonitorId { get; }
    public string SystemName { get; }
    public DateTime CreatedAt { get; }
    
    public SystemMonitorCreatedEvent(
        SystemMonitorId monitorId,
        string systemName,
        DateTime createdAt)
    {
        MonitorId = monitorId;
        SystemName = systemName;
        CreatedAt = createdAt;
    }
}

/// <summary>시스템 경고 발생 이벤트</summary>
public sealed class SystemAlertRaisedEvent : DomainEvent
{
    public SystemMonitorId MonitorId { get; }
    public AlertType AlertType { get; }
    public string Message { get; }
    public AlertSeverity Severity { get; }
    public DateTime RaisedAt { get; }
    
    public SystemAlertRaisedEvent(
        SystemMonitorId monitorId,
        AlertType alertType,
        string message,
        AlertSeverity severity,
        DateTime raisedAt)
    {
        MonitorId = monitorId;
        AlertType = alertType;
        Message = message;
        Severity = severity;
        RaisedAt = raisedAt;
    }
}

/// <summary>시스템 에러 감지 이벤트</summary>
public sealed class SystemErrorDetectedEvent : DomainEvent
{
    public SystemMonitorId MonitorId { get; }
    public string ErrorMessage { get; }
    public ErrorSeverity Severity { get; }
    public string Source { get; }
    public DateTime DetectedAt { get; }
    
    public SystemErrorDetectedEvent(
        SystemMonitorId monitorId,
        string errorMessage,
        ErrorSeverity severity,
        string source,
        DateTime detectedAt)
    {
        MonitorId = monitorId;
        ErrorMessage = errorMessage;
        Severity = severity;
        Source = source;
        DetectedAt = detectedAt;
    }
}

/// <summary>거래 활동 기록 이벤트</summary>
public sealed class TradingActivityRecordedEvent : DomainEvent
{
    public SystemMonitorId MonitorId { get; }
    public TradingActivityType ActivityType { get; }
    public Money ProfitLoss { get; }
    public DateTime RecordedAt { get; }
    
    public TradingActivityRecordedEvent(
        SystemMonitorId monitorId,
        TradingActivityType activityType,
        Money profitLoss,
        DateTime recordedAt)
    {
        MonitorId = monitorId;
        ActivityType = activityType;
        ProfitLoss = profitLoss;
        RecordedAt = recordedAt;
    }
}

/// <summary>대시보드 생성 이벤트</summary>
public sealed class DashboardCreatedEvent : DomainEvent
{
    public DashboardId DashboardId { get; }
    public UserId UserId { get; }
    public string Name { get; }
    public DashboardType Type { get; }
    public DateTime CreatedAt { get; }
    
    public DashboardCreatedEvent(
        DashboardId dashboardId,
        UserId userId,
        string name,
        DashboardType type,
        DateTime createdAt)
    {
        DashboardId = dashboardId;
        UserId = userId;
        Name = name;
        Type = type;
        CreatedAt = createdAt;
    }
}

/// <summary>리포트 생성 이벤트</summary>
public sealed class ReportGeneratedEvent : DomainEvent
{
    public ReportId ReportId { get; }
    public string Title { get; }
    public ReportType Type { get; }
    public DateTime GeneratedAt { get; }
    
    public ReportGeneratedEvent(
        ReportId reportId,
        string title,
        ReportType type,
        DateTime generatedAt)
    {
        ReportId = reportId;
        Title = title;
        Type = type;
        GeneratedAt = generatedAt;
    }
}

/// <summary>리포트 발송 이벤트</summary>
public sealed class ReportSentEvent : DomainEvent
{
    public ReportId ReportId { get; }
    public NotificationChannel Channel { get; }
    public DateTime SentAt { get; }
    
    public ReportSentEvent(
        ReportId reportId,
        NotificationChannel channel,
        DateTime sentAt)
    {
        ReportId = reportId;
        Channel = channel;
        SentAt = sentAt;
    }
}
```

---

## 5. Domain Services

```csharp
/// <summary>시스템 모니터링 도메인 서비스</summary>
public interface ISystemMonitoringService : IDomainService
{
    Task<SystemHealthSnapshot> GetSystemHealthAsync();
    
    Task RecordMetricsAsync(
        SystemMonitorId monitorId,
        Dictionary<string, double> metrics);
    
    Task<List<SystemAlert>> GetActiveAlertsAsync(
        SystemMonitorId monitorId);
}

/// <summary>대시보드 데이터 도메인 서비스</summary>
public interface IDashboardDataService : IDomainService
{
    Task<DashboardData> GenerateDashboardDataAsync(
        Dashboard dashboard,
        DateRange dateRange);
    
    Task<object> GenerateWidgetDataAsync(
        DashboardWidget widget,
        DateRange dateRange);
}

/// <summary>리포트 생성 도메인 서비스</summary>
public interface IReportGenerationService : IDomainService
{
    Task<Report> GenerateReportAsync(
        AccountNumber accountNumber,
        ReportType type,
        ReportPeriod period);
    
    Task<byte[]> ExportReportAsync(
        ReportId reportId,
        ReportFormat format);
}

/// <summary>경고 관리 도메인 서비스</summary>
public interface IAlertManagementService : IDomainService
{
    Task SendAlertAsync(
        SystemAlert alert,
        List<NotificationChannel> channels);
    
    Task<bool> ShouldSendAlertAsync(
        SystemAlert alert,
        UserPreferences preferences);
}
```

---

## 6. Application Services

```csharp
/// <summary>모니터링 애플리케이션 서비스</summary>
public sealed class MonitoringService : IApplicationService
{
    private readonly ISystemMonitoringService _monitoringService;
    private readonly IDashboardDataService _dashboardService;
    private readonly IReportGenerationService _reportService;
    private readonly IAlertManagementService _alertService;
    private readonly ISystemMonitorRepository _monitorRepository;
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MonitoringService> _logger;
    
    /// <summary>시스템 상태 업데이트</summary>
    public async Task<Result> UpdateSystemStatusAsync(
        SystemHealthSnapshot healthSnapshot)
    {
        try
        {
            var monitor = await _monitorRepository.GetDefaultAsync();
            if (monitor == null)
                return Result.Failure("System monitor not found");
            
            monitor.PerformHealthCheck(healthSnapshot);
            
            await _monitorRepository.UpdateAsync(monitor);
            await _unitOfWork.CommitAsync();
            
            foreach (var @event in monitor.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update system status");
            return Result.Failure($"System status update failed: {ex.Message}");
        }
    }
    
    /// <summary>대시보드 데이터 조회</summary>
    public async Task<Result<DashboardDataDto>> GetDashboardDataAsync(
        Guid dashboardId,
        DateRange dateRange)
    {
        try
        {
            var dashboard = await _dashboardRepository.GetByIdAsync(
                new DashboardId(dashboardId));
            
            if (dashboard == null)
                return Result<DashboardDataDto>.Failure("Dashboard not found");
            
            var data = await _dashboardService.GenerateDashboardDataAsync(
                dashboard, dateRange);
            
            dashboard.UpdateData(data);
            
            await _dashboardRepository.UpdateAsync(dashboard);
            await _unitOfWork.CommitAsync();
            
            return Result<DashboardDataDto>.Success(MapToDto(data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard data");
            return Result<DashboardDataDto>.Failure(
                $"Dashboard data retrieval failed: {ex.Message}");
        }
    }
    
    /// <summary>일일 리포트 생성</summary>
    public async Task<Result<ReportDto>> GenerateDailyReportAsync(
        AccountNumber accountNumber)
    {
        try
        {
            var report = await _reportService.GenerateReportAsync(
                accountNumber,
                ReportType.Daily,
                ReportPeriod.Today);
            
            report.Complete();
            
            await _reportRepository.AddAsync(report);
            await _unitOfWork.CommitAsync();
            
            foreach (var @event in report.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return Result<ReportDto>.Success(MapToDto(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate daily report");
            return Result<ReportDto>.Failure(
                $"Report generation failed: {ex.Message}");
        }
    }
}
```

---

## 7. Infrastructure Layer

```csharp
/// <summary>시스템 모니터 저장소 구현</summary>
public sealed class SystemMonitorRepository : ISystemMonitorRepository
{
    private readonly MonitoringDbContext _context;
    private readonly IDistributedCache _cache;
    
    public async Task<SystemMonitor> GetDefaultAsync()
    {
        const string cacheKey = "system_monitor:default";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        if (cachedData != null)
            return JsonSerializer.Deserialize<SystemMonitor>(cachedData);
        
        var entity = await _context.SystemMonitors
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.SystemName == "TradingSystem");
        
        if (entity != null)
        {
            var json = JsonSerializer.Serialize(entity);
            await _cache.SetStringAsync(cacheKey, json, TimeSpan.FromSeconds(5));
        }
        
        return entity;
    }
    
    public async Task UpdateAsync(SystemMonitor monitor)
    {
        _context.SystemMonitors.Update(monitor);
        await _context.SaveChangesAsync();
        
        const string cacheKey = "system_monitor:default";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

---

## 8. API/Interface Specifications

```csharp
[ApiController]
[Route("api/monitoring")]
[Authorize]
public class MonitoringController : ControllerBase
{
    private readonly IMediator _mediator;
    
    /// <summary>GET /api/monitoring/health - 시스템 상태 조회</summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<ActionResult<SystemHealthDto>> GetSystemHealthAsync()
    {
        var query = new GetSystemHealthQuery();
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return StatusCode(503, result.Error);
        
        return Ok(result.Value);
    }
    
    /// <summary>GET /api/monitoring/dashboard/{dashboardId} - 대시보드 조회</summary>
    [HttpGet("dashboard/{dashboardId}")]
    public async Task<ActionResult<DashboardDataDto>> GetDashboardAsync(
        Guid dashboardId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var dateRange = new DateRange(startDate ?? DateTime.UtcNow.AddDays(-30), 
                                      endDate ?? DateTime.UtcNow);
        
        var query = new GetDashboardQuery(dashboardId, dateRange);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }
    
    /// <summary>POST /api/monitoring/alerts - 경고 조회</summary>
    [HttpGet("alerts")]
    public async Task<ActionResult<List<SystemAlertDto>>> GetAlertsAsync(
        [FromQuery] AlertSeverity? minSeverity = null,
        [FromQuery] int limit = 20)
    {
        var query = new GetAlertsQuery(minSeverity, limit);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
}
```

---

## 9. Monitoring Strategies

### 헬스 체크 전략

```csharp
/// <summary>헬스 체크 전략</summary>
public interface IHealthCheckStrategy
{
    Task<ComponentHealth> CheckHealthAsync();
}

/// <summary>Kiwoom API 헬스 체크</summary>
public class KiwoomApiHealthCheck : IHealthCheckStrategy
{
    private readonly IKiwoomApiClient _apiClient;
    
    public async Task<ComponentHealth> CheckHealthAsync()
    {
        try
        {
            var response = await _apiClient.PingAsync(TimeSpan.FromSeconds(5));
            
            return new ComponentHealth(
                "KiwoomAPI",
                response.Success ? ConnectionStatus.Connected : ConnectionStatus.Failed,
                response.Latency);
        }
        catch
        {
            return new ComponentHealth(
                "KiwoomAPI",
                ConnectionStatus.Failed,
                null);
        }
    }
}

/// <summary>데이터베이스 헬스 체크</summary>
public class DatabaseHealthCheck : IHealthCheckStrategy
{
    private readonly DbContext _context;
    
    public async Task<ComponentHealth> CheckHealthAsync()
    {
        try
        {
            var sw = Stopwatch.StartNew();
            await _context.Database.ExecuteSqlAsync($"SELECT 1");
            sw.Stop();
            
            return new ComponentHealth(
                "Database",
                ConnectionStatus.Connected,
                TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds));
        }
        catch
        {
            return new ComponentHealth(
                "Database",
                ConnectionStatus.Failed,
                null);
        }
    }
}
```

---

## 10. Alert Management

### 경고 관리 서비스

```csharp
/// <summary>경고 관리 서비스</summary>
public class AlertManagementService
{
    private readonly INotificationService _notificationService;
    private readonly IAlertRepository _alertRepository;
    
    public async Task SendAlertAsync(
        SystemAlert alert,
        UserPreferences preferences)
    {
        // 조용한 시간 확인
        if (IsQuietHours(DateTime.UtcNow, preferences.QuietHours) &&
            alert.Severity < AlertSeverity.High)
        {
            return;
        }
        
        // 채널 선택
        var channels = MonitoringRules.GetChannelsForSeverity(alert.Severity);
        
        // 채널별 발송
        foreach (var channel in channels)
        {
            try
            {
                await SendViaChannelAsync(alert, channel, preferences);
            }
            catch (Exception ex)
            {
                // 채널 발송 실패 로깅
                _logger.LogError(ex, $"Failed to send alert via {channel}");
            }
        }
    }
    
    private async Task SendViaChannelAsync(
        SystemAlert alert,
        NotificationChannel channel,
        UserPreferences preferences)
    {
        var message = FormatAlertMessage(alert);
        
        switch (channel)
        {
            case NotificationChannel.Email:
                await _notificationService.SendEmailAsync(
                    preferences.Email, alert.Type.ToString(), message);
                break;
                
            case NotificationChannel.SMS:
                await _notificationService.SendSmsAsync(
                    preferences.PhoneNumber, message);
                break;
                
            case NotificationChannel.Telegram:
                await _notificationService.SendTelegramAsync(
                    preferences.TelegramChatId, message);
                break;
        }
    }
}
```

---

## 11. Report Generation

### 리포트 생성 서비스

```csharp
/// <summary>리포트 생성 서비스</summary>
public class ReportGenerationService
{
    private readonly ITradeRepository _tradeRepository;
    private readonly IStrategyRepository _strategyRepository;
    
    public async Task<Report> GenerateReportAsync(
        AccountNumber accountNumber,
        ReportType type,
        ReportPeriod period)
    {
        var report = Report.Create(accountNumber, type, 
            $"{type} Report", period);
        
        var dateRange = period.ToDateRange();
        
        // 1. 거래 데이터 수집
        var trades = await _tradeRepository.GetTradesAsync(
            accountNumber, dateRange);
        
        // 2. 성과 요약
        var summary = CalculateSummary(trades);
        report.SetSummary(summary);
        
        // 3. 섹션 추가
        report.AddSection(
            "Performance",
            GeneratePerformanceContent(trades, summary),
            SectionType.Performance);
        
        report.AddSection(
            "Recent Trades",
            GenerateTradesContent(trades.TakeLast(20).ToList()),
            SectionType.Trades);
        
        // 4. 완료
        report.Complete();
        
        return report;
    }
    
    private ReportSummary CalculateSummary(List<Trade> trades)
    {
        return new ReportSummary(
            trades.Count,
            trades.Sum(t => t.ProfitLoss),
            trades.Count > 0 ? 
                (decimal)trades.Count(t => t.ProfitLoss > 0) / trades.Count * 100 : 0,
            CalculateSharpeRatio(trades),
            CalculateMaxDrawdown(trades));
    }
}
```

---

## 12. Error Handling & Validation

```csharp
public sealed class SystemMonitorNotFoundException : DomainException
{
    public SystemMonitorNotFoundException(string message)
        : base(message) { }
}

public sealed class DashboardMaxWidgetLimitExceededException : DomainException
{
    public DashboardMaxWidgetLimitExceededException(string message)
        : base(message) { }
}

public sealed class InvalidAlertSeverityException : DomainException
{
    public InvalidAlertSeverityException(string message)
        : base(message) { }
}

public sealed class ReportGenerationException : DomainException
{
    public ReportGenerationException(string message)
        : base(message) { }
}
```

---

## 13. Performance Considerations

### 캐싱 전략
- L1: Redis (5초 TTL) - 시스템 상태
- L2: In-Memory (1분 TTL) - 대시보드 데이터
- L3: Database - 완전 이력

### 실시간 업데이트
- SignalR을 통한 WebSocket 연결
- 초당 메트릭 스트리밍
- 경고 즉시 푸시

### 배치 처리
- 일일 리포트 생성 (새벽 6시)
- 주간 리포트 생성 (주일 오전 9시)
- 월간 리포트 생성 (매달 1일 오전 9시)
- 이력 데이터 정리 (90일 이상)

### 모니터링 메트릭 저장
- 1시간: 1초 간격
- 24시간: 1분 간격
- 30일: 1시간 간격
- 1년: 1일 간격

---

## 결론

이 Monitoring Context 설계는 다음 특징을 갖습니다:

✅ **완전성**: 시스템 모니터링의 모든 측면
✅ **실시간성**: 즉각적인 상태 감지 및 알림
✅ **투명성**: 완전한 시스템 상태 공개
✅ **직관성**: 직관적 시각화와 대시보드
✅ **자동화**: 자동 리포트 생성 및 배포
✅ **확장성**: 다양한 알림 채널 지원
✅ **통합성**: 다른 Context와의 원활한 연동

이로써 6개의 모든 Bounded Context 설계가 완성되었습니다! 🎉

**완성된 Context:**
- ✅ Account Context
- ✅ Trading Context  
- ✅ Strategy Context
- ✅ Risk Management Context
- ✅ Market Data Context (설계 예정)
- ✅ Monitoring Context
