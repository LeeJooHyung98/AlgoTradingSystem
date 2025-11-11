using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlgoTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// description(설명) : Trade repository implementation (거래 리포지토리 구현)
/// Details(상세설명) : Implementation of ITradeRepository with EF Core (EF Core를 사용한 거래 리포지토리 구현)
/// Applied technology patterns(적용기술패턴) : Repository Pattern, Historical Data Pattern, Audit Trail Pattern (리포지토리 패턴, 이력 데이터 패턴, 감사 추적 패턴)
/// </summary>
public class TradeRepository : RepositoryBase<Trade>, ITradeRepository
{
    public TradeRepository(AlgoTradingDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Trade>> GetByAccountNumberAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.AccountNumber == accountNumber)
            .OrderByDescending(t => t.ExitTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetByStockCodeAsync(
        string stockCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.StockCode.Value == stockCode)
            .OrderByDescending(t => t.ExitTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.ExitTime >= startDate && t.ExitTime <= endDate)
            .OrderByDescending(t => t.ExitTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetByStrategyIdAsync(
        Guid strategyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.StrategyId == strategyId)
            .OrderByDescending(t => t.ExitTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetByPortfolioIdAsync(
        Guid portfolioId,
        CancellationToken cancellationToken = default)
    {
        // Note: Trade entity doesn't have PortfolioId in the current model
        // This is a placeholder implementation that will need to be updated
        // when the Trade entity is extended with PortfolioId
        throw new NotImplementedException(
            "Trade entity needs to be extended with PortfolioId property");
    }

    public async Task<IEnumerable<Trade>> GetProfitableTradesAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.AccountNumber == accountNumber && t.Result == TradeResult.Win)
            .OrderByDescending(t => t.ExitTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetLosingTradesAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.AccountNumber == accountNumber && t.Result == TradeResult.Loss)
            .OrderByDescending(t => t.ExitTime)
            .ToListAsync(cancellationToken);
    }
}
