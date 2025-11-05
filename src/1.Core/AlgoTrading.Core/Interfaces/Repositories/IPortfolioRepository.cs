using AlgoTrading.Core.Entities.Portfolio;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : Portfolio 집합근(Aggregate Root)에 대한 데이터 접근 인터페이스
/// Details : Account Context의 Portfolio 엔티티에 대한 CRUD 및 조회 메서드를 정의합니다. 계좌별, 사용자별 포트폴리오 조회, 포지션 및 거래내역을 Eager Loading하는 메서드를 제공하여 N+1 쿼리 문제를 방지합니다. Infrastructure Layer에서 EF Core를 사용하여 구현되며, Include() 메서드로 연관 엔티티를 로드합니다. 포트폴리오는 여러 포지션을 보유하고 거래 내역을 추적하는 Aggregate Root입니다.
/// Applied technology patterns : Repository Pattern (DDD), Unit of Work Pattern, Specification Pattern
/// </summary>
public interface IPortfolioRepository : IRepository<Portfolio>
{
    /// <summary>
    /// 계좌 ID로 포트폴리오 조회
    /// </summary>
    Task<Portfolio?> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 이름으로 포트폴리오 조회
    /// </summary>
    Task<Portfolio?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 사용자 ID로 포트폴리오 목록 조회
    /// </summary>
    Task<IEnumerable<Portfolio>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포지션 포함하여 조회
    /// </summary>
    Task<Portfolio?> GetWithPositionsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 거래 내역 포함하여 조회
    /// </summary>
    Task<Portfolio?> GetWithTransactionsAsync(Guid id, CancellationToken cancellationToken = default);
}
