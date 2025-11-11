using AlgoTrading.Core.Entities.Audit;
using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.Entities.Monitoring;
using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.Entities.Users;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AlgoTrading.Infrastructure.Persistence;

/// <summary>
/// description(설명) : AlgoTrading DbContext for PostgreSQL + TimescaleDB
/// Details(상세설명) : Main database context with multi-schema support and TimescaleDB integration
/// Applied technology patterns(적용기술패턴) : DbContext Pattern, Repository Pattern, Unit of Work Pattern
/// </summary>
public class AlgoTradingDbContext : DbContext
{
    private IDbContextTransaction? _currentTransaction;

    public AlgoTradingDbContext(DbContextOptions<AlgoTradingDbContext> options)
        : base(options)
    {
    }

    #region DbSets - Users Context

    /// <summary>
    /// 사용자 (public.users)
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    #endregion

    #region DbSets - Account Context

    /// <summary>
    /// 계좌 잔액 이력 (account.account_balance_history) - TimescaleDB Hypertable
    /// </summary>
    public DbSet<AccountBalanceHistory> AccountBalanceHistories { get; set; } = null!;

    /// <summary>
    /// 성과 기록 (account.performance_records) - TimescaleDB Hypertable
    /// </summary>
    public DbSet<PerformanceRecord> PerformanceRecords { get; set; } = null!;

    /// <summary>
    /// 계좌 제약 정보 (account.account_restrictions)
    /// </summary>
    public DbSet<AccountRestriction> AccountRestrictions { get; set; } = null!;

    /// <summary>
    /// 출금 요청 (account.account_withdrawals)
    /// </summary>
    public DbSet<AccountWithdrawal> AccountWithdrawals { get; set; } = null!;

    #endregion

    #region DbSets - Market Data Context

    /// <summary>
    /// 종목 정보 (market.stock_info)
    /// </summary>
    public DbSet<StockInfo> StockInfos { get; set; } = null!;

    /// <summary>
    /// 시장 데이터 (market.market_data) - TimescaleDB Hypertable
    /// </summary>
    public DbSet<MarketData> MarketData { get; set; } = null!;

    /// <summary>
    /// 호가 데이터 (market.order_book)
    /// </summary>
    public DbSet<OrderBook> OrderBooks { get; set; } = null!;

    /// <summary>
    /// 틱 데이터 (market.tick_data) - TimescaleDB Hypertable
    /// </summary>
    public DbSet<TickData> TickData { get; set; } = null!;

    #endregion

    #region DbSets - Monitoring Context

    /// <summary>
    /// 시스템 메트릭 (monitoring.system_metrics) - TimescaleDB Hypertable
    /// </summary>
    public DbSet<SystemMetric> SystemMetrics { get; set; } = null!;

    /// <summary>
    /// 거래 메트릭 (monitoring.trading_metrics) - TimescaleDB Hypertable
    /// </summary>
    public DbSet<TradingMetric> TradingMetrics { get; set; } = null!;

    /// <summary>
    /// 알림 (monitoring.alerts)
    /// </summary>
    public DbSet<Alert> Alerts { get; set; } = null!;

    #endregion

    #region DbSets - Audit Context

    /// <summary>
    /// 감사 로그 (audit.audit_logs) - TimescaleDB Hypertable
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    #endregion

    #region DbSets - Trading Context

    /// <summary>
    /// 주문 (trading.orders)
    /// </summary>
    public DbSet<Core.Entities.Trading.Order> Orders { get; set; } = null!;

    /// <summary>
    /// 포지션 (trading.positions)
    /// </summary>
    public DbSet<Core.Entities.Trading.Position> Positions { get; set; } = null!;

    /// <summary>
    /// 거래 (trading.trades)
    /// </summary>
    public DbSet<Core.Entities.Trading.Trade> Trades { get; set; } = null!;

    /// <summary>
    /// 체결 (trading.executions)
    /// </summary>
    public DbSet<Core.Entities.Trading.Execution> Executions { get; set; } = null!;

    #endregion

    #region DbSets - Strategy Context

