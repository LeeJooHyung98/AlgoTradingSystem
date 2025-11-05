namespace AlgoTrading.Core.Exceptions.Strategy;

/// <summary>
/// description : 전략 최적화 프로세스가 실패했을 때 발생하는 예외
/// Details : Strategy Optimization Service에서 Genetic Algorithm, Grid Search, Walk-Forward Analysis 등 최적화 작업이 실패한 경우 발생합니다.
///           최적화는 수천 번의 백테스트를 반복 실행하므로, 시스템 리소스 부족, 수렴 실패, 타임아웃 등 다양한 이유로 실패할 수 있습니다.
///           이 예외는 사용자에게 최적화 실패를 알리고, 파라미터 범위를 조정하거나 최적화 알고리즘을 변경하도록 안내하는 데 사용됩니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Optimization Pattern, Exception Pattern
/// </summary>
public class OptimizationFailedException : DomainException
{
    public string OptimizationMethod { get; }

    public OptimizationFailedException(Guid strategyId, string optimizationMethod, string failureReason)
        : base($"전략 최적화에 실패했습니다. StrategyId: {strategyId}, Method: {optimizationMethod}, Reason: {failureReason}",
            strategyId, "OPTIMIZATION_FAILED")
    {
        OptimizationMethod = optimizationMethod;
    }

    public OptimizationFailedException(Guid strategyId, string optimizationMethod, string failureReason, Exception innerException)
        : base($"전략 최적화에 실패했습니다. StrategyId: {strategyId}, Method: {optimizationMethod}, Reason: {failureReason}",
            innerException, strategyId, "OPTIMIZATION_FAILED")
    {
        OptimizationMethod = optimizationMethod;
    }
}
