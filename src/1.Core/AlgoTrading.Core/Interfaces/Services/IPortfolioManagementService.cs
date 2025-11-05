using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.Entities.Trading;

namespace AlgoTrading.Core.Interfaces.Services;

/// <summary>
/// description : 포트폴리오 생성, 리밸런싱, 성과 추적 서비스 인터페이스
/// Details : Account Context에서 포트폴리오의 생명주기를 관리하는 Application Service입니다. 포트폴리오 생성 시 초기 자본을 설정하고, 포지션 추가/업데이트/청산을 통해 포트폴리오 구성을 변경합니다. 실시간 가격 업데이트에 따라 포지션의 미실현 손익을 계산하고, 목표 자산 배분에 따라 리밸런싱을 수행합니다. 포트폴리오 총 가치는 현금 + 모든 포지션의 시가총액으로 계산되며, 성과 지표(수익률, 변동성)를 제공하여 대시보드에 표시합니다. CQRS 패턴의 Command Handler에서 호출됩니다.
/// Applied technology patterns : Application Service Pattern, CQRS Command Pattern, Portfolio Theory
/// </summary>
public interface IPortfolioManagementService
{
    /// <summary>
    /// 포트폴리오 생성
    /// </summary>
    Task<Portfolio> CreatePortfolioAsync(string name, Guid accountId, decimal initialCapital, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포지션 추가
    /// </summary>
    Task<Position> AddPositionAsync(Guid portfolioId, Position position, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포지션 업데이트
    /// </summary>
    Task UpdatePositionAsync(Guid positionId, decimal currentPrice, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포지션 청산
    /// </summary>
    Task<Trade> ClosePositionAsync(Guid positionId, decimal exitPrice, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포트폴리오 성과 계산
    /// </summary>
    Task<decimal> CalculatePortfolioPerformanceAsync(Guid portfolioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포트폴리오 리밸런싱
    /// </summary>
    Task RebalancePortfolioAsync(Guid portfolioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포트폴리오 총 가치 계산
    /// </summary>
    Task<decimal> CalculateTotalValueAsync(Guid portfolioId, CancellationToken cancellationToken = default);
}
