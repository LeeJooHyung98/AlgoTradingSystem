using AlgoTrading.Core.Interfaces.Repositories;

namespace AlgoTrading.Infrastructure.Persistence;

/// <summary>
/// description(설명) : Temporary Unit of Work implementation (임시 Unit of Work 구현)
/// Details(상세설명) : Provides basic UnitOfWork functionality by wrapping DbContext
///                    without requiring all repositories to be implemented (모든 리포지토리 구현 없이 DbContext를 래핑하여 기본 UnitOfWork 기능 제공)
/// Applied technology patterns(적용기술패턴) : Unit of Work Pattern, Adapter Pattern
/// </summary>
/// <remarks>
/// This is a temporary implementation. Once all repositories are implemented,
/// this should be replaced with a full IUnitOfWork implementation that includes repository properties.
/// </remarks>
public class UnitOfWork : IUnitOfWork
{
    private readonly AlgoTradingDbContext _context;
    private readonly IOrderRepository _orderRepository;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly ITradeRepository _tradeRepository;

    public UnitOfWork(
        AlgoTradingDbContext context,
        IOrderRepository orderRepository,
        IStrategyRepository strategyRepository,
        IPositionRepository positionRepository,
        ITradeRepository tradeRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _strategyRepository = strategyRepository ?? throw new ArgumentNullException(nameof(strategyRepository));
        _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
        _tradeRepository = tradeRepository ?? throw new ArgumentNullException(nameof(tradeRepository));
    }

    public IOrderRepository Orders => _orderRepository;

    public IStrategyRepository Strategies => _strategyRepository;

    public IPortfolioRepository Portfolios => throw new NotImplementedException("PortfolioRepository not yet implemented");

    public IAccountRepository Accounts => throw new NotImplementedException("AccountRepository not yet implemented");

    public IPositionRepository Positions => _positionRepository;

    public ITradeRepository Trades => _tradeRepository;

    public IRiskProfileRepository RiskProfiles => throw new NotImplementedException("RiskProfileRepository not yet implemented");

    public IRiskMonitorRepository RiskMonitors => throw new NotImplementedException("RiskMonitorRepository not yet implemented");

    public IStockRepository Stocks => throw new NotImplementedException("StockRepository not yet implemented");

    public IPriceDataRepository PriceData => throw new NotImplementedException("PriceDataRepository not yet implemented");

    public IBacktestRepository Backtests => throw new NotImplementedException("BacktestRepository not yet implemented");

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _context.BeginTransactionAsync(cancellationToken);
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _context.CommitTransactionAsync(cancellationToken);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _context.RollbackTransactionAsync(cancellationToken);
    }

    public void Dispose()
    {
        // DbContext is managed by DI container, don't dispose here
    }
}
