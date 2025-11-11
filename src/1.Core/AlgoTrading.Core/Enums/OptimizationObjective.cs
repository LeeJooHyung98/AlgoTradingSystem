namespace AlgoTrading.Core.Enums;

/// <summary>
/// description(설명) : Optimization Objective Enumeration (최적화 목표 열거형)
/// Details(상세설명) : Objective functions for optimization algorithms (최적화 알고리즘을 위한 목적 함수)
/// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
/// </summary>
public enum OptimizationObjective
{
    /// <summary>Maximize Total Return (총 수익률 최대화)</summary>
    MaxReturn = 1,

    /// <summary>Maximize Sharpe Ratio (샤프 비율 최대화)</summary>
    MaxSharpe = 2,

    /// <summary>Minimize Maximum Drawdown (최대 낙폭 최소화)</summary>
    MinDrawdown = 3,

    /// <summary>Maximize Profit Factor (수익 인수 최대화)</summary>
    MaxProfitFactor = 4,

    /// <summary>Maximize Win Rate (승률 최대화)</summary>
    MaxWinRate = 5,

    /// <summary>Maximize Risk-Adjusted Return (위험 조정 수익률 최대화)</summary>
    MaxRiskAdjustedReturn = 6,

    /// <summary>Minimize Trade Count (거래 횟수 최소화)</summary>
    MinTradeCount = 7,

    /// <summary>Maximize Average Win (평균 승리 최대화)</summary>
    MaxAverageWin = 8
}
