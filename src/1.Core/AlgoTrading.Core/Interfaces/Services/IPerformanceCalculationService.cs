using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.Entities.Strategy;

namespace AlgoTrading.Core.Interfaces.Services;

/// <summary>
/// description : 포트폴리오 및 전략의 성과 지표 계산 서비스 인터페이스
/// Details : Account Context와 Strategy Context에서 정량적 성과 평가 지표를 계산합니다. Sharpe Ratio(샤프 비율), Sortino Ratio(소티노 비율), Max Drawdown(최대 낙폭), Win Rate(승률), Profit Factor(손익비), Alpha/Beta 등 금융공학의 표준 지표를 제공합니다. 포트폴리오의 일별 수익률 시계열을 분석하여 리스크 조정 수익률을 계산하고, 벤치마크(KOSPI 지수 등)와 비교하여 상대적 성과를 평가합니다. 백테스트 결과 분석과 실전 운용 모니터링에 모두 활용되며, 전략 간 비교와 최적화의 목적 함수로 사용됩니다.
/// Applied technology patterns : Domain Service Pattern, Financial Metrics Pattern, Time Series Analysis
/// </summary>
public interface IPerformanceCalculationService
{
    /// <summary>
    /// 샤프 비율 계산
    /// </summary>
    Task<decimal> CalculateSharpeRatioAsync(Guid portfolioId, decimal riskFreeRate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 소티노 비율 계산
    /// </summary>
    Task<decimal> CalculateSortinoRatioAsync(Guid portfolioId, decimal riskFreeRate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 최대 낙폭 (Max Drawdown) 계산
    /// </summary>
    Task<decimal> CalculateMaxDrawdownAsync(Guid portfolioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 승률 계산
    /// </summary>
    Task<decimal> CalculateWinRateAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 손익비 (Profit Factor) 계산
    /// </summary>
    Task<decimal> CalculateProfitFactorAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 연간 수익률 계산
    /// </summary>
    Task<decimal> CalculateAnnualReturnAsync(Guid portfolioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 알파 (Alpha) 계산
    /// </summary>
    Task<decimal> CalculateAlphaAsync(Guid portfolioId, decimal benchmarkReturn, CancellationToken cancellationToken = default);

    /// <summary>
    /// 베타 (Beta) 계산
    /// </summary>
    Task<decimal> CalculateBetaAsync(Guid portfolioId, string benchmarkCode, CancellationToken cancellationToken = default);
}
