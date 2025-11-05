namespace AlgoTrading.Core.Exceptions.Risk;

/// <summary>
/// description : 최대 낙폭(Max Drawdown) 한도를 초과했을 때 발생하는 예외
/// Details : Risk Management Context에서 포트폴리오의 최대 낙폭이 RiskProfile에 정의된 한도를 초과할 때 발생합니다.
///           MaxDrawdown은 고점 대비 손실 비율로 계산되며(예: -20%), 이 한도를 초과하면 모든 포지션을 강제 청산하고 신규 거래를 차단합니다.
///           이 예외는 연속된 손실로 인한 계좌 파산을 방지하는 중요한 안전 장치이며, 발생 시 즉시 Critical Severity 알림이 전송됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Risk Management Pattern, Drawdown Protection
/// </summary>
public class MaxDrawdownExceededException : DomainException
{
    public decimal MaxDrawdownLimit { get; }
    public decimal CurrentDrawdown { get; }

    public MaxDrawdownExceededException(Guid portfolioId, decimal maxDrawdownLimit, decimal currentDrawdown)
        : base($"최대 낙폭 한도를 초과했습니다. PortfolioId: {portfolioId}, Limit: {maxDrawdownLimit:P2}, Current: {currentDrawdown:P2}",
            portfolioId, "MAX_DRAWDOWN_EXCEEDED")
    {
        MaxDrawdownLimit = maxDrawdownLimit;
        CurrentDrawdown = currentDrawdown;
    }
}
