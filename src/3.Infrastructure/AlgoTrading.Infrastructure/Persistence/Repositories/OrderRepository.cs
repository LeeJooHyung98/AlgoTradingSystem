using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OrderStatus = AlgoTrading.Core.Enums.OrderStatus;

namespace AlgoTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// description(설명) : Order repository implementation (주문 리포지토리 구현)
/// Details(상세설명) : Implementation of IOrderRepository with EF Core (EF Core를 사용한 주문 리포지토리 구현)
/// Applied technology patterns(적용기술패턴) : Repository Pattern, CQRS Pattern, Specification Pattern (리포지토리 패턴, CQRS 패턴, 명세 패턴)
/// </summary>
public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(AlgoTradingDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetByAccountNumberAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.AccountNumber == accountNumber)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStockCodeAsync(
        string stockCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.StockCode.Value == stockCode)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        // Convert Enums.OrderStatus to Trading.OrderStatus by casting the int value
        var tradingStatus = (AlgoTrading.Core.Entities.Trading.OrderStatus)(int)status;

        return await _dbSet
            .Where(o => o.Status == tradingStatus)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByStrategyIdAsync(
        Guid strategyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.StrategyId == strategyId)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.SubmittedAt >= startDate && o.SubmittedAt <= endDate)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        var pendingStatus = AlgoTrading.Core.Entities.Trading.OrderStatus.PendingSubmit;
        var partiallyFilledStatus = AlgoTrading.Core.Entities.Trading.OrderStatus.PartiallyFilled;

        return await _dbSet
            .Where(o => o.Status == pendingStatus || o.Status == partiallyFilledStatus)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetTodaysOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await _dbSet
            .Where(o => o.SubmittedAt >= today && o.SubmittedAt < tomorrow)
            .OrderByDescending(o => o.SubmittedAt)
            .ToListAsync(cancellationToken);
    }
}
