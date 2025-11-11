using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using StrategyStatus = AlgoTrading.Core.Entities.Strategy.StrategyStatus;
using StrategyType = AlgoTrading.Core.Entities.Strategy.StrategyType;

namespace AlgoTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// description : TradingStrategy repository implementation
/// Details : Provides data access for TradingStrategy aggregate root with EF Core
/// Applied technology patterns : Repository Pattern, Aggregate Root Pattern
/// </summary>
public class StrategyRepository : Repository<TradingStrategy, StrategyId>, IStrategyRepository
{
    public StrategyRepository(AlgoTradingDbContext context) : base(context)
    {
    }

    public async Task<TradingStrategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<TradingStrategy>> GetByStatusAsync(StrategyStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TradingStrategy>> GetByTypeAsync(StrategyType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Type == type)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TradingStrategy>> GetActiveStrategiesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == StrategyStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TradingStrategy>> GetByCreatedByAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.CreatedBy == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<TradingStrategy?> GetWithRulesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Rules)
            .FirstOrDefaultAsync(s => s.Id == new StrategyId(id), cancellationToken);
    }

    public async Task<TradingStrategy?> GetWithSignalsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Signals)
            .FirstOrDefaultAsync(s => s.Id == new StrategyId(id), cancellationToken);
    }
}
