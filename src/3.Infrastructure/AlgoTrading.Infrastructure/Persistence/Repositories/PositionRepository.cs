using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlgoTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// description(설명) : Position repository implementation (포지션 리포지토리 구현)
/// Details(상세설명) : Implementation of IPositionRepository with EF Core (EF Core를 사용한 포지션 리포지토리 구현)
/// Applied technology patterns(적용기술패턴) : Repository Pattern, Query Object Pattern, Domain Model Pattern (리포지토리 패턴, 쿼리 객체 패턴, 도메인 모델 패턴)
/// </summary>
public class PositionRepository : RepositoryBase<Position>, IPositionRepository
{
    public PositionRepository(AlgoTradingDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Position>> GetByAccountNumberAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.AccountNumber == accountNumber)
            .OrderByDescending(p => p.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Position?> GetByStockCodeAsync(
        string accountNumber,
        string stockCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(
                p => p.AccountNumber == accountNumber && p.StockCode.Value == stockCode,
                cancellationToken);
    }

    public async Task<IEnumerable<Position>> GetByPortfolioIdAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        // Note: Position entity doesn't have PortfolioId in the current model
        // This is a placeholder implementation that will need to be updated
        // when the Position entity is extended with PortfolioId
        throw new NotImplementedException(
            "Position entity needs to be extended with PortfolioId property");
    }

    public async Task<IEnumerable<Position>> GetOpenPositionsAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.AccountNumber == accountNumber && p.Quantity > 0)
            .OrderByDescending(p => p.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Position>> GetClosedPositionsAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.AccountNumber == accountNumber && p.Quantity == 0)
            .OrderByDescending(p => p.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Position>> GetByStrategyIdAsync(
        Guid strategyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StrategyId == strategyId)
            .OrderByDescending(p => p.OpenedAt)
            .ToListAsync(cancellationToken);
    }
}
