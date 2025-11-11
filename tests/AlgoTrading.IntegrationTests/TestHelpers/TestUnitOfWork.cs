using AlgoTrading.Core.Interfaces.Repositories;

namespace AlgoTrading.IntegrationTests.TestHelpers;

/// <summary>
/// Test-specific implementation of IUnitOfWork for Integration Tests
/// Uses StrategyTestDbContext instead of full AlgoTradingDbContext
/// </summary>
public class TestUnitOfWork : IUnitOfWork
{
    private readonly StrategyTestDbContext _context;

    public IOrderRepository Orders => throw new NotImplementedException("Orders not needed for Strategy tests");
    public IStrategyRepository Strategies { get; }
    public IPortfolioRepository Portfolios => throw new NotImplementedException("Portfolios not needed for Strategy tests");
    public IAccountRepository Accounts => throw new NotImplementedException("Accounts not needed for Strategy tests");
    public IPositionRepository Positions => throw new NotImplementedException("Positions not needed for Strategy tests");
    public ITradeRepository Trades => throw new NotImplementedException("Trades not needed for Strategy tests");
    public IRiskProfileRepository RiskProfiles => throw new NotImplementedException("RiskProfiles not needed for Strategy tests");
    public IRiskMonitorRepository RiskMonitors => throw new NotImplementedException("RiskMonitors not needed for Strategy tests");
    public IStockRepository Stocks => throw new NotImplementedException("Stocks not needed for Strategy tests");
    public IPriceDataRepository PriceData => throw new NotImplementedException("PriceData not needed for Strategy tests");
    public IBacktestRepository Backtests => throw new NotImplementedException("Backtests not needed for Strategy tests");

    public TestUnitOfWork(StrategyTestDbContext context, IStrategyRepository strategyRepository)
    {
        _context = context;
        Strategies = strategyRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // InMemory database doesn't support transactions
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        // InMemory database doesn't support transactions
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        // InMemory database doesn't support transactions
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
