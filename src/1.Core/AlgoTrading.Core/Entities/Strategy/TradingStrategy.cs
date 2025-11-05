using AlgoTrading.Core.ValueObjects;
using AlgoTrading.Core.DomainEvents;

namespace AlgoTrading.Core.Entities.Strategy;

/// <summary>
/// description(설명) : Trading Strategy Aggregate Root representing an algorithmic trading strategy (알고리즘 트레이딩 전략을 나타내는 전략 집합 루트)
/// Details(상세설명) : Manages trading rules, signals, and performance metrics with strategy lifecycle control (전략 생명주기 제어와 함께 거래 규칙, 신호, 성과 지표를 관리)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern, Domain-Driven Design, Strategy Pattern (집합 루트 패턴, 도메인 주도 설계, 전략 패턴)
/// </summary>
public sealed class TradingStrategy : AggregateRoot<StrategyId>
{
    private readonly List<StrategyRule> _rules = new();
    private readonly List<StrategySignal> _signals = new();

    /// <summary>
    /// 전략 이름
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 전략 설명
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// 전략 유형
    /// </summary>
    public StrategyType Type { get; private set; }

    /// <summary>
    /// 전략 상태
    /// </summary>
    public StrategyStatus Status { get; private set; }

    /// <summary>
    /// 대상 종목 코드 목록 (빈 목록이면 모든 종목)
    /// </summary>
    public List<StockCode> TargetStocks { get; private set; }

    /// <summary>
    /// 거래 규칙 목록
    /// </summary>
    public IReadOnlyList<StrategyRule> Rules => _rules.AsReadOnly();

    /// <summary>
    /// 생성된 신호 목록
    /// </summary>
    public IReadOnlyList<StrategySignal> Signals => _signals.AsReadOnly();

    /// <summary>
    /// 전략 파라미터 (JSON 직렬화)
    /// </summary>
    public Dictionary<string, object> Parameters { get; private set; }

    /// <summary>
    /// 최대 포지션 크기 비율
    /// </summary>
    public decimal MaxPositionSizePercent { get; private set; }

    /// <summary>
    /// 손절 비율
    /// </summary>
    public decimal StopLossPercent { get; private set; }

    /// <summary>
    /// 익절 비율
    /// </summary>
    public decimal TakeProfitPercent { get; private set; }

    /// <summary>
    /// 총 거래 수
    /// </summary>
    public int TotalTrades { get; private set; }

    /// <summary>
    /// 수익 거래 수
    /// </summary>
    public int WinningTrades { get; private set; }

    /// <summary>
    /// 손실 거래 수
    /// </summary>
    public int LosingTrades { get; private set; }

    /// <summary>
    /// 총 손익
    /// </summary>
    public decimal TotalProfitLoss { get; private set; }

    /// <summary>
    /// 승률 (퍼센트)
    /// </summary>
    public decimal WinRate => TotalTrades > 0 ? (decimal)WinningTrades / TotalTrades * 100 : 0;

    /// <summary>
    /// 거래 시작 시간
    /// </summary>
    public TimeSpan? TradingStartTime { get; private set; }

    /// <summary>
    /// 거래 종료 시간
    /// </summary>
    public TimeSpan? TradingEndTime { get; private set; }

    /// <summary>
    /// 생성자 사용자 ID
    /// </summary>
    public string CreatedBy { get; private set; }

    /// <summary>
    /// 마지막 활성화 시간
    /// </summary>
    public DateTime? LastActivatedAt { get; private set; }

    /// <summary>
    /// 마지막 비활성화 시간
    /// </summary>
    public DateTime? LastDeactivatedAt { get; private set; }

    private TradingStrategy()
    {
        TargetStocks = new List<StockCode>();
        Parameters = new Dictionary<string, object>();
    }

