# Risk Management Context - DDD 상세 설계

## 📋 목차
1. [Context Overview](#1-context-overview)
2. [Domain Model 상세 설계](#2-domain-model-상세-설계)
3. [Business Rules & Invariants](#3-business-rules--invariants)
4. [Domain Events](#4-domain-events)
5. [Domain Services](#5-domain-services)
6. [Application Services](#6-application-services)
7. [Infrastructure Layer](#7-infrastructure-layer)
8. [API/Interface Specifications](#8-apiinterface-specifications)
9. [Risk Management Strategies](#9-risk-management-strategies)
10. [Monitoring & Alerts](#10-monitoring--alerts)
11. [Error Handling & Validation](#11-error-handling--validation)
12. [Performance Considerations](#12-performance-considerations)

---

## 1. Context Overview

### 1.1 책임과 범위

**Risk Management Context**는 모든 거래 활동의 리스크를 실시간으로 모니터링하고 관리하는 핵심 Bounded Context입니다.

#### 주요 책임
- ✅ 실시간 리스크 지표 모니터링 및 경고
- ✅ 포지션 사이징 및 자금 관리
- ✅ 손절/익절 자동 실행
- ✅ 리스크 한도 설정 및 관리
- ✅ 긴급 상황 대응 (킬 스위치)
- ✅ 포트폴리오 리스크 분석
- ✅ Circuit Breaker 관리
- ✅ 규제 준수 및 컴플라이언스

#### 핵심 원칙
- **보수성**: 리스크 우선 의사결정
- **실시간성**: 즉각적인 위험 감지 및 대응
- **다층방어**: Defense in Depth 전략
- **투명성**: 모든 리스크 지표 공개
- **자동화**: 자동화된 리스크 관리
- **회복력**: 장애 상황에서의 자동 보호

### 1.2 Context Map

```
┌─────────────────────────────────┐
│  Risk Management Context        │
│    (리스크 관리 & 모니터링)     │
└─────────────────────────────────┘
    ↓ 리스크 검증    ↑ 신호
    │               │
    ↓               ↑
Strategy Context  Trading Context
(신호 생성)       (거래 실행)
    ↑               ↓
    │ 거래 결과     │
    │ 포지션       │
    ├─────────────┤
    
    ↓ 포지션 손절/익절
    
Account Context
(포지션 관리)
```

### 1.3 바운더리

**이 Context가 담당하는 것:**
- 리스크 프로필 관리
- 실시간 리스크 모니터링
- 포지션 사이징 계산
- 손절/익절 관리
- 긴급 통제 (킬 스위치)
- 리스크 보고

**이 Context가 담당하지 않는 것:**
- 신호 생성 (Strategy Context)
- 주문 실행 (Trading Context)
- 포지션 보유 (Account Context)
- 시세 데이터 (Market Data Context)
- 모니터링 UI (Monitoring Context)

---

## 2. Domain Model 상세 설계

### 2.1 Aggregates

#### 2.1.1 RiskProfile Aggregate (리스크 프로필)

**Aggregate Root**

```csharp
/// <summary>
/// 리스크 프로필 Aggregate Root
/// 거래 리스크 정책과 한도를 관리
/// </summary>
public class RiskProfile : AggregateRoot<RiskProfileId>
{
    private readonly List<RiskLimit> _riskLimits = new();
    private readonly List<RiskViolationHistory> _violationHistory = new();
    
    // ======================== Identity ========================
    /// <summary>리스크 프로필 고유 ID</summary>
    public RiskProfileId Id { get; private set; }
    
    /// <summary>계좌번호</summary>
    public AccountNumber AccountNumber { get; private set; }
    
    /// <summary>프로필 이름</summary>
    public string ProfileName { get; private set; }
    
    /// <summary>설명</summary>
    public string Description { get; private set; }
    
    // ======================== Position Limits ========================
    /// <summary>최대 포지션 크기</summary>
    public Money MaxPositionSize { get; private set; }
    
    /// <summary>최대 오픈 포지션 수</summary>
    public int MaxOpenPositions { get; private set; }
    
    /// <summary>최대 노출 비율 (총 자본 대비)</summary>
    public decimal MaxExposureRatio { get; private set; }
    
    /// <summary>최대 일일 손실</summary>
    public Money MaxDailyLoss { get; private set; }
    
    /// <summary>최대 낙폭 (Drawdown)</summary>
    public decimal MaxDrawdown { get; private set; }
    
    /// <summary>최대 포트폴리오 VaR</summary>
    public Money MaxValueAtRisk { get; private set; }
    
    // ======================== Stop Loss & Take Profit ========================
    /// <summary>기본 손절 설정</summary>
    public StopLossConfiguration DefaultStopLoss { get; private set; }
    
    /// <summary>기본 익절 설정</summary>
    public TakeProfitConfiguration DefaultTakeProfit { get; private set; }
    
    /// <summary>손절 필수 여부</summary>
    public bool IsStopLossMandatory { get; private set; }
    
    /// <summary>익절 선택 여부</summary>
    public bool IsTakeProfitOptional { get; private set; }
    
    // ======================== Time-based Limits ========================
    /// <summary>거래 가능 시간</summary>
    public TradingTimeRestriction TradingHours { get; private set; }
    
    /// <summary>일일 최대 거래 수</summary>
    public int MaxTradesPerDay { get; private set; }
    
    /// <summary>시간당 최대 거래 수</summary>
    public int MaxTradesPerHour { get; private set; }
    
    // ======================== Monitoring ========================
    /// <summary>모니터링 활성화 여부</summary>
    public bool IsMonitoringEnabled { get; private set; }
    
    /// <summary>모니터링 간격</summary>
    public TimeSpan MonitoringInterval { get; private set; }
    
    /// <summary>경고 활성화 여부</summary>
    public bool IsAlertEnabled { get; private set; }
    
    // ======================== Status ========================
    /// <summary>프로필 상태</summary>
    public RiskProfileStatus Status { get; private set; }
    
    /// <summary>생성 시간</summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>마지막 수정 시간</summary>
    public DateTime LastModifiedAt { get; private set; }
    
    /// <summary>활성화 시간</summary>
    public DateTime? ActivatedAt { get; private set; }
    
    // ======================== Constructor & Factory Methods ========================
    
    /// <summary>
    /// 새 리스크 프로필 생성
    /// </summary>
    public static RiskProfile Create(
        AccountNumber accountNumber,
        string profileName,
        string description)
    {
        var profile = new RiskProfile
        {
            Id = RiskProfileId.New(),
            AccountNumber = accountNumber,
            ProfileName = profileName,
            Description = description,
            Status = RiskProfileStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            
            // 기본값
            MaxPositionSize = Money.Zero,
            MaxOpenPositions = 5,
            MaxExposureRatio = 0.5m,
            MaxDailyLoss = Money.Zero,
            MaxDrawdown = 0.2m,
            MaxValueAtRisk = Money.Zero,
            IsStopLossMandatory = true,
            IsTakeProfitOptional = false,
            IsMonitoringEnabled = true,
            MonitoringInterval = TimeSpan.FromSeconds(1),
            IsAlertEnabled = true,
            MaxTradesPerDay = 20,
            MaxTradesPerHour = 5
        };
        
        profile.AddDomainEvent(new RiskProfileCreatedEvent(
            profile.Id, accountNumber, profileName, DateTime.UtcNow));
        
        return profile;
    }
    
    /// <summary>
    /// Conservative 프로필 템플릿
    /// </summary>
    public static RiskProfile CreateConservativeProfile(
        AccountNumber accountNumber,
        Money totalCapital)
    {
        var profile = Create(accountNumber, "Conservative", "보수적 리스크 관리");
        
        profile.MaxPositionSize = totalCapital * 0.05m; // 5%
        profile.MaxOpenPositions = 3;
        profile.MaxExposureRatio = 0.3m;
        profile.MaxDailyLoss = totalCapital * -0.02m; // -2%
        profile.MaxDrawdown = 0.1m;
        profile.MaxValueAtRisk = totalCapital * 0.02m;
        profile.DefaultStopLoss = StopLossConfiguration.Percentage(3);
        profile.DefaultTakeProfit = TakeProfitConfiguration.Percentage(6);
        
        return profile;
    }
    
    /// <summary>
    /// Moderate 프로필 템플릿
    /// </summary>
    public static RiskProfile CreateModerateProfile(
        AccountNumber accountNumber,
        Money totalCapital)
    {
        var profile = Create(accountNumber, "Moderate", "중간 리스크 관리");
        
        profile.MaxPositionSize = totalCapital * 0.1m; // 10%
        profile.MaxOpenPositions = 5;
        profile.MaxExposureRatio = 0.5m;
        profile.MaxDailyLoss = totalCapital * -0.05m; // -5%
        profile.MaxDrawdown = 0.15m;
        profile.MaxValueAtRisk = totalCapital * 0.05m;
        profile.DefaultStopLoss = StopLossConfiguration.Percentage(5);
        profile.DefaultTakeProfit = TakeProfitConfiguration.Percentage(10);
        
        return profile;
    }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 새 포지션의 리스크 평가
    /// </summary>
    public RiskAssessment AssessNewPosition(
        PositionRequest request,
        CurrentPortfolio portfolio,
        MarketData marketData)
    {
        var assessment = new RiskAssessment(request);
        
        // 1. 포지션 크기 검증
        if (request.Value > MaxPositionSize)
        {
            assessment.AddViolation(new RiskViolation(
                RiskViolationType.PositionSizeExceeded,
                $"포지션 크기 {request.Value:C}가 최대값 {MaxPositionSize:C}를 초과"));
        }
        
        // 2. 오픈 포지션 수 검증
        if (portfolio.OpenPositionCount >= MaxOpenPositions)
        {
            assessment.AddViolation(new RiskViolation(
                RiskViolationType.MaxPositionsExceeded,
                $"오픈 포지션 수 {portfolio.OpenPositionCount}가 최대값 {MaxOpenPositions}에 도달"));
        }
        
        // 3. 노출 비율 검증
        var newExposure = portfolio.CalculateExposureWith(request);
        if (newExposure > MaxExposureRatio)
        {
            assessment.AddViolation(new RiskViolation(
                RiskViolationType.ExposureRatioExceeded,
                $"노출 비율 {newExposure:P}가 최대값 {MaxExposureRatio:P}를 초과"));
        }
        
        // 4. 일일 손실 한도 검증
        if (portfolio.TodayLoss <= MaxDailyLoss)
        {
            assessment.AddViolation(new RiskViolation(
                RiskViolationType.DailyLossExceeded,
                $"일일 손실 {portfolio.TodayLoss:C}가 최대값 {MaxDailyLoss:C}를 초과"));
        }
        
        // 5. VaR 검증
        var estimatedVaR = EstimateVaR(request, marketData);
        if (estimatedVaR > MaxValueAtRisk)
        {
            assessment.AddViolation(new RiskViolation(
                RiskViolationType.VaRExceeded,
                $"Value at Risk {estimatedVaR:C}가 최대값 {MaxValueAtRisk:C}를 초과"));
        }
        
        // 6. 손절 확인
        if (IsStopLossMandatory && request.StopLossPrice == null)
        {
            assessment.AddViolation(new RiskViolation(
                RiskViolationType.MissingStopLoss,
                "손절 주문이 필수이나 설정되지 않음"));
        }
        
        // 최종 승인 결정
        assessment.DetermineApproval();
        
        // 위반 기록
        if (!assessment.IsApproved)
        {
            RecordViolation(assessment);
            AddDomainEvent(new RiskLimitViolatedEvent(
                Id, request, assessment.Violations, DateTime.UtcNow));
        }
        
        return assessment;
    }
    
    /// <summary>
    /// 최적 포지션 크기 계산
    /// </summary>
    public Money CalculateOptimalPositionSize(
        TradingSignal signal,
        CurrentPortfolio portfolio,
        MarketVolatility volatility)
    {
        // Kelly Criterion 기반 계산
        var winProbability = signal.EstimatedWinRate;
        var expectedReturn = signal.ExpectedReturnPercent;
        var riskRewardRatio = signal.RiskRewardRatio;
        
        // Kelly Formula: f = (p * b - q) / b
        // p = win probability, q = 1-p, b = win/loss ratio
        var b = riskRewardRatio;
        var q = 1 - winProbability;
        var kellyFraction = ((winProbability * b) - q) / b;
        
        // 보수적 Kelly (절반)
        var conservativeKelly = kellyFraction * 0.5m;
        
        // 변동성 조정
        var volatilityAdjustment = 1 - (volatility.ImpliedVolatility * 0.1m);
        var adjustedPositionRatio = conservativeKelly * volatilityAdjustment;
        
        // 리스크 한도 적용
        var positionSize = portfolio.TotalEquity * adjustedPositionRatio;
        
        // 최대값 제한
        return Money.Min(
            positionSize,
            MaxPositionSize,
            portfolio.AvailableCash);
    }
    
    /// <summary>
    /// 프로필 활성화
    /// </summary>
    public Result Activate()
    {
        if (Status == RiskProfileStatus.Active)
            return Result.Failure("이미 활성 상태입니다");
        
        // 필수 설정 확인
        if (MaxPositionSize == Money.Zero)
            return Result.Failure("최대 포지션 크기를 설정하세요");
        
        if (MaxDailyLoss == Money.Zero)
            return Result.Failure("일일 최대 손실을 설정하세요");
        
        if (DefaultStopLoss == null)
            return Result.Failure("기본 손절을 설정하세요");
        
        Status = RiskProfileStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        
        AddDomainEvent(new RiskProfileActivatedEvent(Id, DateTime.UtcNow));
        
        return Result.Success();
    }
    
    /// <summary>
    /// 프로필 비활성화
    /// </summary>
    public void Deactivate(string reason = null)
    {
        if (Status != RiskProfileStatus.Active)
            return;
        
        Status = RiskProfileStatus.Inactive;
        LastModifiedAt = DateTime.UtcNow;
        
        AddDomainEvent(new RiskProfileDeactivatedEvent(Id, reason, DateTime.UtcNow));
    }
    
    /// <summary>
    /// VaR 추정
    /// </summary>
    private Money EstimateVaR(
        PositionRequest request,
        MarketData marketData)
    {
        // 단순 VaR 계산 (95% confidence level)
        var dailyVolatility = marketData.HistoricalVolatility * (decimal)Math.Sqrt(1.0 / 252.0);
        var var95 = request.Value * dailyVolatility * 1.645m; // Z-score for 95%
        
        return new Money(var95);
    }
    
    /// <summary>
    /// 위반 기록
    /// </summary>
    private void RecordViolation(RiskAssessment assessment)
    {
        var history = new RiskViolationHistory(
            RiskViolationHistoryId.New(),
            DateTime.UtcNow,
            assessment.Violations.ToList());
        
        _violationHistory.Add(history);
    }
    
    /// <summary>
    /// 리스크 프로필 업데이트
    /// </summary>
    public void UpdateLimits(
        Money? maxPositionSize = null,
        int? maxOpenPositions = null,
        decimal? maxExposureRatio = null,
        Money? maxDailyLoss = null,
        decimal? maxDrawdown = null)
    {
        if (Status == RiskProfileStatus.Active)
            throw new InvalidOperationException("활성 프로필은 수정할 수 없습니다");
        
        if (maxPositionSize.HasValue)
            MaxPositionSize = maxPositionSize.Value;
        
        if (maxOpenPositions.HasValue)
            MaxOpenPositions = maxOpenPositions.Value;
        
        if (maxExposureRatio.HasValue)
            MaxExposureRatio = maxExposureRatio.Value;
        
        if (maxDailyLoss.HasValue)
            MaxDailyLoss = maxDailyLoss.Value;
        
        if (maxDrawdown.HasValue)
            MaxDrawdown = maxDrawdown.Value;
        
        LastModifiedAt = DateTime.UtcNow;
        
        AddDomainEvent(new RiskProfileUpdatedEvent(Id, DateTime.UtcNow));
    }
}
```

#### 2.1.2 RiskMonitor Aggregate (리스크 모니터)

```csharp
/// <summary>
/// 리스크 모니터 Aggregate Root
/// 실시간 리스크 메트릭 모니터링
/// </summary>
public class RiskMonitor : AggregateRoot<RiskMonitorId>
{
    private readonly List<RiskAlert> _activeAlerts = new();
    private readonly List<PortfolioSnapshot> _snapshots = new();
    
    // ======================== Identity ========================
    public RiskMonitorId Id { get; private set; }
    public AccountNumber AccountNumber { get; private set; }
    public RiskProfileId RiskProfileId { get; private set; }
    
    // ======================== Status ========================
    public MonitoringStatus Status { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    
    // ======================== Portfolio Metrics ========================
    /// <summary>포트폴리오 총 가치</summary>
    public Money PortfolioValue { get; private set; }
    
    /// <summary>사용된 마진</summary>
    public Money UsedMargin { get; private set; }
    
    /// <summary>가용 마진</summary>
    public Money AvailableMargin { get; private set; }
    
    // ======================== Risk Metrics ========================
    /// <summary>Value at Risk (95% confidence)</summary>
    public Money ValueAtRisk95 { get; private set; }
    
    /// <summary>Expected Shortfall (CVaR)</summary>
    public Money ExpectedShortfall { get; private set; }
    
    /// <summary>Sharpe Ratio</summary>
    public decimal SharpeRatio { get; private set; }
    
    /// <summary>현재 낙폭</summary>
    public decimal CurrentDrawdown { get; private set; }
    
    /// <summary>최대 낙폭</summary>
    public decimal MaxDrawdown { get; private set; }
    
    /// <summary>노출 비율</summary>
    public decimal ExposureRatio { get; private set; }
    
    // ======================== Alerts ========================
    public IReadOnlyList<RiskAlert> ActiveAlerts => _activeAlerts.AsReadOnly();
    
    public int AlertCount => _activeAlerts.Count;
    
    // ======================== Factory Methods ========================
    
    /// <summary>
    /// 새 리스크 모니터 생성
    /// </summary>
    public static RiskMonitor Create(
        AccountNumber accountNumber,
        RiskProfileId riskProfileId)
    {
        var monitor = new RiskMonitor
        {
            Id = RiskMonitorId.New(),
            AccountNumber = accountNumber,
            RiskProfileId = riskProfileId,
            Status = MonitoringStatus.Initializing,
            LastUpdatedAt = DateTime.UtcNow
        };
        
        monitor.AddDomainEvent(new RiskMonitorCreatedEvent(
            monitor.Id, accountNumber, riskProfileId, DateTime.UtcNow));
        
        return monitor;
    }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 리스크 메트릭 업데이트
    /// </summary>
    public void UpdateMetrics(
        PortfolioMetrics metrics,
        RiskProfile riskProfile)
    {
        PortfolioValue = metrics.TotalValue;
        UsedMargin = metrics.UsedMargin;
        AvailableMargin = metrics.AvailableMargin;
        
        ValueAtRisk95 = metrics.ValueAtRisk95;
        ExpectedShortfall = metrics.ExpectedShortfall;
        SharpeRatio = metrics.SharpeRatio;
        CurrentDrawdown = metrics.CurrentDrawdown;
        MaxDrawdown = Math.Max(MaxDrawdown, metrics.CurrentDrawdown);
        ExposureRatio = metrics.ExposureRatio;
        
        LastUpdatedAt = DateTime.UtcNow;
        
        // 리스크 평가 및 경고
        EvaluateRisks(metrics, riskProfile);
    }
    
    /// <summary>
    /// 포지션 모니터링
    /// </summary>
    public void MonitorPosition(
        Position position,
        MarketPrice currentPrice,
        RiskProfile riskProfile)
    {
        // 손절 확인
        if (position.StopLossPrice.HasValue && 
            ShouldTriggerStopLoss(position, currentPrice))
        {
            AddDomainEvent(new StopLossTriggerEvent(
                Id,
                position.Id,
                position.EntryPrice,
                currentPrice,
                position.UnrealizedLoss,
                DateTime.UtcNow));
        }
        
        // 익절 확인
        if (position.TakeProfitPrice.HasValue && 
            ShouldTriggerTakeProfit(position, currentPrice))
        {
            AddDomainEvent(new TakeProfitTriggerEvent(
                Id,
                position.Id,
                position.EntryPrice,
                currentPrice,
                position.UnrealizedProfit,
                DateTime.UtcNow));
        }
    }
    
    /// <summary>
    /// 포트폴리오 스냅샷 기록
    /// </summary>
    public void RecordSnapshot(PortfolioMetrics metrics)
    {
        var snapshot = new PortfolioSnapshot(
            PortfolioSnapshotId.New(),
            DateTime.UtcNow,
            metrics.TotalValue,
            metrics.CurrentDrawdown,
            metrics.SharpeRatio);
        
        _snapshots.Add(snapshot);
        
        // 최근 1000개만 유지
        if (_snapshots.Count > 1000)
        {
            _snapshots.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 리스크 평가 및 경고 생성
    /// </summary>
    private void EvaluateRisks(PortfolioMetrics metrics, RiskProfile riskProfile)
    {
        // 1. VaR 초과 확인
        if (metrics.ValueAtRisk95 > riskProfile.MaxValueAtRisk)
        {
            AddAlert(new RiskAlert(
                RiskAlertId.New(),
                RiskAlertType.VaRExceeded,
                AlertSeverity.Critical,
                $"VaR {metrics.ValueAtRisk95:C}이 한도 {riskProfile.MaxValueAtRisk:C}를 초과",
                DateTime.UtcNow));
        }
        
        // 2. 낙폭 초과 확인
        if (metrics.CurrentDrawdown > riskProfile.MaxDrawdown)
        {
            AddAlert(new RiskAlert(
                RiskAlertId.New(),
                RiskAlertType.DrawdownExceeded,
                AlertSeverity.High,
                $"낙폭 {metrics.CurrentDrawdown:P}이 한도 {riskProfile.MaxDrawdown:P}를 초과",
                DateTime.UtcNow));
        }
        
        // 3. 노출 비율 초과 확인
        if (metrics.ExposureRatio > riskProfile.MaxExposureRatio)
        {
            AddAlert(new RiskAlert(
                RiskAlertId.New(),
                RiskAlertType.HighExposure,
                AlertSeverity.High,
                $"노출 비율 {metrics.ExposureRatio:P}이 한도 {riskProfile.MaxExposureRatio:P}를 초과",
                DateTime.UtcNow));
        }
        
        // 4. 마진 부족 확인
        if (metrics.AvailableMargin < metrics.UsedMargin * 0.2m)
        {
            AddAlert(new RiskAlert(
                RiskAlertId.New(),
                RiskAlertType.LowMargin,
                AlertSeverity.Medium,
                $"가용 마진 {metrics.AvailableMargin:C}이 위험 수준",
                DateTime.UtcNow));
        }
    }
    
    /// <summary>
    /// 경고 추가
    /// </summary>
    private void AddAlert(RiskAlert alert)
    {
        _activeAlerts.Add(alert);
        
        AddDomainEvent(new RiskAlertGeneratedEvent(
            Id, alert.Type, alert.Message, alert.Severity, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 손절 트리거 확인
    /// </summary>
    private bool ShouldTriggerStopLoss(Position position, MarketPrice currentPrice)
    {
        if (!position.StopLossPrice.HasValue)
            return false;
        
        if (position.Side == OrderSide.Buy)
            return currentPrice.Value <= position.StopLossPrice.Value;
        else
            return currentPrice.Value >= position.StopLossPrice.Value;
    }
    
    /// <summary>
    /// 익절 트리거 확인
    /// </summary>
    private bool ShouldTriggerTakeProfit(Position position, MarketPrice currentPrice)
    {
        if (!position.TakeProfitPrice.HasValue)
            return false;
        
        if (position.Side == OrderSide.Buy)
            return currentPrice.Value >= position.TakeProfitPrice.Value;
        else
            return currentPrice.Value <= position.TakeProfitPrice.Value;
    }
}
```

#### 2.1.3 EmergencyControl Aggregate (긴급 통제)

```csharp
/// <summary>
/// 긴급 통제 Aggregate Root
/// 킬 스위치와 Circuit Breaker 관리
/// </summary>
public class EmergencyControl : AggregateRoot<EmergencyControlId>
{
    // ======================== Identity ========================
    public EmergencyControlId Id { get; private set; }
    public AccountNumber AccountNumber { get; private set; }
    
    // ======================== Kill Switch ========================
    /// <summary>킬 스위치 활성화 여부</summary>
    public bool IsKillSwitchActive { get; private set; }
    
    /// <summary>킬 스위치 활성화 시간</summary>
    public DateTime? KillSwitchActivatedAt { get; private set; }
    
    /// <summary>킬 스위치 활성화 사유</summary>
    public string KillSwitchReason { get; private set; }
    
    /// <summary>킬 스위치 권한자</summary>
    public string KillSwitchAuthorizedBy { get; private set; }
    
    // ======================== Circuit Breaker ========================
    /// <summary>Circuit Breaker 활성화 여부</summary>
    public bool IsCircuitBreakerActive { get; private set; }
    
    /// <summary>연속 손실 횟수</summary>
    public int ConsecutiveLosses { get; private set; }
    
    /// <summary>최대 연속 손실 허용값</summary>
    public int MaxConsecutiveLosses { get; private set; }
    
    /// <summary>최대 손실 한도</summary>
    public Money MaxLossThreshold { get; private set; }
    
    /// <summary>현재 손실액</summary>
    public Money CurrentLoss { get; private set; }
    
    /// <summary>최대 낙폭 한도</summary>
    public decimal MaxDrawdownThreshold { get; private set; }
    
    /// <summary>현재 낙폭</summary>
    public decimal CurrentDrawdown { get; private set; }
    
    // ======================== Recovery ========================
    /// <summary>복구 대기 시간</summary>
    public TimeSpan CooldownPeriod { get; private set; }
    
    /// <summary>복구 가능 시간</summary>
    public DateTime? CanRecoverAt { get; private set; }
    
    // ======================== Status ========================
    public DateTime CreatedAt { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    
    // ======================== Factory Methods ========================
    
    /// <summary>
    /// 새 긴급 통제 생성
    /// </summary>
    public static EmergencyControl Create(AccountNumber accountNumber)
    {
        var control = new EmergencyControl
        {
            Id = EmergencyControlId.New(),
            AccountNumber = accountNumber,
            IsKillSwitchActive = false,
            IsCircuitBreakerActive = false,
            ConsecutiveLosses = 0,
            MaxConsecutiveLosses = 3,
            MaxLossThreshold = Money.Zero,
            CurrentLoss = Money.Zero,
            MaxDrawdownThreshold = 0.2m,
            CurrentDrawdown = 0,
            CooldownPeriod = TimeSpan.FromHours(24),
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
        
        control.AddDomainEvent(new EmergencyControlCreatedEvent(
            control.Id, accountNumber, DateTime.UtcNow));
        
        return control;
    }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 킬 스위치 활성화
    /// </summary>
    public void ActivateKillSwitch(string reason, string authorizedBy)
    {
        if (IsKillSwitchActive)
            return;
        
        IsKillSwitchActive = true;
        KillSwitchActivatedAt = DateTime.UtcNow;
        KillSwitchReason = reason;
        KillSwitchAuthorizedBy = authorizedBy;
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KillSwitchActivatedEvent(
            Id, reason, authorizedBy, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 킬 스위치 해제
    /// </summary>
    public Result DeactivateKillSwitch(string authorizedBy)
    {
        if (!IsKillSwitchActive)
            return Result.Failure("킬 스위치가 활성화되어 있지 않습니다");
        
        if (DateTime.UtcNow - KillSwitchActivatedAt < CooldownPeriod)
            return Result.Failure(
                $"쿨다운 기간 중입니다. {CooldownPeriod.TotalHours:F1}시간 후 해제 가능");
        
        IsKillSwitchActive = false;
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new KillSwitchDeactivatedEvent(
            Id, authorizedBy, DateTime.UtcNow));
        
        return Result.Success();
    }
    
    /// <summary>
    /// Circuit Breaker 확인
    /// </summary>
    public void CheckCircuitBreaker(
        int losses,
        Money totalLoss,
        decimal drawdown)
    {
        ConsecutiveLosses = losses;
        CurrentLoss = totalLoss;
        CurrentDrawdown = drawdown;
        
        // Circuit Breaker 조건 확인
        if (losses >= MaxConsecutiveLosses ||
            totalLoss <= MaxLossThreshold ||
            drawdown >= MaxDrawdownThreshold)
        {
            ActivateCircuitBreaker(
                $"Circuit Breaker Triggered: Losses={losses}, Loss={totalLoss:C}, Drawdown={drawdown:P}",
                "System");
        }
    }
    
    /// <summary>
    /// Circuit Breaker 활성화
    /// </summary>
    private void ActivateCircuitBreaker(string reason, string authorizedBy)
    {
        if (IsCircuitBreakerActive)
            return;
        
        IsCircuitBreakerActive = true;
        CanRecoverAt = DateTime.UtcNow.Add(CooldownPeriod);
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CircuitBreakerActivatedEvent(
            Id, reason, DateTime.UtcNow));
    }
    
    /// <summary>
    /// Circuit Breaker 해제
    /// </summary>
    public Result DeactivateCircuitBreaker(string authorizedBy)
    {
        if (!IsCircuitBreakerActive)
            return Result.Failure("Circuit Breaker가 활성화되어 있지 않습니다");
        
        if (DateTime.UtcNow < CanRecoverAt)
            return Result.Failure(
                $"아직 복구 대기 중입니다. {(CanRecoverAt - DateTime.UtcNow).Value.TotalHours:F1}시간 후 복구 가능");
        
        IsCircuitBreakerActive = false;
        ConsecutiveLosses = 0;
        CurrentLoss = Money.Zero;
        LastUpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CircuitBreakerDeactivatedEvent(
            Id, authorizedBy, DateTime.UtcNow));
        
        return Result.Success();
    }
    
    /// <summary>
    /// 손실 기록
    /// </summary>
    public void RecordLoss(Money loss)
    {
        CurrentLoss += loss;
        LastUpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 리셋
    /// </summary>
    public void Reset()
    {
        ConsecutiveLosses = 0;
        CurrentLoss = Money.Zero;
        CurrentDrawdown = 0;
        IsCircuitBreakerActive = false;
        LastUpdatedAt = DateTime.UtcNow;
    }
}
```

### 2.2 Value Objects

```csharp
/// <summary>리스크 프로필 ID</summary>
public sealed class RiskProfileId : StronglyTypedId<Guid>
{
    public RiskProfileId(Guid value) : base(value) { }
    public static RiskProfileId New() => new(Guid.NewGuid());
}

/// <summary>리스크 모니터 ID</summary>
public sealed class RiskMonitorId : StronglyTypedId<Guid>
{
    public RiskMonitorId(Guid value) : base(value) { }
    public static RiskMonitorId New() => new(Guid.NewGuid());
}

/// <summary>긴급 통제 ID</summary>
public sealed class EmergencyControlId : StronglyTypedId<Guid>
{
    public EmergencyControlId(Guid value) : base(value) { }
    public static EmergencyControlId New() => new(Guid.NewGuid());
}

/// <summary>손절 설정</summary>
public sealed class StopLossConfiguration : ValueObject
{
    public StopLossType Type { get; }
    public decimal Value { get; }
    
    public StopLossConfiguration(StopLossType type, decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("Stop loss value must be positive");
        
        Type = type;
        Value = value;
    }
    
    /// <summary>백분율 기반 손절</summary>
    public static StopLossConfiguration Percentage(decimal percent)
        => new(StopLossType.Percentage, percent);
    
    /// <summary>고정금액 기반 손절</summary>
    public static StopLossConfiguration Fixed(decimal amount)
        => new(StopLossType.Fixed, amount);
    
    /// <summary>ATR 기반 손절</summary>
    public static StopLossConfiguration ATRBased(decimal multiplier)
        => new(StopLossType.ATR, multiplier);
    
    public decimal CalculateStopPrice(decimal entryPrice, OrderSide side)
    {
        var adjustment = Type switch
        {
            StopLossType.Percentage => entryPrice * (Value / 100),
            StopLossType.Fixed => Value,
            _ => 0
        };
        
        return side == OrderSide.Buy 
            ? entryPrice - adjustment 
            : entryPrice + adjustment;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Value;
    }
}

/// <summary>익절 설정</summary>
public sealed class TakeProfitConfiguration : ValueObject
{
    public TakeProfitType Type { get; }
    public decimal Value { get; }
    
    public TakeProfitConfiguration(TakeProfitType type, decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("Take profit value must be positive");
        
        Type = type;
        Value = value;
    }
    
    /// <summary>백분율 기반 익절</summary>
    public static TakeProfitConfiguration Percentage(decimal percent)
        => new(TakeProfitType.Percentage, percent);
    
    /// <summary>고정금액 기반 익절</summary>
    public static TakeProfitConfiguration Fixed(decimal amount)
        => new(TakeProfitType.Fixed, amount);
    
    public decimal CalculateProfitPrice(decimal entryPrice, OrderSide side)
    {
        var adjustment = Type == TakeProfitType.Percentage 
            ? entryPrice * (Value / 100) 
            : Value;
        
        return side == OrderSide.Buy 
            ? entryPrice + adjustment 
            : entryPrice - adjustment;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Value;
    }
}

/// <summary>리스크 평가 결과</summary>
public sealed class RiskAssessment : ValueObject
{
    public PositionRequest Request { get; }
    public bool IsApproved { get; private set; }
    public List<RiskViolation> Violations { get; }
    
    public RiskAssessment(PositionRequest request)
    {
        Request = request;
        Violations = new List<RiskViolation>();
        IsApproved = true;
    }
    
    public void AddViolation(RiskViolation violation)
    {
        Violations.Add(violation);
    }
    
    public void DetermineApproval()
    {
        IsApproved = !Violations.Any();
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Request;
        yield return IsApproved;
        yield return Violations.Count;
    }
}

/// <summary>리스크 위반</summary>
public sealed class RiskViolation : ValueObject
{
    public RiskViolationType Type { get; }
    public string Message { get; }
    
    public RiskViolation(RiskViolationType type, string message)
    {
        Type = type;
        Message = message;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Message;
    }
}

/// <summary>리스크 경고</summary>
public sealed class RiskAlert : ValueObject
{
    public RiskAlertId Id { get; }
    public RiskAlertType Type { get; }
    public AlertSeverity Severity { get; }
    public string Message { get; }
    public DateTime GeneratedAt { get; }
    
    public RiskAlert(
        RiskAlertId id,
        RiskAlertType type,
        AlertSeverity severity,
        string message,
        DateTime generatedAt)
    {
        Id = id;
        Type = type;
        Severity = severity;
        Message = message;
        GeneratedAt = generatedAt;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Type;
        yield return GeneratedAt;
    }
}

/// <summary>포트폴리오 메트릭</summary>
public sealed class PortfolioMetrics : ValueObject
{
    public Money TotalValue { get; }
    public Money UsedMargin { get; }
    public Money AvailableMargin { get; }
    public Money ValueAtRisk95 { get; }
    public Money ExpectedShortfall { get; }
    public decimal SharpeRatio { get; }
    public decimal CurrentDrawdown { get; }
    public decimal ExposureRatio { get; }
    
    public PortfolioMetrics(
        Money totalValue,
        Money usedMargin,
        Money availableMargin,
        Money valueAtRisk95,
        Money expectedShortfall,
        decimal sharpeRatio,
        decimal currentDrawdown,
        decimal exposureRatio)
    {
        TotalValue = totalValue;
        UsedMargin = usedMargin;
        AvailableMargin = availableMargin;
        ValueAtRisk95 = valueAtRisk95;
        ExpectedShortfall = expectedShortfall;
        SharpeRatio = sharpeRatio;
        CurrentDrawdown = currentDrawdown;
        ExposureRatio = exposureRatio;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TotalValue;
        yield return SharpeRatio;
        yield return CurrentDrawdown;
    }
}
```

### 2.3 Enumerations

```csharp
/// <summary>리스크 프로필 상태</summary>
public enum RiskProfileStatus
{
    Draft = 1,
    Active = 2,
    Inactive = 3,
    Archived = 4
}

/// <summary>모니터링 상태</summary>
public enum MonitoringStatus
{
    Initializing = 1,
    Running = 2,
    Paused = 3,
    Stopped = 4
}

/// <summary>리스크 위반 유형</summary>
public enum RiskViolationType
{
    PositionSizeExceeded = 1,
    MaxPositionsExceeded = 2,
    ExposureRatioExceeded = 3,
    DailyLossExceeded = 4,
    VaRExceeded = 5,
    MissingStopLoss = 6,
    LeverageExceeded = 7,
    MarginInsufficient = 8
}

/// <summary>리스크 경고 타입</summary>
public enum RiskAlertType
{
    HighRisk = 1,
    VaRExceeded = 2,
    DrawdownExceeded = 3,
    HighExposure = 4,
    LowMargin = 5,
    LowBalance = 6,
    StopLossTriggered = 7,
    TakeProfitTriggered = 8
}

/// <summary>경고 심각도</summary>
public enum AlertSeverity
{
    Info = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>손절 유형</summary>
public enum StopLossType
{
    Percentage = 1,
    Fixed = 2,
    ATR = 3,
    Support = 4,
    TrailingStop = 5
}

/// <summary>익절 유형</summary>
public enum TakeProfitType
{
    Percentage = 1,
    Fixed = 2,
    MultiLevel = 3,
    Resistance = 4
}
```

---

## 3. Business Rules & Invariants

### 3.1 핵심 불변식

```csharp
/// <summary>
/// 리스크 관리 핵심 비즈니스 규칙
/// </summary>
public static class RiskManagementInvariants
{
    /// <summary>
    /// Invariant 1: 최대 포지션 크기는 양수여야 함
    /// </summary>
    public static void ValidateMaxPositionSize(Money maxPositionSize)
    {
        if (maxPositionSize <= Money.Zero)
            throw new InvalidOperationException("최대 포지션 크기는 양수여야 합니다");
    }
    
    /// <summary>
    /// Invariant 2: 최대 오픈 포지션 수는 1 이상이어야 함
    /// </summary>
    public static void ValidateMaxOpenPositions(int maxOpenPositions)
    {
        if (maxOpenPositions < 1)
            throw new InvalidOperationException("최대 오픈 포지션 수는 1 이상이어야 합니다");
    }
    
    /// <summary>
    /// Invariant 3: 노출 비율은 0-1 범위
    /// </summary>
    public static void ValidateExposureRatio(decimal ratio)
    {
        if (ratio < 0 || ratio > 1)
            throw new InvalidOperationException("노출 비율은 0-1 사이여야 합니다");
    }
    
    /// <summary>
    /// Invariant 4: 낙폭은 0-1 범위
    /// </summary>
    public static void ValidateDrawdown(decimal drawdown)
    {
        if (drawdown < 0 || drawdown > 1)
            throw new InvalidOperationException("낙폭은 0-1 사이여야 합니다");
    }
    
    /// <summary>
    /// Invariant 5: 손절은 진입가와 다른 가격
    /// </summary>
    public static void ValidateStopLossPrice(
        decimal entryPrice,
        decimal stopLossPrice,
        OrderSide side)
    {
        if (side == OrderSide.Buy && stopLossPrice >= entryPrice)
            throw new InvalidOperationException("매수 시 손절가는 진입가보다 낮아야 합니다");
        
        if (side == OrderSide.Sell && stopLossPrice <= entryPrice)
            throw new InvalidOperationException("매도 시 손절가는 진입가보다 높아야 합니다");
    }
}
```

### 3.2 비즈니스 규칙

```csharp
/// <summary>
/// 리스크 관리 비즈니스 규칙
/// </summary>
public static class RiskManagementRules
{
    /// <summary>
    /// 규칙 1: 신호는 리스크 평가를 거쳐야 함
    /// </summary>
    public static bool RequiresRiskApproval(SignalType signalType)
    {
        return signalType == SignalType.Buy || signalType == SignalType.Sell;
    }
    
    /// <summary>
    /// 규칙 2: 손절은 진입 시에 설정되어야 함
    /// </summary>
    public static bool StopLossIsRequired(RiskProfile profile, OrderSide side)
    {
        return profile.IsStopLossMandatory;
    }
    
    /// <summary>
    /// 규칙 3: 킬 스위치 활성화 후 복구 불가
    /// </summary>
    public static bool KillSwitchIsIrreversible(EmergencyControl control)
    {
        return control.IsKillSwitchActive && 
               (DateTime.UtcNow - control.KillSwitchActivatedAt) < control.CooldownPeriod;
    }
    
    /// <summary>
    /// 규칙 4: 높은 변동성에서는 포지션 크기 감소
    /// </summary>
    public static Money AdjustPositionSizeForVolatility(
        Money baseSize,
        decimal impliedVolatility,
        decimal volatilityThreshold = 0.3m)
    {
        if (impliedVolatility > volatilityThreshold)
        {
            var adjustment = 1 - ((impliedVolatility - volatilityThreshold) * 0.5m);
            return baseSize * adjustment;
        }
        
        return baseSize;
    }
}
```

---

## 4. Domain Events

```csharp
/// <summary>리스크 프로필 생성 이벤트</summary>
public sealed class RiskProfileCreatedEvent : DomainEvent
{
    public RiskProfileId RiskProfileId { get; }
    public AccountNumber AccountNumber { get; }
    public string ProfileName { get; }
    public DateTime CreatedAt { get; }
    
    public RiskProfileCreatedEvent(
        RiskProfileId riskProfileId,
        AccountNumber accountNumber,
        string profileName,
        DateTime createdAt)
    {
        RiskProfileId = riskProfileId;
        AccountNumber = accountNumber;
        ProfileName = profileName;
        CreatedAt = createdAt;
    }
}

/// <summary>리스크 한도 위반 이벤트</summary>
public sealed class RiskLimitViolatedEvent : DomainEvent
{
    public RiskProfileId RiskProfileId { get; }
    public PositionRequest Request { get; }
    public List<RiskViolation> Violations { get; }
    public DateTime ViolatedAt { get; }
    
    public RiskLimitViolatedEvent(
        RiskProfileId riskProfileId,
        PositionRequest request,
        List<RiskViolation> violations,
        DateTime violatedAt)
    {
        RiskProfileId = riskProfileId;
        Request = request;
        Violations = violations;
        ViolatedAt = violatedAt;
    }
}

/// <summary>손절 트리거 이벤트</summary>
public sealed class StopLossTriggerEvent : DomainEvent
{
    public RiskMonitorId RiskMonitorId { get; }
    public PositionId PositionId { get; }
    public decimal EntryPrice { get; }
    public decimal CurrentPrice { get; }
    public Money Loss { get; }
    public DateTime TriggeredAt { get; }
    
    public StopLossTriggerEvent(
        RiskMonitorId riskMonitorId,
        PositionId positionId,
        decimal entryPrice,
        decimal currentPrice,
        Money loss,
        DateTime triggeredAt)
    {
        RiskMonitorId = riskMonitorId;
        PositionId = positionId;
        EntryPrice = entryPrice;
        CurrentPrice = currentPrice;
        Loss = loss;
        TriggeredAt = triggeredAt;
    }
}

/// <summary>킬 스위치 활성화 이벤트</summary>
public sealed class KillSwitchActivatedEvent : DomainEvent
{
    public EmergencyControlId EmergencyControlId { get; }
    public string Reason { get; }
    public string AuthorizedBy { get; }
    public DateTime ActivatedAt { get; }
    
    public KillSwitchActivatedEvent(
        EmergencyControlId emergencyControlId,
        string reason,
        string authorizedBy,
        DateTime activatedAt)
    {
        EmergencyControlId = emergencyControlId;
        Reason = reason;
        AuthorizedBy = authorizedBy;
        ActivatedAt = activatedAt;
    }
}

/// <summary>리스크 경고 생성 이벤트</summary>
public sealed class RiskAlertGeneratedEvent : DomainEvent
{
    public RiskMonitorId RiskMonitorId { get; }
    public RiskAlertType AlertType { get; }
    public string Message { get; }
    public AlertSeverity Severity { get; }
    public DateTime GeneratedAt { get; }
    
    public RiskAlertGeneratedEvent(
        RiskMonitorId riskMonitorId,
        RiskAlertType alertType,
        string message,
        AlertSeverity severity,
        DateTime generatedAt)
    {
        RiskMonitorId = riskMonitorId;
        AlertType = alertType;
        Message = message;
        Severity = severity;
        GeneratedAt = generatedAt;
    }
}
```

---

## 5. Domain Services

```csharp
/// <summary>리스크 평가 도메인 서비스</summary>
public interface IRiskAssessmentService : IDomainService
{
    Task<RiskAssessment> AssessPositionAsync(
        RiskProfile riskProfile,
        PositionRequest request,
        CurrentPortfolio portfolio,
        MarketData marketData);
    
    Task<Money> CalculateOptimalPositionSizeAsync(
        TradingSignal signal,
        CurrentPortfolio portfolio,
        MarketVolatility volatility);
}

/// <summary>포지션 모니터링 도메인 서비스</summary>
public interface IPositionMonitoringService : IDomainService
{
    Task MonitorPositionsAsync(
        IEnumerable<Position> positions,
        IEnumerable<MarketPrice> currentPrices,
        RiskProfile riskProfile);
    
    Task<List<Position>> FindRiskyPositionsAsync(
        IEnumerable<Position> positions,
        RiskProfile riskProfile);
}

/// <summary>긴급 통제 도메인 서비스</summary>
public interface IEmergencyControlService : IDomainService
{
    Task CheckCircuitBreakerAsync(
        EmergencyControl control,
        TradingStatistics statistics);
    
    Task<bool> ShouldBlockTradingAsync(
        EmergencyControl control);
}
```

---

## 6. Application Services

```csharp
/// <summary>리스크 관리 애플리케이션 서비스</summary>
public sealed class RiskManagementService : IApplicationService
{
    private readonly IRiskAssessmentService _assessmentService;
    private readonly IPositionMonitoringService _monitoringService;
    private readonly IRiskProfileRepository _riskProfileRepository;
    private readonly IRiskMonitorRepository _riskMonitorRepository;
    private readonly IEmergencyControlRepository _emergencyControlRepository;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RiskManagementService> _logger;
    
    /// <summary>포지션 리스크 평가</summary>
    public async Task<Result<RiskAssessmentDto>> AssessPositionRiskAsync(
        Guid riskProfileId,
        PositionRequest request,
        CurrentPortfolio portfolio,
        MarketData marketData)
    {
        try
        {
            var riskProfile = await _riskProfileRepository.GetByIdAsync(
                new RiskProfileId(riskProfileId));
            
            if (riskProfile == null)
                return Result<RiskAssessmentDto>.Failure("Risk profile not found");
            
            var assessment = riskProfile.AssessNewPosition(request, portfolio, marketData);
            
            await _unitOfWork.CommitAsync();
            
            foreach (var @event in riskProfile.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return Result<RiskAssessmentDto>.Success(MapToDto(assessment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assess position risk");
            return Result<RiskAssessmentDto>.Failure(
                $"Risk assessment failed: {ex.Message}");
        }
    }
    
    /// <summary>포지션 모니터링</summary>
    public async Task<r> MonitorPositionsAsync(
        Guid riskMonitorId,
        List<Position> positions,
        List<MarketPrice> currentPrices)
    {
        try
        {
            var riskMonitor = await _riskMonitorRepository.GetByIdAsync(
                new RiskMonitorId(riskMonitorId));
            
            if (riskMonitor == null)
                return Result.Failure("Risk monitor not found");
            
            var riskProfile = await _riskProfileRepository.GetByIdAsync(
                riskMonitor.RiskProfileId);
            
            // 포지션별 모니터링
            for (int i = 0; i < positions.Count; i++)
            {
                var position = positions[i];
                var currentPrice = currentPrices[i];
                riskMonitor.MonitorPosition(position, currentPrice, riskProfile);
            }
            
            await _riskMonitorRepository.UpdateAsync(riskMonitor);
            await _unitOfWork.CommitAsync();
            
            foreach (var @event in riskMonitor.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to monitor positions");
            return Result.Failure($"Position monitoring failed: {ex.Message}");
        }
    }
    
    private RiskAssessmentDto MapToDto(RiskAssessment assessment)
    {
        return new RiskAssessmentDto
        {
            IsApproved = assessment.IsApproved,
            ViolationCount = assessment.Violations.Count,
            Violations = assessment.Violations
                .Select(v => new RiskViolationDto
                {
                    Type = v.Type.ToString(),
                    Message = v.Message
                })
                .ToList()
        };
    }
}
```

---

## 7. Infrastructure Layer

```csharp
/// <summary>리스크 프로필 저장소 구현</summary>
public sealed class RiskProfileRepository : IRiskProfileRepository
{
    private readonly RiskDbContext _context;
    private readonly IDistributedCache _cache;
    
    public async Task<RiskProfile> GetByIdAsync(RiskProfileId id)
    {
        var cacheKey = $"riskprofile:{id}";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        if (cachedData != null)
            return JsonSerializer.Deserialize<RiskProfile>(cachedData);
        
        var entity = await _context.RiskProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (entity != null)
        {
            var json = JsonSerializer.Serialize(entity);
            await _cache.SetStringAsync(cacheKey, json, TimeSpan.FromMinutes(5));
        }
        
        return entity;
    }
    
    public async Task AddAsync(RiskProfile riskProfile)
    {
        _context.RiskProfiles.Add(riskProfile);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(RiskProfile riskProfile)
    {
        _context.RiskProfiles.Update(riskProfile);
        await _context.SaveChangesAsync();
        
        var cacheKey = $"riskprofile:{riskProfile.Id}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

---

## 8. API/Interface Specifications

```csharp
[ApiController]
[Route("api/risk-management")]
[Authorize]
public class RiskManagementController : ControllerBase
{
    private readonly IMediator _mediator;
    
    /// <summary>POST /api/risk-management/assess - 포지션 리스크 평가</summary>
    [HttpPost("assess")]
    public async Task<ActionResult<RiskAssessmentDto>> AssessPositionRiskAsync(
        [FromBody] AssessPositionRiskRequest request)
    {
        var command = new AssessPositionRiskCommand(
            request.RiskProfileId,
            request.StockCode,
            request.Side,
            request.Quantity,
            request.EntryPrice,
            request.StopLossPrice);
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
    
    /// <summary>POST /api/risk-management/kill-switch/activate - 킬 스위치 활성화</summary>
    [HttpPost("kill-switch/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateKillSwitchAsync(
        [FromBody] ActivateKillSwitchRequest request)
    {
        var command = new ActivateKillSwitchCommand(
            request.EmergencyControlId,
            request.Reason);
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok();
    }
}
```

---

## 9. Risk Management Strategies

### 9.1 포지션 사이징 전략

```csharp
/// <summary>Kelly Criterion 기반 포지션 사이징</summary>
public class KellyCriterionPositionSizing : IPositionSizingStrategy
{
    public Money Calculate(
        RiskProfile riskProfile,
        TradingSignal signal,
        CurrentPortfolio portfolio,
        MarketVolatility volatility)
    {
        var winProbability = signal.WinRate;
        var riskRewardRatio = signal.RiskRewardRatio;
        
        // Kelly Formula
        var b = riskRewardRatio;
        var q = 1 - winProbability;
        var kellyFraction = ((winProbability * b) - q) / b;
        
        // Conservative Kelly (25%)
        var conservativeKelly = kellyFraction * 0.25m;
        
        var positionSize = portfolio.TotalEquity * conservativeKelly;
        
        return Money.Min(positionSize, riskProfile.MaxPositionSize);
    }
}

/// <summary>고정 리스크 기반 포지션 사이징</summary>
public class FixedRiskPositionSizing : IPositionSizingStrategy
{
    private readonly decimal _riskPerTrade = 0.02m; // 2%
    
    public Money Calculate(
        RiskProfile riskProfile,
        TradingSignal signal,
        CurrentPortfolio portfolio,
        MarketVolatility volatility)
    {
        var riskAmount = portfolio.TotalEquity * _riskPerTrade;
        var stopLossDistance = Math.Abs(signal.StopLossPrice - signal.EntryPrice);
        
        if (stopLossDistance == 0)
            return riskProfile.MaxPositionSize;
        
        var positionValue = riskAmount / stopLossDistance * signal.EntryPrice;
        
        return Money.Min(positionValue, riskProfile.MaxPositionSize);
    }
}
```

### 9.2 손절 전략

```csharp
/// <summary>ATR 기반 손절</summary>
public class ATRBasedStopLoss : IStopLossStrategy
{
    private readonly decimal _atrMultiplier = 2.0m;
    
    public decimal CalculateStopPrice(
        Position position,
        MarketPrice currentPrice,
        TechnicalIndicators indicators)
    {
        var atr = indicators.ATR(14);
        var stopDistance = atr * _atrMultiplier;
        
        if (position.Side == OrderSide.Buy)
        {
            var trailingStop = currentPrice.Value - stopDistance;
            return Math.Max(trailingStop, position.StopLossPrice ?? 0);
        }
        else
        {
            var trailingStop = currentPrice.Value + stopDistance;
            return Math.Min(trailingStop, position.StopLossPrice ?? decimal.MaxValue);
        }
    }
}
```

---

## 10. Monitoring & Alerts

```csharp
/// <summary>리스크 알림 서비스</summary>
public class RiskAlertService
{
    private readonly INotificationService _notificationService;
    
    public async Task SendRiskAlertAsync(RiskAlert alert)
    {
        var message = FormatAlertMessage(alert);
        
        switch (alert.Severity)
        {
            case AlertSeverity.Critical:
                await _notificationService.SendSMS(message);
                await _notificationService.SendEmail(message);
                await _notificationService.SendPushNotification(message);
                break;
                
            case AlertSeverity.High:
                await _notificationService.SendEmail(message);
                await _notificationService.SendPushNotification(message);
                break;
                
            case AlertSeverity.Medium:
                await _notificationService.SendPushNotification(message);
                break;
        }
    }
    
    private string FormatAlertMessage(RiskAlert alert)
    {
        return $"[{alert.Severity}] {alert.Type}: {alert.Message}";
    }
}
```

---

## 11. Error Handling & Validation

```csharp
public sealed class RiskProfileNotFoundException : DomainException
{
    public RiskProfileId RiskProfileId { get; }
    
    public RiskProfileNotFoundException(string message, RiskProfileId id = null)
        : base(message) { RiskProfileId = id; }
}

public sealed class RiskLimitExceededException : DomainException
{
    public RiskViolationType ViolationType { get; }
    
    public RiskLimitExceededException(string message, RiskViolationType violationType)
        : base(message) { ViolationType = violationType; }
}

public sealed class KillSwitchActiveException : DomainException
{
    public KillSwitchActiveException(string message)
        : base(message) { }
}
```

---

## 12. Performance Considerations

### 캐싱 전략
- L1: Redis (5분 TTL) - 활성 리스크 프로필
- L2: Database - 완전 데이터

### 실시간 모니터링
- 초당 포지션 모니터링
- 손절/익절 즉시 트리거
- 알림 배치 처리 (100개 단위)

### 배치 처리
- 일일 리스크 리포트 생성
- 주간 메트릭 계산
- 월간 성과 분석

---

## 결론

이 Risk Management Context 설계는 다음 특징을 갖습니다:

✅ **완전성**: 리스크 관리의 모든 측면
✅ **실시간성**: 즉각적인 위험 감지
✅ **자동화**: 자동화된 손절/익절 실행
✅ **안전성**: 킬 스위치와 Circuit Breaker
✅ **투명성**: 모든 리스크 지표 추적
✅ **확장성**: 다양한 포지션 사이징 전략
✅ **통합성**: 다른 Context와의 원활한 연동