    /// <summary>
    /// 트레이딩 전략 (strategy.trading_strategies)
    /// </summary>
    public DbSet<Core.Entities.Strategy.TradingStrategy> TradingStrategies { get; set; } = null!;

    #endregion

    #region DbSets - TODO: Add remaining entities

    // TODO: Add remaining Strategy Context entities
    // - StrategyParameter (strategy.strategy_parameters)
    // - SignalHistory (strategy.signal_history)

    // TODO: Add Risk Context entities
    // - RiskLimit (risk.risk_limits)
    // - RiskEvent (risk.risk_events)
    // - DailyRiskMetric (risk.daily_risk_metrics)

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        ApplyConfigurations(modelBuilder);

        // Apply global query filters if needed
        // ApplyGlobalFilters(modelBuilder);
    }

    /// <summary>
    /// description(설명) : Apply all entity type configurations
    /// Details(상세설명) : Registers all IEntityTypeConfiguration implementations
    /// </summary>
    private void ApplyConfigurations(ModelBuilder modelBuilder)
    {
        // Users Context
        modelBuilder.ApplyConfiguration(new UserConfiguration());

        // Account Context
        modelBuilder.ApplyConfiguration(new AccountBalanceHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new PerformanceRecordConfiguration());
        modelBuilder.ApplyConfiguration(new AccountRestrictionConfiguration());
        modelBuilder.ApplyConfiguration(new AccountWithdrawalConfiguration());

        // Market Data Context
        modelBuilder.ApplyConfiguration(new StockInfoConfiguration());
        modelBuilder.ApplyConfiguration(new MarketDataConfiguration());
        modelBuilder.ApplyConfiguration(new OrderBookConfiguration());
        modelBuilder.ApplyConfiguration(new TickDataConfiguration());

        // Monitoring Context
        modelBuilder.ApplyConfiguration(new SystemMetricConfiguration());
        modelBuilder.ApplyConfiguration(new TradingMetricConfiguration());
        modelBuilder.ApplyConfiguration(new AlertConfiguration());

        // Audit Context
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        // Trading Context
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new PositionConfiguration());
        modelBuilder.ApplyConfiguration(new TradeConfiguration());
        modelBuilder.ApplyConfiguration(new ExecutionConfiguration());

        // Strategy Context
        modelBuilder.ApplyConfiguration(new TradingStrategyConfiguration());

        // TODO: Apply remaining configurations when created
        // etc...
    }

    /// <summary>
    /// description(설명) : Apply global query filters
    /// Details(상세설명) : Soft delete, multi-tenancy, etc.
    /// </summary>
    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        // Example: Soft delete filter
        // modelBuilder.Entity<Order>().HasQueryFilter(o => o.DeletedAt == null);

        // Example: Audit timestamp automatic update will be handled in SaveChangesAsync
    }

    /// <summary>
    /// description(설명) : Override SaveChangesAsync for audit trail
    /// Details(상세설명) : Automatically updates CreatedAt, UpdatedAt timestamps
    /// Applied technology patterns(적용기술패턴) : Audit Trail Pattern
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps for entities with tracking properties
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Handle CreatedAt timestamp
            if (entry.State == EntityState.Added)
            {
                var createdAtProperty = entry.Properties
                    .FirstOrDefault(p => p.Metadata.Name == "CreatedAt");

                if (createdAtProperty != null && createdAtProperty.CurrentValue == null)
                {
                    createdAtProperty.CurrentValue = DateTime.UtcNow;
                }
            }

            // Handle UpdatedAt timestamp
            if (entry.State == EntityState.Modified)
            {
                var updatedAtProperty = entry.Properties
                    .FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");

                if (updatedAtProperty != null)
                {
                    updatedAtProperty.CurrentValue = DateTime.UtcNow;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// description(설명) : Configure database provider options
    /// Details(상세설명) : PostgreSQL + TimescaleDB specific configurations
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable sensitive data logging in development only
        #if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
        #endif
    }

    #region Transaction Management

    /// <summary>
    /// Begin database transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commit transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            _currentTransaction?.Commit();
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Rollback transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }

        await Task.CompletedTask;
    }

    #endregion
}
