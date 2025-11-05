namespace AlgoTrading.Core.Exceptions.Account;

/// <summary>
/// description : 마진콜(증거금 추가 요구) 상태에서 신규 거래를 시도할 때 발생하는 예외
/// Details : Margin 계좌에서 마진 비율이 유지 마진 비율(MaintenanceMarginRatio) 이하로 떨어져 마진콜이 발생한 상태에서 신규 주문을 시도하면 발생합니다.
///           마진콜 상태에서는 모든 신규 매수가 차단되며, 사용자는 반드시 추가 입금 또는 기존 포지션 청산을 통해 마진 비율을 회복해야 합니다.
///           이 예외는 레버리지 거래의 리스크를 관리하는 중요한 안전 장치이며, 강제 청산(Liquidation)을 방지하는 데 기여합니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Margin Management Pattern, Risk Protection Pattern
/// </summary>
public class MarginCallException : DomainException
{
    public decimal CurrentMarginRatio { get; }
    public decimal MaintenanceMarginRatio { get; }
    public decimal RequiredDeposit { get; }

    public MarginCallException(Guid accountId, decimal currentMarginRatio, decimal maintenanceMarginRatio, decimal requiredDeposit)
        : base($"마진콜 상태로 신규 거래가 차단되었습니다. AccountId: {accountId}, CurrentMargin: {currentMarginRatio:P2}, MaintenanceMargin: {maintenanceMarginRatio:P2}, RequiredDeposit: {requiredDeposit:N2}",
            accountId, "MARGIN_CALL")
    {
        CurrentMarginRatio = currentMarginRatio;
        MaintenanceMarginRatio = maintenanceMarginRatio;
        RequiredDeposit = requiredDeposit;
    }
}
