namespace AlgoTrading.Core.DomainEvents.Account;

/// <summary>
/// description : 신용 거래 계좌에서 마진콜(증거금 추가 요구)이 발생했을 때 발생하는 도메인 이벤트
/// Details : Account Context에서 Margin 계좌의 현재 마진 비율이 유지 마진 비율(MaintenanceMarginRatio) 이하로 떨어졌을 때 발행됩니다. CurrentMarginRatio(현재 마진 비율), RequiredDeposit(필요 입금액)을 포함하여 마진콜 상세 정보를 제공합니다. 이벤트 구독자는 모든 신규 주문을 즉시 차단하고, 사용자에게 긴급 알림(Critical Severity)을 모든 채널로 전송하며, 입금이 이루어지지 않으면 자동으로 포지션을 강제 청산합니다. 마진콜은 레버리지 거래의 리스크를 관리하는 중요한 안전 장치이며, 발생 내역은 규제 보고를 위해 영구 기록됩니다.
/// Applied technology patterns : Domain Event Pattern, Margin Management Pattern, Critical Alert Pattern
/// </summary>
public class MarginCallTriggeredEvent : DomainEvent
{
    /// <summary>
    /// 계좌 ID
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// 계좌번호
    /// </summary>
    public string AccountNumber { get; }

    /// <summary>
    /// 현재 마진 비율 (%)
    /// </summary>
    public decimal CurrentMarginRatio { get; }

    /// <summary>
    /// 유지 마진 비율 (%)
    /// </summary>
    public decimal MaintenanceMarginRatio { get; }

    /// <summary>
    /// 필요 입금액
    /// </summary>
    public decimal RequiredDeposit { get; }

    public MarginCallTriggeredEvent(Guid accountId, string accountNumber, decimal currentMarginRatio, decimal maintenanceMarginRatio, decimal requiredDeposit)
    {
        AccountId = accountId;
        AccountNumber = accountNumber;
        CurrentMarginRatio = currentMarginRatio;
        MaintenanceMarginRatio = maintenanceMarginRatio;
        RequiredDeposit = requiredDeposit;
    }
}
