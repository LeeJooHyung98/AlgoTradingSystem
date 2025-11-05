using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Risk;

/// <summary>
/// description(설명) : Risk Profile Aggregate Root - Represents risk management rules and limits for an account (리스크 프로필 집합 루트 - 계좌의 리스크 관리 규칙 및 한도를 나타냄)
/// Details(상세설명) : Defines risk parameters including position limits, stop-loss, take-profit, and maximum drawdown thresholds (포지션 한도, 손절매, 익절매, 최대 낙폭 임계값 등 리스크 파라미터 정의)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern (집합 루트 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class RiskProfile : AggregateRoot<RiskProfileId>
{
    /// <summary>
    /// 프로필 이름
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 계좌 번호
    /// </summary>
    public string AccountNumber { get; private set; }

    /// <summary>
    /// 리스크 수준
    /// </summary>
    public RiskLevel RiskLevel { get; private set; }

    /// <summary>
    /// 최대 포지션 크기 (포트폴리오 대비 %)
    /// </summary>
    public decimal MaxPositionSizePercent { get; private set; }

    /// <summary>
    /// 일일 최대 손실 한도
    /// </summary>
    public Money MaxDailyLoss { get; private set; }

    /// <summary>
    /// 총 최대 손실 한도
    /// </summary>
    public Money MaxTotalLoss { get; private set; }

    /// <summary>
    /// 최대 낙폭 비율 (%)
    /// </summary>
    public decimal MaxDrawdownPercent { get; private set; }

    /// <summary>
    /// 최대 동시 보유 포지션 수
    /// </summary>
    public int MaxOpenPositions { get; private set; }

    /// <summary>
    /// 기본 손절매 비율 (%)
    /// </summary>
    public decimal DefaultStopLossPercent { get; private set; }

    /// <summary>
    /// 기본 익절매 비율 (%)
    /// </summary>
    public decimal DefaultTakeProfitPercent { get; private set; }

    /// <summary>
    /// 프로필 활성화 여부
    /// </summary>
    public bool IsActive { get; private set; }

    private RiskProfile() { }

    private RiskProfile(
        RiskProfileId id,
        string name,
        string accountNumber,
        RiskLevel riskLevel)
    {
        Id = id;
        Name = name;
        AccountNumber = accountNumber;
        RiskLevel = riskLevel;
        IsActive = true;

        // Set defaults based on risk level
        SetDefaultsByRiskLevel(riskLevel);
    }

    /// <summary>
    /// description(설명) : Create a new risk profile (새 리스크 프로필 생성)
    /// Details(상세설명) : Factory method to create risk profile with predefined limits based on risk level (리스크 수준에 따라 미리 정의된 한도로 리스크 프로필 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴), Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    /// <returns>Newly created RiskProfile instance (새로 생성된 리스크 프로필 인스턴스)</returns>
    public static RiskProfile Create(
        string name,
        string accountNumber,
        RiskLevel riskLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Profile name cannot be empty", nameof(name));

        var id = new RiskProfileId(Guid.NewGuid());
        return new RiskProfile(id, name, accountNumber, riskLevel);
    }

    /// <summary>
    /// description(설명) : Set risk limits (리스크 한도 설정)
    /// Details(상세설명) : Configures risk management limits including position size, loss limits, and max positions (포지션 크기, 손실 한도, 최대 포지션 수 등 리스크 관리 한도 설정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void SetLimits(
        decimal maxPositionSizePercent,
        Money maxDailyLoss,
        Money maxTotalLoss,
        decimal maxDrawdownPercent,
        int maxOpenPositions)
    {
        if (maxPositionSizePercent <= 0 || maxPositionSizePercent > 100)
            throw new ArgumentException("Max position size must be between 0 and 100", nameof(maxPositionSizePercent));

        if (maxDrawdownPercent <= 0 || maxDrawdownPercent > 100)
            throw new ArgumentException("Max drawdown must be between 0 and 100", nameof(maxDrawdownPercent));

        if (maxOpenPositions <= 0)
            throw new ArgumentException("Max open positions must be positive", nameof(maxOpenPositions));

        MaxPositionSizePercent = maxPositionSizePercent;
        MaxDailyLoss = maxDailyLoss;
        MaxTotalLoss = maxTotalLoss;
        MaxDrawdownPercent = maxDrawdownPercent;
        MaxOpenPositions = maxOpenPositions;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Set default stop-loss and take-profit (기본 손절매 및 익절매 설정)
    /// Details(상세설명) : Sets default percentage thresholds for automatic stop-loss and take-profit orders (자동 손절매 및 익절매 주문을 위한 기본 비율 임계값 설정)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void SetDefaultStopLossTakeProfit(decimal stopLossPercent, decimal takeProfitPercent)
    {
        if (stopLossPercent <= 0 || stopLossPercent > 100)
            throw new ArgumentException("Stop loss must be between 0 and 100", nameof(stopLossPercent));

        if (takeProfitPercent <= 0)
            throw new ArgumentException("Take profit must be positive", nameof(takeProfitPercent));

        DefaultStopLossPercent = stopLossPercent;
        DefaultTakeProfitPercent = takeProfitPercent;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Activate profile (프로필 활성화)
    /// Details(상세설명) : Enables the risk profile for active risk management monitoring (활성 리스크 관리 모니터링을 위해 리스크 프로필 활성화)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Deactivate profile (프로필 비활성화)
    /// Details(상세설명) : Disables the risk profile to pause risk management monitoring (리스크 관리 모니터링을 일시 중지하기 위해 리스크 프로필 비활성화)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        MarkAsModified();
    }

    private void SetDefaultsByRiskLevel(RiskLevel level)
    {
        switch (level)
        {
            case RiskLevel.Conservative:
                MaxPositionSizePercent = 5;
                MaxDrawdownPercent = 10;
                MaxOpenPositions = 3;
                DefaultStopLossPercent = 3;
                DefaultTakeProfitPercent = 6;
                break;

            case RiskLevel.Moderate:
                MaxPositionSizePercent = 10;
                MaxDrawdownPercent = 20;
                MaxOpenPositions = 5;
                DefaultStopLossPercent = 5;
                DefaultTakeProfitPercent = 10;
                break;

            case RiskLevel.Aggressive:
                MaxPositionSizePercent = 20;
                MaxDrawdownPercent = 30;
                MaxOpenPositions = 10;
                DefaultStopLossPercent = 8;
                DefaultTakeProfitPercent = 15;
                break;
        }

        MaxDailyLoss = Money.Zero;
        MaxTotalLoss = Money.Zero;
    }
}

/// <summary>
/// description(설명) : Risk Profile identifier value object (리스크 프로필 식별자 값 객체)
/// Details(상세설명) : Strongly-typed identifier for RiskProfile entity using GUID (GUID를 사용한 리스크 프로필 엔티티의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern (값 객체 패턴), Domain-Driven Design (도메인 주도 설계)
/// </summary>
public sealed class RiskProfileId : GuidId
{
    public RiskProfileId(Guid value) : base(value) { }

    public static RiskProfileId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Risk level enumeration (리스크 수준 열거형)
/// Details(상세설명) : Defines risk tolerance levels from conservative to aggressive (보수적부터 공격적까지 리스크 허용 수준 정의)
/// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum RiskLevel
{
    Conservative = 1,           // Low risk, small positions (낮은 리스크, 작은 포지션)
    Moderate = 2,               // Medium risk, moderate positions (중간 리스크, 중간 포지션)
    Aggressive = 3              // High risk, large positions (높은 리스크, 큰 포지션)
}
