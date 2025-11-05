using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Risk;

/// <summary>
/// description(설명) : Risk Monitor Aggregate Root - Monitors portfolio risk metrics in real-time (리스크 모니터 집합 루트 - 포트폴리오 리스크 지표를 실시간으로 모니터링)
/// Details(상세설명) : Tracks current portfolio value, P/L, positions, drawdown and generates risk alerts when limits are breached (현재 포트폴리오 가치, 손익, 포지션, 낙폭 추적 및 한도 위반 시 리스크 경고 생성)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern (집합 루트 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class RiskMonitor : AggregateRoot<RiskMonitorId>
{
    private readonly List<RiskAlert> _alerts = new();

    /// <summary>
    /// 모니터링 대상 리스크 프로필 ID
    /// </summary>
    public RiskProfileId RiskProfileId { get; private set; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; private set; }

    /// <summary>
    /// 현재 포트폴리오 가치
    /// </summary>
    public Money CurrentPortfolioValue { get; private set; }

    /// <summary>
    /// 금일 손익
    /// </summary>
    public Money TodayPL { get; private set; }

    /// <summary>
    /// 현재 보유 포지션 수
    /// </summary>
    public int OpenPositionsCount { get; private set; }

    /// <summary>
    /// 현재 낙폭 비율 (%)
    /// </summary>
    public decimal CurrentDrawdownPercent { get; private set; }

    /// <summary>
    /// 최고 포트폴리오 가치
    /// </summary>
    public Money PeakPortfolioValue { get; private set; }

    /// <summary>
    /// 활성 경고 목록
    /// </summary>
    public IReadOnlyList<RiskAlert> ActiveAlerts => _alerts.Where(a => a.IsActive).ToList().AsReadOnly();

    /// <summary>
    /// 마지막 업데이트 시간
    /// </summary>
    public DateTime LastUpdatedAt { get; private set; }

    /// <summary>
    /// 모니터링 상태
    /// </summary>
    public MonitoringStatus Status { get; private set; }

    private RiskMonitor() { }

    private RiskMonitor(
        RiskMonitorId id,
        RiskProfileId riskProfileId,
        string accountNumber)
    {
        Id = id;
        RiskProfileId = riskProfileId;
        AccountNumber = accountNumber;
        Status = MonitoringStatus.Active;
        CurrentPortfolioValue = Money.Zero;
        TodayPL = Money.Zero;
        PeakPortfolioValue = Money.Zero;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// description(설명) : Create a new risk monitor (새 리스크 모니터 생성)
    /// Details(상세설명) : Factory method to create a real-time risk monitor for an account (계좌에 대한 실시간 리스크 모니터 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created RiskMonitor instance (새로 생성된 리스크 모니터 인스턴스)</returns>
    public static RiskMonitor Create(
        RiskProfileId riskProfileId,
        string accountNumber)
    {
        var id = new RiskMonitorId(Guid.NewGuid());
        return new RiskMonitor(id, riskProfileId, accountNumber);
    }

    /// <summary>
    /// description(설명) : Update portfolio metrics (포트폴리오 지표 업데이트)
    /// Details(상세설명) : Updates current portfolio value, P/L, positions and calculates drawdown (현재 포트폴리오 가치, 손익, 포지션 업데이트 및 낙폭 계산)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateMetrics(
        Money portfolioValue,
        Money todayPL,
        int openPositionsCount)
    {
        CurrentPortfolioValue = portfolioValue;
        TodayPL = todayPL;
        OpenPositionsCount = openPositionsCount;

        // Update peak value
        if (portfolioValue > PeakPortfolioValue)
        {
            PeakPortfolioValue = portfolioValue;
        }

        // Calculate drawdown
        if (PeakPortfolioValue.Amount > 0)
        {
            CurrentDrawdownPercent = ((PeakPortfolioValue.Amount - portfolioValue.Amount) / PeakPortfolioValue.Amount) * 100;
        }

        LastUpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Add risk alert (리스크 경고 추가)
    /// Details(상세설명) : Adds a new risk alert when a risk threshold is breached (리스크 임계값 위반 시 새 리스크 경고 추가)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void AddAlert(RiskAlert alert)
    {
        _alerts.Add(alert);
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Resolve alert (경고 해결)
    /// Details(상세설명) : Marks a risk alert as resolved when the risk condition is cleared (리스크 조건이 해소되면 리스크 경고를 해결됨으로 표시)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void ResolveAlert(RiskAlertId alertId)
    {
        var alert = _alerts.FirstOrDefault(a => a.Id == alertId);
        if (alert != null)
        {
            alert.Resolve();
            MarkAsModified();
        }
    }

    /// <summary>
    /// description(설명) : Start monitoring (모니터링 시작)
    /// Details(상세설명) : Activates real-time risk monitoring (실시간 리스크 모니터링 활성화)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Start()
    {
        Status = MonitoringStatus.Active;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Stop monitoring (모니터링 중지)
    /// Details(상세설명) : Deactivates real-time risk monitoring (실시간 리스크 모니터링 비활성화)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Stop()
    {
        Status = MonitoringStatus.Stopped;
        MarkAsModified();
    }
}

/// <summary>
/// description(설명) : Risk Monitor identifier value object (리스크 모니터 식별자 값 객체)
/// Details(상세설명) : Strongly-typed identifier for RiskMonitor entity using GUID (GUID를 사용한 리스크 모니터 엔티티의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class RiskMonitorId : GuidId
{
    public RiskMonitorId(Guid value) : base(value) { }

    public static RiskMonitorId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Monitoring status enumeration (모니터링 상태 열거형)
/// Details(상세설명) : Tracks the current status of risk monitoring activity (리스크 모니터링 활동의 현재 상태 추적)
/// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum MonitoringStatus
{
    Active = 1,                 // Currently monitoring (현재 모니터링 중)
    Paused = 2,                 // Temporarily paused (일시 중지됨)
    Stopped = 3                 // Stopped (중지됨)
}
