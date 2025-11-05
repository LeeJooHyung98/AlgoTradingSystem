namespace AlgoTrading.Core.DomainEvents.Account;

/// <summary>
/// description : 포트폴리오의 성과 지표가 재계산되어 업데이트되었을 때 발생하는 도메인 이벤트
/// Details : Account Context에서 Portfolio 엔티티의 성과 지표(TotalReturn, SharpeRatio, MaxDrawdown, WinRate)가 주기적으로 재계산되거나 거래 체결 후 업데이트될 때 발행됩니다. 성과 지표는 일별, 주별, 월별로 계산되며, 포트폴리오의 리스크 조정 수익률과 전략 효과성을 평가하는 데 사용됩니다. 이벤트 구독자는 업데이트된 지표를 대시보드에 실시간으로 표시하고, 성과가 목표 기준을 초과하거나 미달하면 사용자에게 알림을 전송하며, 성과 히스토리를 시계열 데이터베이스에 저장하여 트렌드 분석을 지원합니다.
/// Applied technology patterns : Domain Event Pattern, Performance Metrics Pattern, Real-time Analytics Pattern
/// </summary>
public class PerformanceMetricsUpdatedEvent : DomainEvent
{
    /// <summary>
    /// 포트폴리오 ID
    /// </summary>
    public Guid PortfolioId { get; }

    /// <summary>
    /// 총 수익률 (%)
    /// </summary>
    public decimal TotalReturn { get; }

    /// <summary>
    /// 샤프 비율
    /// </summary>
    public decimal SharpeRatio { get; }

    /// <summary>
    /// 최대 낙폭 (%)
    /// </summary>
    public decimal MaxDrawdown { get; }

    /// <summary>
    /// 승률 (%)
    /// </summary>
    public decimal WinRate { get; }

    public PerformanceMetricsUpdatedEvent(Guid portfolioId, decimal totalReturn, decimal sharpeRatio, decimal maxDrawdown, decimal winRate)
    {
        PortfolioId = portfolioId;
        TotalReturn = totalReturn;
        SharpeRatio = sharpeRatio;
        MaxDrawdown = maxDrawdown;
        WinRate = winRate;
    }
}
