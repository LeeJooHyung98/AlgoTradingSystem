namespace AlgoTrading.Core.DomainEvents.Strategy;

/// <summary>
/// description : 전략 백테스트 실행이 완료되었을 때 발생하는 도메인 이벤트
/// Details : Strategy Context의 Backtesting Service가 과거 데이터로 전략을 시뮬레이션하고 성과를 분석한 후 발행합니다. IsSuccessful 속성은 백테스트가 오류 없이 완료되었는지를 나타내며, 성공 시 TotalReturn(총 수익률)과 SharpeRatio(샤프 비율) 등의 성과 지표를 포함합니다. 이벤트 구독자는 백테스트 결과를 데이터베이스에 저장하고, 성과가 기준을 충족하면 전략을 Testing 상태로 승격시키며, 사용자에게 결과 알림을 전송합니다. Optimization Service는 여러 파라미터 조합의 백테스트 결과를 비교하여 최적 파라미터를 선택합니다.
/// Applied technology patterns : Domain Event Pattern, Result Event Pattern, Performance Metrics Pattern
/// </summary>
public class BacktestCompletedEvent : DomainEvent
{
    /// <summary>
    /// 백테스트 ID
    /// </summary>
    public Guid BacktestId { get; }

    /// <summary>
    /// 전략 ID
    /// </summary>
    public Guid StrategyId { get; }

    /// <summary>
    /// 백테스트 이름
    /// </summary>
    public string BacktestName { get; }

    /// <summary>
    /// 성공 여부
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// 총 수익률 (%)
    /// </summary>
    public decimal? TotalReturn { get; }

    /// <summary>
    /// 샤프 비율
    /// </summary>
    public decimal? SharpeRatio { get; }

    public BacktestCompletedEvent(Guid backtestId, Guid strategyId, string backtestName, bool isSuccessful, decimal? totalReturn = null, decimal? sharpeRatio = null)
    {
        BacktestId = backtestId;
        StrategyId = strategyId;
        BacktestName = backtestName;
        IsSuccessful = isSuccessful;
        TotalReturn = totalReturn;
        SharpeRatio = sharpeRatio;
    }
}
