namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 트레이딩 전략의 분석 방법론에 따른 카테고리 분류
/// Details : 알고리즘 트레이딩 전략을 기술적 분석(Technical), 기본적 분석(Fundamental), 통계적 분석(Statistical), 머신러닝(MachineLearning), 혼합(Hybrid) 방식으로 분류합니다. Strategy Context에서 전략 생성 시 카테고리를 지정하여 전략의 특성과 적용 가능한 최적화 알고리즘을 결정합니다. 예를 들어 Technical 전략은 지표 기반 파라미터 최적화를, MachineLearning 전략은 모델 학습 기반 최적화를 수행합니다.
/// Applied technology patterns : Strategy Pattern (GoF), Domain-Driven Design Classification
/// </summary>
public enum StrategyCategory
{
    /// <summary>
    /// 기술적 분석 기반 전략
    /// </summary>
    Technical = 1,

    /// <summary>
    /// 기본적 분석 기반 전략
    /// </summary>
    Fundamental = 2,

    /// <summary>
    /// 통계적 분석 기반 전략
    /// </summary>
    Statistical = 3,

    /// <summary>
    /// 머신러닝 기반 전략
    /// </summary>
    MachineLearning = 4,

    /// <summary>
    /// 혼합 전략
    /// </summary>
    Hybrid = 5
}
