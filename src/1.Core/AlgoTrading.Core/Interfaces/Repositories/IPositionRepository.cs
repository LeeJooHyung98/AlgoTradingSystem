using AlgoTrading.Core.Entities.Trading;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : Position 엔티티에 대한 데이터 접근 인터페이스
/// Details : Trading Context의 Position 엔티티를 관리하며, 열린 포지션(OpenPositions)과 닫힌 포지션(ClosedPositions)을 구분하여 조회합니다. 계좌별, 종목별, 전략별 포지션 필터링을 지원하며, 실시간 포지션 추적에 필요한 쿼리 메서드를 제공합니다. Position은 평균 매입가, 보유 수량, 미실현 손익(P&L)을 계산하는 비즈니스 로직을 포함합니다. Risk Management Context는 이 인터페이스를 통해 포지션 크기와 노출 비율을 모니터링합니다.
/// Applied technology patterns : Repository Pattern, Query Object Pattern, Domain Model Pattern
/// </summary>
public interface IPositionRepository : IRepository<Position>
{
    /// <summary>
    /// 계좌번호로 포지션 목록 조회
    /// </summary>
    Task<IEnumerable<Position>> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 종목코드로 포지션 조회
    /// </summary>
    Task<Position?> GetByStockCodeAsync(string accountNumber, string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포트폴리오 ID로 포지션 목록 조회
    /// </summary>
    Task<IEnumerable<Position>> GetByPortfolioIdAsync(Guid portfolioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 열린 포지션 목록 조회
    /// </summary>
    Task<IEnumerable<Position>> GetOpenPositionsAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 닫힌 포지션 목록 조회
    /// </summary>
    Task<IEnumerable<Position>> GetClosedPositionsAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 ID로 포지션 목록 조회
    /// </summary>
    Task<IEnumerable<Position>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);
}
