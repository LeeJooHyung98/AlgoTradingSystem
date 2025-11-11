namespace AlgoTrading.Core.Enums;

/// <summary>
/// description(설명) : Optimization Type Enumeration (최적화 유형 열거형)
/// Details(상세설명) : Types of optimization algorithms for strategy parameter tuning (전략 매개변수 튜닝을 위한 최적화 알고리즘 유형)
/// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum OptimizationType
{
    /// <summary>Grid Search - Tests all parameter combinations (그리드 검색 - 모든 매개변수 조합 테스트)</summary>
    GridSearch = 1,

    /// <summary>Random Search - Random parameter sampling (랜덤 검색 - 무작위 매개변수 샘플링)</summary>
    RandomSearch = 2,

    /// <summary>Genetic Algorithm - Evolutionary optimization (유전 알고리즘 - 진화적 최적화)</summary>
    GeneticAlgorithm = 3,

    /// <summary>Bayesian Optimization - Probabilistic model-based optimization (베이지안 최적화 - 확률 모델 기반 최적화)</summary>
    BayesianOptimization = 4,

    /// <summary>Walk-Forward Analysis - Rolling time window testing (워크포워드 분석 - 롤링 시간 윈도우 테스팅)</summary>
    WalkForward = 5
}
