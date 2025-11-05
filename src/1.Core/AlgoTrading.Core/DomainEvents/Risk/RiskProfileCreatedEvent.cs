namespace AlgoTrading.Core.DomainEvents.Risk;

/// <summary>
/// description : 새로운 리스크 프로필이 생성되었을 때 발생하는 도메인 이벤트
/// Details : Risk Management Context에서 RiskProfile 집합근이 생성될 때 발행됩니다. 리스크 프로필은 계좌의 리스크 관리 정책(MaxDailyLoss, MaxTotalLoss, 포지션 한도 등)을 정의하며, 계좌당 하나 이상의 프로필을 가질 수 있습니다. 이벤트 구독자는 해당 계좌에 RiskMonitor 인스턴스를 생성하여 실시간 모니터링을 시작하고, 리스크 프로필을 계좌의 모든 활성 전략에 적용하며, 사용자에게 프로필 생성 알림을 전송합니다. 프로필 이름, 계좌번호, 주요 리스크 한도를 포함하여 감사 추적을 제공합니다.
/// Applied technology patterns : Domain Event Pattern, Policy Creation Event, Risk Management Pattern
/// </summary>
public class RiskProfileCreatedEvent : DomainEvent
{
    /// <summary>
    /// 리스크 프로필 ID
    /// </summary>
    public Guid RiskProfileId { get; }

    /// <summary>
    /// 프로필 이름
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string AccountNumber { get; }

    /// <summary>
    /// 최대 일일 손실 한도
    /// </summary>
    public decimal MaxDailyLoss { get; }

    /// <summary>
    /// 최대 총 손실 한도
    /// </summary>
    public decimal MaxTotalLoss { get; }

    public RiskProfileCreatedEvent(Guid riskProfileId, string name, string accountNumber, decimal maxDailyLoss, decimal maxTotalLoss)
    {
        RiskProfileId = riskProfileId;
        Name = name;
        AccountNumber = accountNumber;
        MaxDailyLoss = maxDailyLoss;
        MaxTotalLoss = maxTotalLoss;
    }
}
