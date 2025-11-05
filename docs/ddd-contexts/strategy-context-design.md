# Strategy Context - DDD 상세 설계

## 📋 목차
1. [Context Overview](#1-context-overview)
2. [Domain Model 상세 설계](#2-domain-model-상세-설계)
3. [Business Rules & Invariants](#3-business-rules--invariants)
4. [Domain Events](#4-domain-events)
5. [Domain Services](#5-domain-services)
6. [Application Services](#6-application-services)
7. [Infrastructure Layer](#7-infrastructure-layer)
8. [API/Interface Specifications](#8-apiinterface-specifications)
9. [Strategy Execution Flow](#9-strategy-execution-flow)
10. [Backtesting & Optimization](#10-backtesting--optimization)
11. [Error Handling & Validation](#11-error-handling--validation)
12. [Performance Considerations](#12-performance-considerations)

---

## 1. Context Overview

### 1.1 책임과 범위

**Strategy Context**는 매매 전략의 정의, 검증, 신호 생성, 백테스팅, 최적화를 담당하는 핵심 Bounded Context입니다.

#### 주요 책임
- ✅ 거래 전략 정의 및 관리
- ✅ 기술적 지표 기반 신호 생성
- ✅ 실시간 신호 평가 및 실행
- ✅ 전략 백테스팅
- ✅ 매개변수 최적화
- ✅ 성과 분석 및 보고
- ✅ 전략 검증 및 리스크 평가
- ✅ 전략 버전 관리

#### 핵심 원칙
- **신뢰성**: 모든 신호는 검증되어야 함
- **신호성**: 신호의 명확한 근거 제시
- **백테스트**: 실제 거래 전에 검증
- **최적화**: 지속적인 성능 개선
- **투명성**: 모든 전략 로직 문서화
- **회수가능성**: 전략 변경 이력 관리

### 1.2 Context Map

```
┌─────────────────────────────────┐
│     Strategy Context            │
│   (거래 전략 관리 & 신호)       │
└─────────────────────────────────┘
    ↓ 신호 생성    ↑ 신호 실행
    │             │
    ↓             ↑
Market Data    Trading Context
Context        (거래 실행)
    ↑             ↑
    │ 시세 데이터 │
    │             │
    ├─────────────┤
    
    ↓ 전략 평가
    
Risk Management Context
(리스크 검증)
```

### 1.3 바운더리

**이 Context가 담당하는 것:**
- 전략 정의 및 관리
- 신호 생성 로직
- 지표 계산 및 평가
- 백테스팅 실행
- 매개변수 최적화

**이 Context가 담당하지 않는 것:**
- 실시간 시세 (Market Data Context)
- 주문 실행 (Trading Context)
- 리스크 한도 (Risk Management Context)
- 계좌 관리 (Account Context)
- 모니터링 알림 (Monitoring Context)

---

## 2. Domain Model 상세 설계

### 2.1 Aggregates

#### 2.1.1 TradingStrategy Aggregate (거래 전략)

**Aggregate Root**

```csharp
/// <summary>
/// 거래 전략 Aggregate Root
/// 전략의 전체 라이프사이클을 관리
/// </summary>
public class TradingStrategy : AggregateRoot<StrategyId>
{
    private readonly List<StrategySignal> _recentSignals = new();
    private readonly List<StrategyRule> _rules = new();
    private readonly Dictionary<string, object> _parameters = new();
    
    // ======================== Identity ========================
    /// <summary>전략 고유 ID</summary>
    public StrategyId Id { get; private set; }
    
    /// <summary>전략 이름</summary>
    public string Name { get; private set; }
    
    /// <summary>전략 설명</summary>
    public string Description { get; private set; }
    
    // ======================== Strategy Configuration ========================
    /// <summary>전략 유형</summary>
    public StrategyType Type { get; private set; }
    
    /// <summary>전략 카테고리</summary>
    public StrategyCategory Category { get; private set; }
    
    /// <summary>거래 스타일</summary>
    public TradingStyle TradingStyle { get; private set; }
    
    /// <summary>전략 상태</summary>
    public StrategyStatus Status { get; private set; }
    
    /// <summary>실행 모드</summary>
    public ExecutionMode ExecutionMode { get; private set; }
    
    // ======================== Timeframes & Scheduling ========================
    /// <summary>분석 시간 단위 (5분, 15분, 1시간, 1일 등)</summary>
    public TimeFrame TimeFrame { get; private set; }
    
    /// <summary>신호 생성 시간대</summary>
    public TradingTimeRange TradingHours { get; private set; }
    
    /// <summary>신호 생성 빈도</summary>
    public SignalFrequency SignalFrequency { get; private set; }
    
    // ======================== Entry/Exit Configuration ========================
    /// <summary>진입 규칙들</summary>
    public IReadOnlyList<StrategyRule> EntryRules => 
        _rules.Where(r => r.RuleType == RuleType.Entry).ToList();
    
    /// <summary>청산 규칙들</summary>
    public IReadOnlyList<StrategyRule> ExitRules => 
        _rules.Where(r => r.RuleType == RuleType.Exit).ToList();
    
    /// <summary>필터 규칙들</summary>
    public IReadOnlyList<StrategyRule> FilterRules => 
        _rules.Where(r => r.RuleType == RuleType.Filter).ToList();
    
    // ======================== Risk Management ========================
    /// <summary>리스크 프로필</summary>
    public RiskProfile RiskProfile { get; private set; }
    
    /// <summary>손절 규칙</summary>
    public StopLossRule StopLoss { get; private set; }
    
    /// <summary>익절 규칙</summary>
    public TakeProfitRule TakeProfit { get; private set; }
    
    /// <summary>포지션 사이징</summary>
    public PositionSizing PositionSizing { get; private set; }
    
    // ======================== Indicators ========================
    /// <summary>사용할 지표들</summary>
    public List<IndicatorConfiguration> Indicators { get; private set; }
    
    /// <summary>사용자 정의 지표 로직</summary>
    public string CustomIndicatorCode { get; private set; }
    
    // ======================== Parameters ========================
    /// <summary>전략 매개변수 (SMA 기간, RSI 임계값 등)</summary>
    public IReadOnlyDictionary<string, object> Parameters => 
        new Dictionary<string, object>(_parameters);
    
    // ======================== Performance ========================
    /// <summary>누적 성과</summary>
    public StrategyPerformance Performance { get; private set; }
    
    /// <summary>최근 백테스트 결과</summary>
    public BacktestResult LastBacktest { get; private set; }
    
    /// <summary>최근 최적화 결과</summary>
    public OptimizationResult LastOptimization { get; private set; }
    
    /// <summary>마지막 신호 생성 시간</summary>
    public DateTime? LastSignalAt { get; private set; }
    
    /// <summary>마지막 신호</summary>
    public StrategySignal LastSignal { get; private set; }
    
    /// <summary>최근 신호 히스토리</summary>
    public IReadOnlyList<StrategySignal> RecentSignals => 
        _recentSignals.AsReadOnly();
    
    // ======================== Metadata ========================
    /// <summary>전략 버전</summary>
    public Version Version { get; private set; }
    
    /// <summary>작성자</summary>
    public string Author { get; private set; }
    
    /// <summary>생성 시간</summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>마지막 수정 시간</summary>
    public DateTime LastModifiedAt { get; private set; }
    
    /// <summary>활성화 시간</summary>
    public DateTime? ActivatedAt { get; private set; }
    
    /// <summary>비활성화 시간</summary>
    public DateTime? DeactivatedAt { get; private set; }
    
    /// <summary>비활성화 사유</summary>
    public string DeactivationReason { get; private set; }
    
    // ======================== Constructor & Factory Methods ========================
    
    /// <summary>
    /// 새 거래 전략 생성
    /// </summary>
    public static TradingStrategy Create(
        string name,
        string description,
        StrategyType type,
        StrategyCategory category,
        TradingStyle tradingStyle,
        TimeFrame timeFrame,
        string author)
    {
        var strategy = new TradingStrategy
        {
            Id = StrategyId.New(),
            Name = name,
            Description = description,
            Type = type,
            Category = category,
            TradingStyle = tradingStyle,
            TimeFrame = timeFrame,
            Status = StrategyStatus.Draft,
            ExecutionMode = ExecutionMode.Manual,
            Author = author,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            Version = new Version(1, 0, 0),
            Performance = StrategyPerformance.Initialize(),
            RiskProfile = RiskProfile.Default(),
            Indicators = new List<IndicatorConfiguration>(),
            TradingHours = TradingTimeRange.Default()
        };
        
        strategy.AddDomainEvent(new StrategyCreatedEvent(strategy.Id, name, author, DateTime.UtcNow));
        
        return strategy;
    }
    
    /// <summary>
    /// Momentum 전략 템플릿
    /// </summary>
    public static TradingStrategy CreateMomentumStrategy(
        string name,
        TimeFrame timeFrame,
        string author)
    {
        var strategy = Create(
            name,
            "가격과 거래량의 추세를 따라가는 모멘텀 전략",
            StrategyType.Momentum,
            StrategyCategory.Technical,
            TradingStyle.SwingTrading,
            timeFrame,
            author);
        
        // 기본 지표 설정
        strategy.Indicators.Add(new IndicatorConfiguration(
            IndicatorType.EMA,
            new[] { ("period", 20), ("applyTo", "close") }));
        
        strategy.Indicators.Add(new IndicatorConfiguration(
            IndicatorType.RSI,
            new[] { ("period", 14), ("overBought", 70), ("overSold", 30) }));
        
        // 기본 매개변수
        strategy._parameters["FastEMA"] = 12;
        strategy._parameters["SlowEMA"] = 26;
        strategy._parameters["SignalLine"] = 9;
        strategy._parameters["RSIThreshold"] = 50;
        
        return strategy;
    }
    
    /// <summary>
    /// Mean Reversion 전략 템플릿
    /// </summary>
    public static TradingStrategy CreateMeanReversionStrategy(
        string name,
        TimeFrame timeFrame,
        string author)
    {
        var strategy = Create(
            name,
            "평균으로의 회귀를 이용한 평균회귀 전략",
            StrategyType.MeanReversion,
            StrategyCategory.Technical,
            TradingStyle.DayTrading,
            timeFrame,
            author);
        
        strategy.Indicators.Add(new IndicatorConfiguration(
            IndicatorType.BollingerBands,
            new[] { ("period", 20), ("stdDev", 2) }));
        
        strategy.Indicators.Add(new IndicatorConfiguration(
            IndicatorType.RSI,
            new[] { ("period", 14) }));
        
        return strategy;
    }
    
    // ======================== Domain Methods ========================
    
    /// <summary>
    /// 규칙 추가
    /// </summary>
    public void AddRule(StrategyRule rule)
    {
        if (Status == StrategyStatus.Active)
            throw new InvalidOperationException(
                "활성 전략은 규칙을 수정할 수 없습니다");
        
        _rules.Add(rule);
        Version = Version.Increment();
        LastModifiedAt = DateTime.UtcNow;
        
        AddDomainEvent(new StrategyRuleAddedEvent(Id, rule.Id, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 규칙 제거
    /// </summary>
    public void RemoveRule(StrategyRuleId ruleId)
    {
        if (Status == StrategyStatus.Active)
            throw new InvalidOperationException(
                "활성 전략은 규칙을 수정할 수 없습니다");
        
        var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null)
        {
            _rules.Remove(rule);
            Version = Version.Increment();
            LastModifiedAt = DateTime.UtcNow;
            
            AddDomainEvent(new StrategyRuleRemovedEvent(Id, ruleId, DateTime.UtcNow));
        }
    }
    
    /// <summary>
    /// 매개변수 업데이트
    /// </summary>
    public void UpdateParameter(string paramName, object value)
    {
        if (Status == StrategyStatus.Active)
            throw new InvalidOperationException(
                "활성 전략은 매개변수를 수정할 수 없습니다");
        
        _parameters[paramName] = value;
        Version = Version.Increment();
        LastModifiedAt = DateTime.UtcNow;
        
        AddDomainEvent(new StrategyParameterChangedEvent(
            Id, paramName, value, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 전략 활성화
    /// </summary>
    public Result Activate()
    {
        if (Status == StrategyStatus.Active)
            return Result.Failure("이미 활성 상태입니다");
        
        // 최소한 하나의 진입 규칙 확인
        if (!EntryRules.Any())
            return Result.Failure("진입 규칙이 없습니다");
        
        // 최소한 하나의 청산 규칙 확인
        if (!ExitRules.Any())
            return Result.Failure("청산 규칙이 없습니다");
        
        Status = StrategyStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new StrategyActivatedEvent(Id, DateTime.UtcNow));
        
        return Result.Success();
    }
    
    /// <summary>
    /// 전략 비활성화
    /// </summary>
    public void Deactivate(string reason = null)
    {
        if (Status != StrategyStatus.Active)
            return;
        
        Status = StrategyStatus.Inactive;
        DeactivatedAt = DateTime.UtcNow;
        DeactivationReason = reason;
        
        AddDomainEvent(new StrategyDeactivatedEvent(Id, reason, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 신호 기록
    /// </summary>
    public void RecordSignal(StrategySignal signal)
    {
        if (Status != StrategyStatus.Active)
            throw new InvalidOperationException("비활성 전략에서 신호를 생성할 수 없습니다");
        
        _recentSignals.Add(signal);
        
        // 최근 100개만 유지
        if (_recentSignals.Count > 100)
        {
            _recentSignals.RemoveAt(0);
        }
        
        LastSignal = signal;
        LastSignalAt = signal.GeneratedAt;
        
        AddDomainEvent(new SignalGeneratedEvent(
            Id,
            signal.Id,
            signal.Type,
            signal.StockCode,
            signal.Strength,
            signal.GeneratedAt));
    }
    
    /// <summary>
    /// 성과 업데이트
    /// </summary>
    public void UpdatePerformance(TradeResult tradeResult)
    {
        Performance = Performance.AddTrade(tradeResult);
        
        // 성과 임계값 확인 (동적 비활성화)
        ValidatePerformance();
        
        LastModifiedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 성과 검증
    /// </summary>
    private void ValidatePerformance()
    {
        // 손실이 너무 크면 비활성화
        if (Performance.TotalPnL < RiskProfile.MaxDailyLoss)
        {
            Deactivate($"일일 최대 손실 초과: {Performance.TotalPnL:C}");
        }
        
        // 낙폭이 너무 크면 비활성화
        if (Performance.MaxDrawdown > RiskProfile.MaxDrawdown)
        {
            Deactivate($"최대 낙폭 초과: {Performance.MaxDrawdown:P}");
        }
    }
    
    /// <summary>
    /// 백테스트 결과 기록
    /// </summary>
    public void RecordBacktest(BacktestResult result)
    {
        LastBacktest = result;
        LastModifiedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BacktestCompletedEvent(Id, result, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 최적화 결과 기록
    /// </summary>
    public void RecordOptimization(OptimizationResult result)
    {
        LastOptimization = result;
        
        // 최적 매개변수 자동 적용 옵션
        if (result.ShouldAutoApply && Status != StrategyStatus.Active)
        {
            foreach (var param in result.OptimalParameters)
            {
                _parameters[param.Key] = param.Value;
            }
        }
        
        LastModifiedAt = DateTime.UtcNow;
        
        AddDomainEvent(new OptimizationCompletedEvent(Id, result, DateTime.UtcNow));
    }
    
    /// <summary>
    /// 신호 신뢰도 계산
    /// </summary>
    public decimal CalculateSignalConfidence(
        Dictionary<string, decimal> indicatorValues,
        MarketContext context)
    {
        decimal confidence = 0.5m;
        
        // RSI 기반 신뢰도
        if (indicatorValues.TryGetValue("RSI", out var rsi))
        {
            if (rsi < 30 || rsi > 70)
                confidence += 0.15m;
        }
        
        // 거래량 기반 신뢰도
        if (context.MarketData.VolumeRatio > 1.5m)
        {
            confidence += 0.1m;
        }
        
        // 추세 일치성
        if (context.MarketTrend == TrendDirection.Up && Type == StrategyType.Momentum)
        {
            confidence += 0.15m;
        }
        
        // 신뢰도 범위 제한
        return Math.Min(1.0m, Math.Max(0.1m, confidence));
    }
}
```

### 2.2 Value Objects

```csharp
/// <summary>전략 ID</summary>
public sealed class StrategyId : StronglyTypedId<Guid>
{
    public StrategyId(Guid value) : base(value) { }
    public static StrategyId New() => new(Guid.NewGuid());
}

/// <summary>신호 ID</summary>
public sealed class StrategySignalId : StronglyTypedId<Guid>
{
    public StrategySignalId(Guid value) : base(value) { }
    public static StrategySignalId New() => new(Guid.NewGuid());
}

/// <summary>전략 규칙</summary>
public sealed class StrategyRule : ValueObject
{
    public StrategyRuleId Id { get; }
    public RuleType RuleType { get; }
    public string Condition { get; }
    public string Description { get; }
    public bool IsEnabled { get; }
    
    public StrategyRule(
        StrategyRuleId id,
        RuleType ruleType,
        string condition,
        string description,
        bool isEnabled = true)
    {
        Id = id;
        RuleType = ruleType;
        Condition = condition;
        Description = description;
        IsEnabled = isEnabled;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return RuleType;
        yield return Condition;
    }
}

/// <summary>지표 설정</summary>
public sealed class IndicatorConfiguration : ValueObject
{
    public IndicatorType Type { get; }
    public Dictionary<string, object> Parameters { get; }
    
    public IndicatorConfiguration(
        IndicatorType type,
        params (string key, object value)[] parameters)
    {
        Type = type;
        Parameters = parameters.ToDictionary(p => p.key, p => p.value);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return string.Join(",", Parameters.Select(p => $"{p.Key}={p.Value}"));
    }
}

/// <summary>리스크 프로필</summary>
public sealed class RiskProfile : ValueObject
{
    public decimal MaxDailyLoss { get; }
    public decimal MaxDrawdown { get; }
    public int MaxOpenPositions { get; }
    public decimal MaxPositionSize { get; }
    public decimal StopLossPercent { get; }
    public decimal TakeProfitPercent { get; }
    
    public RiskProfile(
        decimal maxDailyLoss,
        decimal maxDrawdown,
        int maxOpenPositions,
        decimal maxPositionSize,
        decimal stopLossPercent,
        decimal takeProfitPercent)
    {
        MaxDailyLoss = maxDailyLoss;
        MaxDrawdown = maxDrawdown;
        MaxOpenPositions = maxOpenPositions;
        MaxPositionSize = maxPositionSize;
        StopLossPercent = stopLossPercent;
        TakeProfitPercent = takeProfitPercent;
    }
    
    public static RiskProfile Default() => new(
        maxDailyLoss: -1000000m,
        maxDrawdown: 0.2m,
        maxOpenPositions: 5,
        maxPositionSize: 50000000m,
        stopLossPercent: 0.05m,
        takeProfitPercent: 0.1m);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MaxDailyLoss;
        yield return MaxDrawdown;
        yield return MaxOpenPositions;
    }
}

/// <summary>손절 규칙</summary>
public sealed class StopLossRule : ValueObject
{
    public StopLossType Type { get; }
    public decimal Value { get; }
    
    public StopLossRule(StopLossType type, decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("Stop loss value must be positive");
        
        Type = type;
        Value = value;
    }
    
    public decimal CalculateStopPrice(decimal entryPrice)
    {
        return Type switch
        {
            StopLossType.Percentage => entryPrice * (1 - Value / 100),
            StopLossType.Fixed => entryPrice - Value,
            _ => entryPrice
        };
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Value;
    }
}

/// <summary>익절 규칙</summary>
public sealed class TakeProfitRule : ValueObject
{
    public TakeProfitType Type { get; }
    public decimal Value { get; }
    
    public TakeProfitRule(TakeProfitType type, decimal value)
    {
        Type = type;
        Value = value;
    }
    
    public decimal CalculateTakeProfitPrice(decimal entryPrice)
    {
        return Type switch
        {
            TakeProfitType.Percentage => entryPrice * (1 + Value / 100),
            TakeProfitType.Fixed => entryPrice + Value,
            _ => entryPrice
        };
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Type;
        yield return Value;
    }
}

/// <summary>포지션 사이징</summary>
public sealed class PositionSizing : ValueObject
{
    public SizingMethod Method { get; }
    public decimal Value { get; }
    
    public PositionSizing(SizingMethod method, decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("Position sizing value must be positive");
        
        Method = method;
        Value = value;
    }
    
    public int CalculateQuantity(decimal availableBalance, decimal stockPrice)
    {
        var amount = Method switch
        {
            SizingMethod.FixedAmount => Value,
            SizingMethod.PercentOfCapital => availableBalance * (Value / 100),
            SizingMethod.KellyCriterion => CalculateKellyAmount(availableBalance),
            _ => availableBalance * 0.02m // 기본값: 자본의 2%
        };
        
        return Math.Max(1, (int)(amount / stockPrice));
    }
    
    private decimal CalculateKellyAmount(decimal availableBalance)
    {
        // Kelly Criterion: f = (b*p - q) / b
        // 여기서는 단순화된 버전
        return availableBalance * Math.Min(Value / 100, 0.25m);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Method;
        yield return Value;
    }
}

/// <summary>거래 전략 신호</summary>
public sealed class StrategySignal : ValueObject
{
    public StrategySignalId Id { get; }
    public StrategyId StrategyId { get; }
    public StockCode StockCode { get; }
    public SignalType Type { get; }
    public decimal Strength { get; }
    public DateTime GeneratedAt { get; }
    public Dictionary<string, decimal> IndicatorValues { get; }
    
    public StrategySignal(
        StrategySignalId id,
        StrategyId strategyId,
        StockCode stockCode,
        SignalType type,
        decimal strength,
        DateTime generatedAt,
        Dictionary<string, decimal> indicatorValues = null)
    {
        if (strength < 0 || strength > 1)
            throw new ArgumentException("Signal strength must be between 0 and 1");
        
        Id = id;
        StrategyId = strategyId;
        StockCode = stockCode;
        Type = type;
        Strength = strength;
        GeneratedAt = generatedAt;
        IndicatorValues = indicatorValues ?? new Dictionary<string, decimal>();
    }
    
    public bool IsStrongSignal(decimal threshold = 0.7m) => Strength >= threshold;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return StrategyId;
        yield return StockCode;
        yield return Type;
        yield return GeneratedAt;
    }
}

/// <summary>전략 성과</summary>
public sealed class StrategyPerformance : ValueObject
{
    public int TotalTrades { get; private set; }
    public int WinningTrades { get; private set; }
    public int LosingTrades { get; private set; }
    public decimal WinRate { get; private set; }
    public decimal TotalPnL { get; private set; }
    public decimal MaxDrawdown { get; private set; }
    public decimal SharpeRatio { get; private set; }
    public decimal ProfitFactor { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    
    private StrategyPerformance() { }
    
    public static StrategyPerformance Initialize() => new()
    {
        TotalTrades = 0,
        WinningTrades = 0,
        LosingTrades = 0,
        WinRate = 0,
        TotalPnL = 0,
        MaxDrawdown = 0,
        SharpeRatio = 0,
        ProfitFactor = 0,
        LastUpdatedAt = DateTime.UtcNow
    };
    
    public StrategyPerformance AddTrade(TradeResult result)
    {
        var newPerformance = new StrategyPerformance
        {
            TotalTrades = TotalTrades + 1,
            WinningTrades = WinningTrades + (result.PnL > 0 ? 1 : 0),
            LosingTrades = LosingTrades + (result.PnL <= 0 ? 1 : 0),
            TotalPnL = TotalPnL + result.PnL,
            MaxDrawdown = Math.Max(MaxDrawdown, result.Drawdown),
            LastUpdatedAt = DateTime.UtcNow
        };
        
        newPerformance.WinRate = newPerformance.TotalTrades > 0 
            ? (decimal)newPerformance.WinningTrades / newPerformance.TotalTrades * 100 
            : 0;
        
        return newPerformance;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TotalTrades;
        yield return TotalPnL;
        yield return WinRate;
    }
}
```

### 2.3 Enumerations

```csharp
/// <summary>전략 유형</summary>
public enum StrategyType
{
    Momentum = 1,           // 모멘텀
    MeanReversion = 2,      // 평균회귀
    Breakout = 3,           // 돌파
    Arbitrage = 4,          // 차익거래
    PairTrading = 5,        // 페어트레이딩
    MarketMaking = 6,       // 마켓메이킹
    Custom = 7              // 사용자정의
}

/// <summary>전략 카테고리</summary>
public enum StrategyCategory
{
    Technical = 1,          // 기술적
    Fundamental = 2,        // 기본적
    Statistical = 3,        // 통계적
    MachineLearning = 4,    // 머신러닝
    Hybrid = 5              // 하이브리드
}

/// <summary>거래 스타일</summary>
public enum TradingStyle
{
    Scalping = 1,           // 스캘핑
    DayTrading = 2,         // 데이트레이딩
    SwingTrading = 3,       // 스윙트레이딩
    PositionTrading = 4,    // 포지션트레이딩
    LongTermInvesting = 5   // 장기투자
}

/// <summary>전략 상태</summary>
public enum StrategyStatus
{
    Draft = 1,              // 초안
    Testing = 2,            // 테스트중
    Active = 3,             // 활성
    Inactive = 4,           // 비활성
    Archived = 5            // 보관
}

/// <summary>실행 모드</summary>
public enum ExecutionMode
{
    Manual = 1,             // 수동
    Auto = 2,               // 자동
    Paper = 3,              // 모의
    Hybrid = 4              // 하이브리드
}

/// <summary>신호 유형</summary>
public enum SignalType
{
    Buy = 1,                // 매수
    Sell = 2,               // 매도
    StopLoss = 3,           // 손절
    TakeProfit = 4,         // 익절
    Hold = 5,               // 보유
    Close = 6               // 청산
}

/// <summary>규칙 유형</summary>
public enum RuleType
{
    Entry = 1,              // 진입
    Exit = 2,               // 청산
    Filter = 3,             // 필터
    Risk = 4                // 리스크
}

/// <summary>지표 유형</summary>
public enum IndicatorType
{
    // 추세 지표
    SMA = 1,
    EMA = 2,
    WMA = 3,
    MACD = 4,
    ADX = 5,
    
    // 모멘텀 지표
    RSI = 6,
    Stochastic = 7,
    CCI = 8,
    Williams = 9,
    
    // 변동성 지표
    BollingerBands = 10,
    ATR = 11,
    KeltnerChannel = 12,
    DonchianChannel = 13,
    
    // 거래량 지표
    Volume = 14,
    OBV = 15,
    MFI = 16,
    VWAP = 17,
    
    // 기타
    Custom = 18
}

/// <summary>백테스트 상태</summary>
public enum BacktestStatus
{
    Created = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

/// <summary>최적화 유형</summary>
public enum OptimizationType
{
    GridSearch = 1,
    RandomSearch = 2,
    GeneticAlgorithm = 3,
    BayesianOptimization = 4,
    WalkForward = 5
}

/// <summary>최적화 목표</summary>
public enum OptimizationObjective
{
    MaxReturn = 1,
    MaxSharpe = 2,
    MinDrawdown = 3,
    MaxProfitFactor = 4,
    MaxWinRate = 5
}

/// <summary>손절 유형</summary>
public enum StopLossType
{
    Percentage = 1,
    Fixed = 2,
    ATR = 3,
    Support = 4
}

/// <summary>익절 유형</summary>
public enum TakeProfitType
{
    Percentage = 1,
    Fixed = 2,
    MultiLevel = 3,
    Resistance = 4
}

/// <summary>포지션 사이징 방법</summary>
public enum SizingMethod
{
    FixedAmount = 1,
    PercentOfCapital = 2,
    KellyCriterion = 3,
    VolatilityBased = 4
}

/// <summary>추세 방향</summary>
public enum TrendDirection
{
    Up = 1,
    Down = 2,
    Sideways = 3
}
```

---

## 3. Business Rules & Invariants

### 3.1 핵심 불변식

```csharp
/// <summary>
/// 전략의 핵심 비즈니스 규칙
/// </summary>
public static class StrategyInvariants
{
    /// <summary>
    /// Invariant 1: 활성 전략은 최소 1개의 진입 규칙 필요
    /// </summary>
    public static void ValidateEntryRules(TradingStrategy strategy)
    {
        if (strategy.Status == StrategyStatus.Active && !strategy.EntryRules.Any())
            throw new InvalidOperationException("활성 전략은 진입 규칙이 필요합니다");
    }
    
    /// <summary>
    /// Invariant 2: 활성 전략은 최소 1개의 청산 규칙 필요
    /// </summary>
    public static void ValidateExitRules(TradingStrategy strategy)
    {
        if (strategy.Status == StrategyStatus.Active && !strategy.ExitRules.Any())
            throw new InvalidOperationException("활성 전략은 청산 규칙이 필요합니다");
    }
    
    /// <summary>
    /// Invariant 3: 신호 강도는 0-1 범위
    /// </summary>
    public static void ValidateSignalStrength(StrategySignal signal)
    {
        if (signal.Strength < 0 || signal.Strength > 1)
            throw new InvalidOperationException("신호 강도는 0과 1 사이여야 합니다");
    }
    
    /// <summary>
    /// Invariant 4: 승률은 0-100 범위
    /// </summary>
    public static void ValidateWinRate(StrategyPerformance performance)
    {
        if (performance.WinRate < 0 || performance.WinRate > 100)
            throw new InvalidOperationException("승률은 0-100 사이여야 합니다");
    }
}
```

### 3.2 비즈니스 규칙

```csharp
/// <summary>
/// 전략 실행 관련 비즈니스 규칙
/// </summary>
public static class StrategyBusinessRules
{
    /// <summary>
    /// 규칙 1: 신호 생성 조건 확인
    /// </summary>
    public static bool CanGenerateSignal(
        TradingStrategy strategy,
        DateTime currentTime)
    {
        if (strategy.Status != StrategyStatus.Active)
            return false;
        
        if (strategy.TradingHours != null && !strategy.TradingHours.IsWithinTradingHours(currentTime))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// 규칙 2: 신호 강도 검증
    /// </summary>
    public static bool IsValidSignal(
        StrategySignal signal,
        decimal minimumStrength = 0.5m)
    {
        return signal.Strength >= minimumStrength;
    }
    
    /// <summary>
    /// 규칙 3: 전략 성과 임계값 확인
    /// </summary>
    public static bool HasAcceptablePerformance(
        StrategyPerformance performance,
        decimal minWinRate = 0.3m,
        decimal maxDrawdown = 0.2m)
    {
        if (performance.TotalTrades == 0)
            return true;
        
        return performance.WinRate >= minWinRate * 100 &&
               performance.MaxDrawdown <= maxDrawdown;
    }
}
```

---

## 4. Domain Events

```csharp
/// <summary>전략 생성 이벤트</summary>
public sealed class StrategyCreatedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public string StrategyName { get; }
    public string Author { get; }
    public DateTime CreatedAt { get; }
    
    public StrategyCreatedEvent(StrategyId strategyId, string strategyName, string author, DateTime createdAt)
    {
        StrategyId = strategyId;
        StrategyName = strategyName;
        Author = author;
        CreatedAt = createdAt;
    }
}

/// <summary>전략 활성화 이벤트</summary>
public sealed class StrategyActivatedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public DateTime ActivatedAt { get; }
    
    public StrategyActivatedEvent(StrategyId strategyId, DateTime activatedAt)
    {
        StrategyId = strategyId;
        ActivatedAt = activatedAt;
    }
}

/// <summary>전략 비활성화 이벤트</summary>
public sealed class StrategyDeactivatedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public string Reason { get; }
    public DateTime DeactivatedAt { get; }
    
    public StrategyDeactivatedEvent(StrategyId strategyId, string reason, DateTime deactivatedAt)
    {
        StrategyId = strategyId;
        Reason = reason;
        DeactivatedAt = deactivatedAt;
    }
}

/// <summary>규칙 추가 이벤트</summary>
public sealed class StrategyRuleAddedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public StrategyRuleId RuleId { get; }
    public DateTime AddedAt { get; }
    
    public StrategyRuleAddedEvent(StrategyId strategyId, StrategyRuleId ruleId, DateTime addedAt)
    {
        StrategyId = strategyId;
        RuleId = ruleId;
        AddedAt = addedAt;
    }
}

/// <summary>신호 생성 이벤트</summary>
public sealed class SignalGeneratedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public StrategySignalId SignalId { get; }
    public SignalType SignalType { get; }
    public StockCode StockCode { get; }
    public decimal SignalStrength { get; }
    public DateTime GeneratedAt { get; }
    
    public SignalGeneratedEvent(
        StrategyId strategyId,
        StrategySignalId signalId,
        SignalType signalType,
        StockCode stockCode,
        decimal signalStrength,
        DateTime generatedAt)
    {
        StrategyId = strategyId;
        SignalId = signalId;
        SignalType = signalType;
        StockCode = stockCode;
        SignalStrength = signalStrength;
        GeneratedAt = generatedAt;
    }
}

/// <summary>백테스트 완료 이벤트</summary>
public sealed class BacktestCompletedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public BacktestResult Result { get; }
    public DateTime CompletedAt { get; }
    
    public BacktestCompletedEvent(StrategyId strategyId, BacktestResult result, DateTime completedAt)
    {
        StrategyId = strategyId;
        Result = result;
        CompletedAt = completedAt;
    }
}

/// <summary>최적화 완료 이벤트</summary>
public sealed class OptimizationCompletedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public OptimizationResult Result { get; }
    public DateTime CompletedAt { get; }
    
    public OptimizationCompletedEvent(StrategyId strategyId, OptimizationResult result, DateTime completedAt)
    {
        StrategyId = strategyId;
        Result = result;
        CompletedAt = completedAt;
    }
}
```

---

## 5. Domain Services

```csharp
/// <summary>전략 실행 서비스</summary>
public interface IStrategyExecutionService : IDomainService
{
    Task<StrategySignal> ExecuteStrategyAsync(StrategyId strategyId, MarketContext context);
    Task<List<StrategySignal>> ExecuteAllActiveStrategiesAsync(MarketContext context);
    Task<bool> ValidateStrategyAsync(TradingStrategy strategy);
}

/// <summary>신호 생성 서비스</summary>
public interface ISignalGenerationService : IDomainService
{
    Task<StrategySignal> GenerateSignalAsync(
        TradingStrategy strategy,
        StockCode stockCode,
        MarketData marketData);
    Task<Dictionary<string, decimal>> CalculateIndicatorsAsync(
        List<IndicatorConfiguration> indicators,
        List<MarketData> historicalData);
}

/// <summary>백테스팅 서비스</summary>
public interface IBacktestingService : IDomainService
{
    Task<BacktestResult> RunBacktestAsync(
        TradingStrategy strategy,
        BacktestConfiguration config);
    Task<List<BacktestTrade>> SimulateTradesAsync(
        TradingStrategy strategy,
        List<MarketData> historicalData);
}

/// <summary>최적화 서비스</summary>
public interface IOptimizationService : IDomainService
{
    Task<OptimizationResult> OptimizeStrategyAsync(
        TradingStrategy strategy,
        OptimizationConfiguration config);
    Task<Dictionary<string, object>> FindOptimalParametersAsync(
        StrategyId strategyId,
        OptimizationObjective objective);
}
```

---

## 6. Application Services

```csharp
/// <summary>전략 애플리케이션 서비스</summary>
public sealed class StrategyService : IApplicationService
{
    private readonly IStrategyRepository _strategyRepository;
    private readonly IStrategyExecutionService _executionService;
    private readonly ISignalGenerationService _signalService;
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StrategyService> _logger;
    
    /// <summary>전략 생성</summary>
    public async Task<Result<StrategyDto>> CreateStrategyAsync(
        CreateStrategyCommand command)
    {
        try
        {
            var strategy = TradingStrategy.Create(
                command.Name,
                command.Description,
                command.Type,
                command.Category,
                command.TradingStyle,
                command.TimeFrame,
                command.Author);
            
            await _strategyRepository.AddAsync(strategy);
            await _unitOfWork.CommitAsync();
            
            foreach (var @event in strategy.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return Result<StrategyDto>.Success(MapToDto(strategy));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create strategy");
            return Result<StrategyDto>.Failure($"Strategy creation failed: {ex.Message}");
        }
    }
    
    /// <summary>전략 활성화</summary>
    public async Task<Result> ActivateStrategyAsync(Guid strategyId)
    {
        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId));
            if (strategy == null)
                return Result.Failure("Strategy not found");
            
            var result = strategy.Activate();
            if (!result.IsSuccess)
                return result;
            
            await _strategyRepository.UpdateAsync(strategy);
            await _unitOfWork.CommitAsync();
            
            foreach (var @event in strategy.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate strategy");
            return Result.Failure($"Strategy activation failed: {ex.Message}");
        }
    }
    
    /// <summary>신호 생성</summary>
    public async Task<Result<StrategySignalDto>> GenerateSignalAsync(
        Guid strategyId,
        StockCode stockCode,
        MarketData marketData)
    {
        try
        {
            var strategy = await _strategyRepository.GetByIdAsync(new StrategyId(strategyId));
            if (strategy == null)
                return Result<StrategySignalDto>.Failure("Strategy not found");
            
            var signal = await _signalService.GenerateSignalAsync(
                strategy,
                stockCode,
                marketData);
            
            strategy.RecordSignal(signal);
            await _strategyRepository.UpdateAsync(strategy);
            await _unitOfWork.CommitAsync();
            
            foreach (var @event in strategy.DomainEvents)
            {
                await _eventBus.PublishAsync(@event);
            }
            
            return Result<StrategySignalDto>.Success(MapToSignalDto(signal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate signal");
            return Result<StrategySignalDto>.Failure($"Signal generation failed: {ex.Message}");
        }
    }
    
    private StrategyDto MapToDto(TradingStrategy strategy)
    {
        return new StrategyDto
        {
            Id = strategy.Id.Value,
            Name = strategy.Name,
            Type = strategy.Type.ToString(),
            Status = strategy.Status.ToString(),
            CreatedAt = strategy.CreatedAt,
            LastModifiedAt = strategy.LastModifiedAt
        };
    }
    
    private StrategySignalDto MapToSignalDto(StrategySignal signal)
    {
        return new StrategySignalDto
        {
            Id = signal.Id.Value,
            StrategyId = signal.StrategyId.Value,
            StockCode = signal.StockCode.Value,
            Type = signal.Type.ToString(),
            Strength = signal.Strength,
            GeneratedAt = signal.GeneratedAt
        };
    }
}
```

---

## 7. Infrastructure Layer

```csharp
/// <summary>전략 저장소 구현</summary>
public sealed class StrategyRepository : IStrategyRepository
{
    private readonly StrategyDbContext _context;
    private readonly IDistributedCache _cache;
    
    public async Task<TradingStrategy> GetByIdAsync(StrategyId strategyId)
    {
        var cacheKey = $"strategy:{strategyId}";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        if (cachedData != null)
            return JsonSerializer.Deserialize<TradingStrategy>(cachedData);
        
        var entity = await _context.Strategies
            .AsNoTracking()
            .Include(s => s.Rules)
            .FirstOrDefaultAsync(s => s.Id == strategyId);
        
        if (entity != null)
        {
            var json = JsonSerializer.Serialize(entity);
            await _cache.SetStringAsync(cacheKey, json, TimeSpan.FromMinutes(5));
        }
        
        return entity;
    }
    
    public async Task AddAsync(TradingStrategy strategy)
    {
        _context.Strategies.Add(strategy);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(TradingStrategy strategy)
    {
        _context.Strategies.Update(strategy);
        await _context.SaveChangesAsync();
        
        var cacheKey = $"strategy:{strategy.Id}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

---

## 8. API/Interface Specifications

```csharp
[ApiController]
[Route("api/strategies")]
[Authorize]
public class StrategiesController : ControllerBase
{
    private readonly IMediator _mediator;
    
    /// <summary>POST /api/strategies - 전략 생성</summary>
    [HttpPost]
    public async Task<ActionResult<StrategyDto>> CreateStrategyAsync(
        [FromBody] CreateStrategyRequest request)
    {
        var command = new CreateStrategyCommand(
            request.Name,
            request.Description,
            request.Type,
            request.Category,
            request.TradingStyle,
            request.TimeFrame,
            request.Author);
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Created($"/api/strategies/{result.Value.Id}", result.Value);
    }
    
    /// <summary>POST /api/strategies/{strategyId}/activate - 전략 활성화</summary>
    [HttpPost("{strategyId}/activate")]
    public async Task<IActionResult> ActivateStrategyAsync(Guid strategyId)
    {
        var command = new ActivateStrategyCommand(strategyId);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        
        return Ok();
    }
    
    /// <summary>GET /api/strategies/{strategyId}/signals - 신호 조회</summary>
    [HttpGet("{strategyId}/signals")]
    public async Task<ActionResult<List<StrategySignalDto>>> GetSignalsAsync(Guid strategyId)
    {
        var query = new GetSignalsQuery(strategyId);
        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);
        
        return Ok(result.Value);
    }
}
```

---

## 9. Strategy Execution Flow

```
전략 활성화
    ↓
실시간 시세 수신
    ↓
신호 생성
    ├─ 지표 계산
    ├─ 규칙 평가
    ├─ 신호 강도 계산
    └─ 신호 생성
    ↓
신호 필터링
    ├─ 신호 강도 검증
    ├─ 거래 시간 확인
    └─ 리스크 검증
    ↓
신호 실행
    ├─ Trading Context로 전달
    ├─ 주문 생성
    └─ 성과 기록
    ↓
성과 추적
    ├─ 거래 결과 수집
    ├─ 성과 지표 갱신
    └─ 리스크 모니터링
```

---

## 10. Backtesting & Optimization

### 백테스팅 프로세스

```csharp
public class BacktestingService : IBacktestingService
{
    public async Task<BacktestResult> RunBacktestAsync(
        TradingStrategy strategy,
        BacktestConfiguration config)
    {
        var result = new BacktestResult();
        
        // 1. 히스토리 데이터 로드
        var historicalData = await LoadHistoricalDataAsync(
            config.Symbol,
            config.StartDate,
            config.EndDate);
        
        // 2. 거래 시뮬레이션
        var trades = SimulateTrades(strategy, historicalData);
        
        // 3. 결과 계산
        result.TotalTrades = trades.Count;
        result.WinningTrades = trades.Count(t => t.PnL > 0);
        result.TotalPnL = trades.Sum(t => t.PnL);
        result.MaxDrawdown = CalculateMaxDrawdown(trades);
        result.SharpeRatio = CalculateSharpeRatio(trades);
        
        return result;
    }
}
```

### 최적화 프로세스

```csharp
public class OptimizationService : IOptimizationService
{
    public async Task<OptimizationResult> OptimizeStrategyAsync(
        TradingStrategy strategy,
        OptimizationConfiguration config)
    {
        var results = new List<OptimizationResult>();
        
        // 1. 매개변수 범위 설정
        var paramRanges = DefineParameterRanges(strategy);
        
        // 2. 조합 생성 및 평가
        foreach (var combination in GenerateCombinations(paramRanges, config.Type))
        {
            strategy.UpdateParameters(combination);
            var backtest = await RunBacktestAsync(strategy, config.BacktestConfig);
            results.Add(new OptimizationResult(combination, backtest));
        }
        
        // 3. 최적 매개변수 선택
        var optimal = results.OrderByDescending(r => r.Score).First();
        
        return optimal;
    }
}
```

---

## 11. Error Handling & Validation

```csharp
public sealed class InvalidStrategyException : DomainException
{
    public StrategyId StrategyId { get; }
    
    public InvalidStrategyException(string message, StrategyId strategyId = null)
        : base(message) { StrategyId = strategyId; }
}

public sealed class SignalGenerationException : DomainException
{
    public StrategyId StrategyId { get; }
    
    public SignalGenerationException(string message, StrategyId strategyId)
        : base(message) { StrategyId = strategyId; }
}

public sealed class BacktestException : DomainException
{
    public string BacktestId { get; }
    
    public BacktestException(string message, string backtestId)
        : base(message) { BacktestId = backtestId; }
}
```

---

## 12. Performance Considerations

### 캐싱 전략
- L1: Redis (5분 TTL) - 활성 전략
- L2: Database - 완전 데이터

### 배치 처리
- 신호 배치 처리 (100개 단위)
- 백테스트 병렬 처리

### 모니터링
- 신호 생성 빈도 모니터링
- 성과 지표 실시간 추적
- 리스크 한도 초과 알림

---

## 결론

이 Strategy Context 설계는 다음 특징을 갖습니다:

✅ **완전성**: 전략 관리의 모든 측면
✅ **유연성**: 다양한 전략 유형 지원
✅ **신뢰성**: 신호의 검증 및 백테스팅
✅ **성능**: 캐싱과 배치 처리로 저지연
✅ **확장성**: 사용자 정의 지표 지원
✅ **투명성**: 모든 신호와 거래의 완전한 기록
