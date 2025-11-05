using AlgoTrading.Core.Entities.Strategy;

namespace AlgoTrading.Core.Interfaces.Services;

/// <summary>
/// description : 트레이딩 신호 생성 및 평가 서비스 인터페이스
/// Details : Strategy Context에서 트레이딩 전략의 규칙을 평가하여 매매 신호(Buy/Sell/Hold)를 생성합니다. 전략의 StrategyRule들을 순차적으로 평가하고, 모든 진입 규칙이 충족되면 SignalGeneratedEvent를 발행합니다. 기술적 지표(RSI, MACD 등), 가격 패턴, 거래량 조건 등을 종합적으로 분석하며, 신호의 신뢰도(Confidence)를 0~1 범위로 계산하여 포지션 크기 조절에 활용합니다. 실시간 시장 데이터와 과거 가격 데이터를 결합하여 백테스트와 실전 거래 모두에서 일관된 신호를 생성합니다.
/// Applied technology patterns : Domain Service Pattern, Rule Engine Pattern, Event-Driven Architecture
/// </summary>
public interface ISignalGenerationService
{
    /// <summary>
    /// 전략 신호 생성
    /// </summary>
    Task<StrategySignal> GenerateSignalAsync(Guid strategyId, string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 규칙 평가
    /// </summary>
    Task<bool> EvaluateRuleAsync(StrategyRule rule, string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 여러 종목에 대한 신호 생성
    /// </summary>
    Task<IEnumerable<StrategySignal>> GenerateSignalsForStocksAsync(Guid strategyId, IEnumerable<string> stockCodes, CancellationToken cancellationToken = default);

    /// <summary>
    /// 신호 검증
    /// </summary>
    Task<bool> ValidateSignalAsync(StrategySignal signal, CancellationToken cancellationToken = default);
}
