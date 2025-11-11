using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AlgoTrading.IntegrationTests.TestHelpers;

/// <summary>
/// Test-specific implementation of IStrategyRepository for Integration Tests
/// Uses StrategyTestDbContext instead of full AlgoTradingDbContext
/// </summary>
public class TestStrategyRepository : IStrategyRepository
{
    private readonly StrategyTestDbContext _context;
    private readonly DbSet<TradingStrategy> _dbSet;

    public TestStrategyRepository(StrategyTestDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TradingStrategy>();
    }

    public async Task<TradingStrategy?> GetByIdAsync(StrategyId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<TradingStrategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
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

    public async Task<TradingStrategy?> GetWithRulesAsync(Guid strategyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Id.Value == strategyId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TradingStrategy?> GetWithSignalsAsync(Guid strategyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Id.Value == strategyId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TradingStrategy>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TradingStrategy>> FindAsync(
        Expression<Func<TradingStrategy, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TradingStrategy strategy, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(strategy, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TradingStrategy> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(TradingStrategy strategy)
    {
        _dbSet.Update(strategy);
    }

    public void Remove(TradingStrategy strategy)
    {
        _dbSet.Remove(strategy);
    }

    public void RemoveRange(IEnumerable<TradingStrategy> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public async Task<bool> ExistsAsync(StrategyId id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TradingStrategy, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }
}
