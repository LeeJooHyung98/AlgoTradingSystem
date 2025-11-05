using AlgoTrading.Core.Entities.Risk;
using AlgoTrading.Core.Entities.Trading;

namespace AlgoTrading.Core.Interfaces.Services;

/// <summary>
/// description : 포지션 크기 계산 서비스 인터페이스
/// Details : Risk Management Context에서 주문 수량을 결정하는 포지션 사이징 알고리즘을 제공합니다. Fixed Ratio(고정 비율), Kelly Criterion(켈리 공식), ATR 기반 변동성 조절 등 다양한 방법론을 지원하며, 리스크 프로필의 제약 조건(최대 포지션 크기, 계좌 잔고 대비 비율)을 검증합니다. 각 전략의 과거 성과(승률, 평균 손익비)와 현재 계좌 상태를 고려하여 최적의 포지션 크기를 계산함으로써 파산 리스크를 최소화하고 장기 수익을 극대화합니다. Application Layer에서 구현되며, Domain Service로 동작합니다.
/// Applied technology patterns : Domain Service Pattern, Strategy Pattern (알고리즘 선택), Money Management Pattern
/// </summary>
public interface IPositionSizingService
{
    /// <summary>
    /// 고정 비율 방식으로 포지션 크기 계산
    /// </summary>
    Task<int> CalculateFixedRatioSizeAsync(string accountNumber, string stockCode, decimal riskPercentage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kelly Criterion 방식으로 포지션 크기 계산
    /// </summary>
    Task<int> CalculateKellyCriterionSizeAsync(string accountNumber, string stockCode, decimal winRate, decimal avgWinLossRatio, CancellationToken cancellationToken = default);

    /// <summary>
    /// ATR 기반 포지션 크기 계산
    /// </summary>
    Task<int> CalculateATRBasedSizeAsync(string accountNumber, string stockCode, decimal riskAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// 리스크 프로필 기반 포지션 크기 계산
    /// </summary>
    Task<int> CalculateRiskBasedSizeAsync(RiskProfile riskProfile, Order order, CancellationToken cancellationToken = default);

    /// <summary>
    /// 최대 포지션 크기 검증
    /// </summary>
    Task<bool> ValidatePositionSizeAsync(string accountNumber, string stockCode, int quantity, CancellationToken cancellationToken = default);
}
