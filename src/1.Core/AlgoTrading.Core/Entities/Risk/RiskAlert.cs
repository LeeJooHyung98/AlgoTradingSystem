using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Risk;

/// <summary>
/// description(설명) : Risk Alert Entity - Represents a risk violation or warning (리스크 경고 엔티티 - 리스크 위반 또는 경고를 나타냄)
/// Details(상세설명) : Created when risk thresholds are breached, contains alert type, priority, and violation details (리스크 임계값 위반 시 생성, 경고 유형, 우선순위 및 위반 상세 정보 포함)
/// Applied technology patterns(적용기술패턴) : Entity Pattern (엔티티 패턴), Domain-Driven Design (도메인 주도 설계), Factory Pattern (팩토리 패턴)
/// </summary>
public sealed class RiskAlert : Entity<RiskAlertId>
{
    /// <summary>
    /// 경고 유형
    /// </summary>
    public RiskAlertType Type { get; private set; }

    /// <summary>
    /// 경고 우선순위
    /// </summary>
    public AlertPriority Priority { get; private set; }

    /// <summary>
    /// 경고 메시지
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// 경고 상세 정보
    /// </summary>
    public string? Details { get; private set; }

    /// <summary>
    /// 경고 발생 시간
    /// </summary>
    public DateTime TriggeredAt { get; private set; }

    /// <summary>
    /// 경고 해결 시간
    /// </summary>
    public DateTime? ResolvedAt { get; private set; }

    /// <summary>
    /// 경고 활성 상태
    /// </summary>
    public bool IsActive => !ResolvedAt.HasValue;

    /// <summary>
    /// 연관된 종목 코드 (해당하는 경우)
    /// </summary>
    public StockCode? StockCode { get; private set; }

    /// <summary>
    /// 경고를 발생시킨 현재 값
    /// </summary>
    public decimal? CurrentValue { get; private set; }

    /// <summary>
    /// 임계값
    /// </summary>
    public decimal? ThresholdValue { get; private set; }

    private RiskAlert() { }

    private RiskAlert(
        RiskAlertId id,
        RiskAlertType type,
        AlertPriority priority,
        string message,
        DateTime triggeredAt)
    {
        Id = id;
        Type = type;
        Priority = priority;
        Message = message;
        TriggeredAt = triggeredAt;
    }

    /// <summary>
    /// description(설명) : Create a new risk alert (새 리스크 경고 생성)
    /// Details(상세설명) : Factory method to create an alert when a risk threshold is breached (리스크 임계값 위반 시 경고를 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created RiskAlert instance (새로 생성된 리스크 경고 인스턴스)</returns>
    public static RiskAlert Create(
        RiskAlertType type,
        AlertPriority priority,
        string message,
        decimal? currentValue = null,
        decimal? thresholdValue = null,
        StockCode? stockCode = null,
        string? details = null)
    {
        var id = new RiskAlertId(Guid.NewGuid());
        var alert = new RiskAlert(id, type, priority, message, DateTime.UtcNow);

        alert.CurrentValue = currentValue;
        alert.ThresholdValue = thresholdValue;
        alert.StockCode = stockCode;
        alert.Details = details;

        return alert;
    }

    /// <summary>
    /// description(설명) : Resolve the alert (경고 해결)
    /// Details(상세설명) : Marks the alert as resolved when the risk condition is no longer present (리스크 조건이 더 이상 존재하지 않을 때 경고를 해결됨으로 표시)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Resolve()
    {
        if (ResolvedAt.HasValue)
            return;

        ResolvedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Get alert duration (경고 지속 시간 조회)
    /// Details(상세설명) : Calculates how long the alert has been active or was active before resolution (경고가 활성 상태였거나 해결 전까지 활성 상태였던 시간 계산)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Duration of the alert (경고의 지속 시간)</returns>
    public TimeSpan GetDuration()
    {
        var endTime = ResolvedAt ?? DateTime.UtcNow;
        return endTime - TriggeredAt;
    }
}

/// <summary>
/// description(설명) : Risk Alert identifier value object (리스크 경고 식별자 값 객체)
/// Details(상세설명) : Strongly-typed identifier for RiskAlert entity using GUID (GUID를 사용한 리스크 경고 엔티티의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class RiskAlertId : GuidId
{
    public RiskAlertId(Guid value) : base(value) { }

    public static RiskAlertId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Risk Alert Type enumeration (리스크 경고 유형 열거형)
/// Details(상세설명) : Categorizes different types of risk violations and warnings (다양한 유형의 리스크 위반 및 경고 분류)
/// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum RiskAlertType
{
    DailyLossExceeded = 1,      // Daily loss limit exceeded (일일 손실 한도 초과)
    TotalLossExceeded = 2,      // Total loss limit exceeded (총 손실 한도 초과)
    DrawdownExceeded = 3,       // Maximum drawdown exceeded (최대 낙폭 초과)
    PositionSizeExceeded = 4,   // Position size too large (포지션 크기가 너무 큼)
    MaxPositionsReached = 5,    // Maximum positions reached (최대 포지션 수 도달)
    MarginCallWarning = 6,      // Margin call warning (마진콜 경고)
    VolatilitySpike = 7,        // High volatility detected (높은 변동성 감지)
    PriceGap = 8,               // Large price gap detected (큰 가격 갭 감지)
    StopLossHit = 9,            // Stop loss triggered (손절매 발동)
    TakeProfitHit = 10          // Take profit triggered (익절매 발동)
}

/// <summary>
/// description(설명) : Alert Priority enumeration (경고 우선순위 열거형)
/// Details(상세설명) : Defines the urgency level of risk alerts (리스크 경고의 긴급도 수준 정의)
/// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum AlertPriority
{
    Low = 1,                    // Low priority, informational (낮은 우선순위, 정보성)
    Medium = 2,                 // Medium priority, requires attention (중간 우선순위, 주의 필요)
    High = 3,                   // High priority, action recommended (높은 우선순위, 조치 권장)
    Critical = 4                // Critical, immediate action required (긴급, 즉각 조치 필요)
}
