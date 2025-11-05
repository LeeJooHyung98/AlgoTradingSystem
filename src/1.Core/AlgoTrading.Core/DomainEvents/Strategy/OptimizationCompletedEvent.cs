namespace AlgoTrading.Core.DomainEvents.Strategy;

/// <summary>
/// description : 전략 파라미터 최적화가 완료되었을 때 발생하는 도메인 이벤트
/// Details : Strategy Context의 Optimization Service가 Grid Search 또는 Genetic Algorithm을 통해 최적 파라미터를 찾은 후 발행합니다. OptimizationMethod(최적화 방법), OptimalParameters(최적 파라미터 딕셔너리), BestPerformanceMetric(최고 성과 지표)을 포함하여 최적화 결과를 전달합니다. 이벤트 구독자는 TradingStrategy 엔티티의 파라미터를 자동으로 업데이트하고, 최적화 히스토리를 기록하며, 사용자에게 결과를 알립니다. 최적화는 계산 집약적 작업으로 백그라운드에서 비동기적으로 실행되므로, 이벤트를 통해 완료를 알립니다.
/// Applied technology patterns : Domain Event Pattern, Async Result Pattern, Parameter Optimization Pattern
/// </summary>
public class OptimizationCompletedEvent : DomainEvent
{
    /// <summary>
    /// 전략 ID
    /// </summary>
    public Guid StrategyId { get; }

    /// <summary>
    /// 최적화 방법
    /// </summary>
    public string OptimizationMethod { get; }

    /// <summary>
    /// 최적 파라미터
    /// </summary>
    public IDictionary<string, object> OptimalParameters { get; }

    /// <summary>
    /// 최적 성과 지표
    /// </summary>
    public decimal BestPerformanceMetric { get; }

    public OptimizationCompletedEvent(Guid strategyId, string optimizationMethod, IDictionary<string, object> optimalParameters, decimal bestPerformanceMetric)
    {
        StrategyId = strategyId;
        OptimizationMethod = optimizationMethod;
        OptimalParameters = optimalParameters;
        BestPerformanceMetric = bestPerformanceMetric;
    }
}
