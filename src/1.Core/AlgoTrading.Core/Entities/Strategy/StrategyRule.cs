using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Strategy;

/// <summary>
/// description(설명) : Strategy Rule Entity - Represents a trading rule within a strategy (전략 규칙 엔티티 - 전략 내의 매매 규칙을 나타냄)
/// Details(상세설명) : Defines entry, exit, and filter conditions with priority and parameters for strategy execution (전략 실행을 위한 진입, 청산, 필터 조건을 우선순위 및 파라미터와 함께 정의)
/// Applied technology patterns(적용기술패턴) : Entity Pattern (엔티티 패턴), Domain-Driven Design (도메인 주도 설계), Strategy Pattern (전략 패턴)
/// </summary>
public sealed class StrategyRule : Entity<StrategyRuleId>
{
    /// <summary>
    /// 규칙 이름
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 규칙 설명
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// 규칙 유형 (진입/청산/필터)
    /// </summary>
    public RuleType Type { get; private set; }

    /// <summary>
    /// 규칙 조건 (표현식 또는 코드)
    /// </summary>
    public string Condition { get; private set; }

    /// <summary>
    /// 규칙 우선순위 (높을수록 먼저 평가)
    /// </summary>
    public int Priority { get; private set; }

    /// <summary>
    /// 규칙 활성화 여부
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// 규칙 파라미터
    /// </summary>
    public Dictionary<string, object> Parameters { get; private set; }

    /// <summary>
    /// 규칙 평가 시간프레임
    /// </summary>
    public string? Timeframe { get; private set; }

    private StrategyRule()
    {
        Parameters = new Dictionary<string, object>();
    }

    private StrategyRule(
        StrategyRuleId id,
        string name,
        string description,
        RuleType type,
        string condition,
        int priority = 0)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Condition = condition;
        Priority = priority;
        IsEnabled = true;
        Parameters = new Dictionary<string, object>();
    }

    /// <summary>
    /// description(설명) : Create a new strategy rule (새 전략 규칙 생성)
    /// Details(상세설명) : Factory method to create a rule defining entry, exit, or filter conditions (진입, 청산 또는 필터 조건을 정의하는 규칙을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created StrategyRule instance (새로 생성된 전략 규칙 인스턴스)</returns>
    public static StrategyRule Create(
        string name,
        string description,
        RuleType type,
        string condition,
        int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Rule condition cannot be empty", nameof(condition));

        var id = new StrategyRuleId(Guid.NewGuid());
        return new StrategyRule(id, name, description, type, condition, priority);
    }

    /// <summary>
    /// description(설명) : Enable the rule (규칙 활성화)
    /// Details(상세설명) : Marks the rule as active for evaluation during strategy execution (전략 실행 중 평가를 위해 규칙을 활성으로 표시)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Disable the rule (규칙 비활성화)
    /// Details(상세설명) : Marks the rule as inactive to skip during strategy execution (전략 실행 중 건너뛰도록 규칙을 비활성으로 표시)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Update rule condition (규칙 조건 업데이트)
    /// Details(상세설명) : Modifies the evaluation condition for the rule (규칙의 평가 조건 수정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            throw new ArgumentException("Condition cannot be empty", nameof(condition));

        Condition = condition;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Set rule priority (규칙 우선순위 설정)
    /// Details(상세설명) : Sets evaluation order priority where higher values are evaluated first (높은 값이 먼저 평가되는 우선순위 설정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void SetPriority(int priority)
    {
        Priority = priority;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Add or update a parameter (파라미터 추가 또는 업데이트)
    /// Details(상세설명) : Sets a key-value parameter for rule customization (규칙 커스터마이징을 위한 키-값 파라미터 설정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void SetParameter(string key, object value)
    {
        Parameters[key] = value;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Set timeframe for rule evaluation (규칙 평가 시간프레임 설정)
    /// Details(상세설명) : Specifies the timeframe context for rule evaluation (규칙 평가를 위한 시간프레임 컨텍스트 지정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void SetTimeframe(string timeframe)
    {
        Timeframe = timeframe;
        MarkAsModified();
    }
}

/// <summary>
/// description(설명) : Strategy Rule identifier value object (전략 규칙 식별자 값 객체)
/// Details(상세설명) : Strongly-typed identifier for StrategyRule entity using GUID (GUID를 사용한 전략 규칙 엔티티의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class StrategyRuleId : GuidId
{
    public StrategyRuleId(Guid value) : base(value) { }

    public static StrategyRuleId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Rule type enumeration (규칙 유형 열거형)
/// Details(상세설명) : Categorizes rules by their purpose in trading logic (매매 로직에서의 목적에 따라 규칙 분류)
/// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum RuleType
{
    EntryLong = 1,              // Long entry condition (롱 진입 조건)
    EntryShort = 2,             // Short entry condition (숏 진입 조건)
    ExitLong = 3,               // Long exit condition (롱 청산 조건)
    ExitShort = 4,              // Short exit condition (숏 청산 조건)
    Filter = 5,                 // Filter condition that must pass (통과해야 하는 필터 조건)
    StopLoss = 6,               // Stop loss trigger condition (손절 트리거 조건)
    TakeProfit = 7              // Take profit trigger condition (익절 트리거 조건)
}
