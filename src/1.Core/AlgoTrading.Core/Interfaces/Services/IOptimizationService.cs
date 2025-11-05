using AlgoTrading.Core.Entities.Strategy;

namespace AlgoTrading.Core.Interfaces.Services;

/// <summary>
/// description : 전략 파라미터 최적화 서비스 인터페이스
/// Details : Strategy Context에서 트레이딩 전략의 파라미터(예: 이동평균 기간, RSI 임계값)를 최적화하여 백테스트 성과를 극대화합니다. Grid Search(전수 탐색)와 Genetic Algorithm(유전 알고리즘)을 지원하며, 각 파라미터 조합에 대해 백테스트를 수행하고 Sharpe Ratio, 총 수익률, Max Drawdown 등의 목적 함수를 평가합니다. Overfitting을 방지하기 위해 In-Sample/Out-of-Sample 분리와 Walk-Forward Analysis를 결합하며, 최적 파라미터 세트를 반환하여 전략에 자동 적용합니다. 계산 집약적 작업이므로 병렬 처리와 캐싱을 활용합니다.
/// Applied technology patterns : Domain Service Pattern, Optimization Algorithms, Parallel Processing Pattern
/// </summary>
public interface IOptimizationService
{
    /// <summary>
    /// 그리드 서치 최적화 실행
    /// </summary>
    Task<TradingStrategy> GridSearchOptimizationAsync(Guid strategyId, IDictionary<string, IEnumerable<object>> parameterGrid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 유전 알고리즘 최적화 실행
    /// </summary>
    Task<TradingStrategy> GeneticAlgorithmOptimizationAsync(Guid strategyId, int populationSize, int generations, CancellationToken cancellationToken = default);

    /// <summary>
    /// 파라미터 평가
    /// </summary>
    Task<decimal> EvaluateParametersAsync(Guid strategyId, IDictionary<string, object> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// 최적 파라미터 선택
    /// </summary>
    Task<IDictionary<string, object>> SelectBestParametersAsync(Guid strategyId, IEnumerable<IDictionary<string, object>> parameterSets, CancellationToken cancellationToken = default);
}
