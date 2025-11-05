using AlgoTrading.Core.Entities.Strategy;

namespace AlgoTrading.Core.Interfaces.Services;

/// <summary>
/// description : 전략 백테스트 실행 및 성과 분석 서비스 인터페이스
/// Details : Strategy Context에서 과거 데이터를 사용하여 트레이딩 전략을 시뮬레이션하고 성과를 평가합니다. 지정된 기간의 OHLC 데이터를 순차적으로 읽어 신호를 생성하고, 가상의 포트폴리오에서 주문을 체결하여 손익을 계산합니다. 백테스트 결과는 Backtest 엔티티에 저장되며, 총 수익률, Sharpe Ratio, Max Drawdown, 거래 횟수 등의 지표를 포함합니다. Walk-Forward Analysis는 과최적화(Overfitting)를 방지하기 위해 데이터를 여러 구간으로 나누어 반복적으로 백테스트를 수행하고, Out-of-Sample 성과를 검증합니다.
/// Applied technology patterns : Domain Service Pattern, Simulation Pattern, Walk-Forward Validation
/// </summary>
public interface IBacktestingService
{
    /// <summary>
    /// 백테스트 실행
    /// </summary>
    Task<Backtest> RunBacktestAsync(Guid strategyId, DateTime startDate, DateTime endDate, decimal initialCapital, CancellationToken cancellationToken = default);

    /// <summary>
    /// 백테스트 결과 계산
    /// </summary>
    Task CalculateBacktestResultsAsync(Guid backtestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 백테스트 성과 지표 계산
    /// </summary>
    Task CalculatePerformanceMetricsAsync(Guid backtestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Walk-Forward 분석 실행
    /// </summary>
    Task<IEnumerable<Backtest>> RunWalkForwardAnalysisAsync(Guid strategyId, DateTime startDate, DateTime endDate, int windowSize, int stepSize, CancellationToken cancellationToken = default);
}