    private TradingStrategy(
        StrategyId id,
        string name,
        string description,
        StrategyType type,
        string createdBy)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        CreatedBy = createdBy;
        Status = StrategyStatus.Draft;
        TargetStocks = new List<StockCode>();
        Parameters = new Dictionary<string, object>();
        MaxPositionSizePercent = 10; // Default 10%
        StopLossPercent = 5; // Default 5%
        TakeProfitPercent = 10; // Default 10%
    }

    /// <summary>
    /// description(설명) : Create a new trading strategy (새로운 거래 전략 생성)
    /// Details(상세설명) : Factory method to create a trading strategy with initial state and validation (초기 상태와 검증을 포함한 거래 전략을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern, Domain-Driven Design, Domain Events (팩토리 패턴, 도메인 주도 설계, 도메인 이벤트)
    /// </summary>
    /// <returns>Created trading strategy instance (생성된 거래 전략 인스턴스)</returns>
    public static TradingStrategy Create(
        string name,
        string description,
        StrategyType type,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Strategy name cannot be empty", nameof(name));

        var id = new StrategyId(Guid.NewGuid());
        var strategy = new TradingStrategy(id, name, description, type, createdBy);

        strategy.AddDomainEvent(new StrategyCreatedEvent(id, name, type, DateTime.UtcNow));

        return strategy;
    }

    /// <summary>
    /// description(설명) : Add a trading rule to the strategy (전략에 거래 규칙 추가)
    /// Details(상세설명) : Adds a new trading rule to the strategy, cannot modify active strategy (전략에 새 거래 규칙을 추가, 활성 전략은 수정 불가)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Aggregate Root Pattern (도메인 주도 설계, 집합 루트 패턴)
    /// </summary>
    public void AddRule(StrategyRule rule)
    {
        if (Status == StrategyStatus.Active)
            throw new InvalidOperationException("Cannot modify active strategy");

        _rules.Add(rule);
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Remove a trading rule from the strategy (전략에서 거래 규칙 제거)
    /// Details(상세설명) : Removes an existing trading rule by ID, cannot modify active strategy (ID로 기존 거래 규칙을 제거, 활성 전략은 수정 불가)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Aggregate Root Pattern (도메인 주도 설계, 집합 루트 패턴)
    /// </summary>
    public void RemoveRule(StrategyRuleId ruleId)
    {
        if (Status == StrategyStatus.Active)
            throw new InvalidOperationException("Cannot modify active strategy");

        var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule != null)
        {
            _rules.Remove(rule);
            MarkAsModified();
        }
    }

    /// <summary>
    /// description(설명) : Set target stocks for the strategy (전략의 대상 종목 설정)
    /// Details(상세설명) : Defines which stocks this strategy will trade, empty list means all stocks (이 전략이 거래할 종목을 정의, 빈 목록은 모든 종목을 의미)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void SetTargetStocks(List<StockCode> stockCodes)
    {
        TargetStocks = stockCodes ?? new List<StockCode>();
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Set risk management parameters (리스크 관리 파라미터 설정)
    /// Details(상세설명) : Configures max position size, stop loss, and take profit percentages with validation (최대 포지션 크기, 손절, 익절 비율을 검증과 함께 설정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Value Validation (도메인 주도 설계, 값 검증)
    /// </summary>
    public void SetRiskParameters(decimal maxPositionSize, decimal stopLoss, decimal takeProfit)
    {
        if (maxPositionSize <= 0 || maxPositionSize > 100)
            throw new ArgumentException("Max position size must be between 0 and 100", nameof(maxPositionSize));

        if (stopLoss <= 0 || stopLoss > 100)
            throw new ArgumentException("Stop loss must be between 0 and 100", nameof(stopLoss));

        if (takeProfit <= 0 || takeProfit > 1000)
            throw new ArgumentException("Take profit must be between 0 and 1000", nameof(takeProfit));

        MaxPositionSizePercent = maxPositionSize;
        StopLossPercent = stopLoss;
        TakeProfitPercent = takeProfit;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Set trading hours for the strategy (전략의 거래 시간 설정)
    /// Details(상세설명) : Defines time window when strategy can execute trades with validation (전략이 거래를 실행할 수 있는 시간 창을 검증과 함께 정의)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void SetTradingHours(TimeSpan? startTime, TimeSpan? endTime)
    {
        if (startTime.HasValue && endTime.HasValue && startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        TradingStartTime = startTime;
        TradingEndTime = endTime;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Activate the strategy for trading (거래를 위한 전략 활성화)
    /// Details(상세설명) : Changes status to Active, validates rules exist, and raises domain event (상태를 활성으로 변경, 규칙 존재 확인, 도메인 이벤트 발생)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern, Domain Events (상태 머신 패턴, 도메인 이벤트)
    /// </summary>
    public void Activate()
    {
        if (Status == StrategyStatus.Active)
            throw new InvalidOperationException("Strategy is already active");

        if (!_rules.Any())
            throw new InvalidOperationException("Cannot activate strategy without rules");

        Status = StrategyStatus.Active;
        LastActivatedAt = DateTime.UtcNow;

        AddDomainEvent(new StrategyActivatedEvent(Id, Name, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Deactivate the strategy (전략 비활성화)
    /// Details(상세설명) : Changes status to Inactive and raises domain event with optional reason (상태를 비활성으로 변경하고 선택적 사유와 함께 도메인 이벤트 발생)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern, Domain Events (상태 머신 패턴, 도메인 이벤트)
    /// </summary>
    public void Deactivate(string reason = null)
    {
        if (Status != StrategyStatus.Active)
            throw new InvalidOperationException("Strategy is not active");

        Status = StrategyStatus.Inactive;
        LastDeactivatedAt = DateTime.UtcNow;

        AddDomainEvent(new StrategyDeactivatedEvent(Id, Name, reason, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Record a trading signal generated by this strategy (이 전략이 생성한 거래 신호 기록)
    /// Details(상세설명) : Adds signal to collection, maintains only recent 100 signals, and raises domain event (신호를 컬렉션에 추가, 최근 100개 신호만 유지, 도메인 이벤트 발생)
    /// Applied technology patterns(적용기술패턴) : Domain Events, Collection Management (도메인 이벤트, 컬렉션 관리)
    /// </summary>
    public void RecordSignal(StrategySignal signal)
    {
        _signals.Add(signal);

        // Keep only recent 100 signals
        if (_signals.Count > 100)
        {
            _signals.RemoveAt(0);
        }

        AddDomainEvent(new SignalGeneratedEvent(Id, signal.Id, signal.StockCode, signal.SignalType, DateTime.UtcNow));
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update performance metrics after trade completion (거래 완료 후 성과 지표 업데이트)
    /// Details(상세설명) : Records trade result and updates win/loss statistics and total P&L (거래 결과를 기록하고 승/패 통계와 총 손익을 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdatePerformance(bool isWin, decimal profitLoss)
    {
        TotalTrades++;
        if (isWin)
            WinningTrades++;
        else
            LosingTrades++;

        TotalProfitLoss += profitLoss;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if strategy can trade at current time (현재 시간에 전략이 거래할 수 있는지 확인)
    /// Details(상세설명) : Validates strategy is active and current time is within trading hours (전략이 활성 상태이고 현재 시간이 거래 시간 내인지 검증)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>True if strategy can trade now (현재 거래 가능하면 true)</returns>
    public bool CanTradeNow()
    {
        if (Status != StrategyStatus.Active)
            return false;

        if (!TradingStartTime.HasValue || !TradingEndTime.HasValue)
            return true;

        var now = DateTime.Now.TimeOfDay;
        return now >= TradingStartTime.Value && now <= TradingEndTime.Value;
    }

    /// <summary>
    /// description(설명) : Check if strategy targets a specific stock (전략이 특정 종목을 대상으로 하는지 확인)
    /// Details(상세설명) : Returns true if target list is empty or contains the stock (대상 목록이 비어있거나 종목을 포함하면 true 반환)
    /// Applied technology patterns(적용기술패턴) : Query Method Pattern (쿼리 메서드 패턴)
    /// </summary>
    /// <returns>True if strategy targets the stock (전략이 종목을 대상으로 하면 true)</returns>
    public bool TargetsStock(StockCode stockCode)
    {
        return !TargetStocks.Any() || TargetStocks.Contains(stockCode);
    }
}

/// <summary>
/// description(설명) : Strategy ID value object (전략 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for TradingStrategy aggregate (TradingStrategy 집합의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class StrategyId : GuidId
{
    public StrategyId(Guid value) : base(value) { }

    public static StrategyId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Strategy Type enumeration (전략 유형 열거형)
/// Details(상세설명) : Defines the classification of trading strategies (트레이딩 전략의 분류를 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum StrategyType
{
    Trend = 1,                  // Trend following strategy (추세 추종 전략)
    MeanReversion = 2,          // Mean reversion strategy (평균 회귀 전략)
    Momentum = 3,               // Momentum strategy (모멘텀 전략)
    Arbitrage = 4,              // Arbitrage strategy (차익거래 전략)
    MarketMaking = 5,           // Market making strategy (마켓 메이킹 전략)
    Statistical = 6,            // Statistical arbitrage strategy (통계적 차익거래 전략)
    MachineLearning = 7,        // Machine learning based strategy (머신러닝 기반 전략)
    Custom = 99                 // Custom strategy (사용자 정의 전략)
}

/// <summary>
/// description(설명) : Strategy Status enumeration (전략 상태 열거형)
/// Details(상세설명) : Represents the current lifecycle state of a strategy (전략의 현재 생명주기 상태를 나타냄)
/// Applied technology patterns(적용기술패턴) : State Machine Pattern, Enumeration Pattern (상태 머신 패턴, 열거형 패턴)
/// </summary>
public enum StrategyStatus
{
    Draft = 1,                  // Being developed (개발 중)
    Testing = 2,                // Being tested (테스트 중)
    Active = 3,                 // Currently running (현재 실행 중)
    Inactive = 4,               // Paused (일시 중지)
    Archived = 5                // Archived (보관됨)
}

// Domain Events
public sealed class StrategyCreatedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public string Name { get; }
    public StrategyType Type { get; }

    public StrategyCreatedEvent(StrategyId strategyId, string name, StrategyType type, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        StrategyId = strategyId;
        Name = name;
        Type = type;
    }
}

public sealed class StrategyActivatedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public string Name { get; }

    public StrategyActivatedEvent(StrategyId strategyId, string name, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        StrategyId = strategyId;
        Name = name;
    }
}

public sealed class StrategyDeactivatedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public string Name { get; }
    public string? Reason { get; }

    public StrategyDeactivatedEvent(StrategyId strategyId, string name, string? reason, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        StrategyId = strategyId;
        Name = name;
        Reason = reason;
    }
}

public sealed class SignalGeneratedEvent : DomainEvent
{
    public StrategyId StrategyId { get; }
    public StrategySignalId SignalId { get; }
    public StockCode StockCode { get; }
    public SignalType SignalType { get; }

    public SignalGeneratedEvent(StrategyId strategyId, StrategySignalId signalId, StockCode stockCode, SignalType signalType, DateTime occurredAt)
        : base(Guid.NewGuid(), occurredAt)
    {
        StrategyId = strategyId;
        SignalId = signalId;
        StockCode = stockCode;
        SignalType = signalType;
    }
}
