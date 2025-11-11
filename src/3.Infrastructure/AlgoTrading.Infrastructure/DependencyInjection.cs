using AlgoTrading.Core.Interfaces;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Infrastructure.Persistence;
using AlgoTrading.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoTrading.Infrastructure;

/// <summary>
/// description(설명) : Infrastructure layer dependency injection configuration
/// Details(상세설명) : Extension methods for registering infrastructure services
/// Applied technology patterns(적용기술패턴) : Dependency Injection Pattern, Extension Methods Pattern
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// description(설명) : Add Infrastructure layer services to DI container
    /// Details(상세설명) : Registers DbContext, Repositories, External Services, Caching, etc.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add Database Context
        services.AddDatabase(configuration);

        // Add Repositories
        services.AddRepositories();

        // Add Market Data Providers
        services.AddMarketDataProviders();

        // TODO: Add External Services (Kiwoom, eBest, etc.)
        // services.AddExternalServices(configuration);

        // TODO: Add Caching (Redis, MemoryCache)
        // services.AddCaching(configuration);

        // TODO: Add Message Bus (RabbitMQ)
        // services.AddMessageBus(configuration);

        // TODO: Add Background Services
        // services.AddBackgroundServices();

        return services;
    }

    /// <summary>
    /// description(설명) : Add Database Context with PostgreSQL + TimescaleDB
    /// Details(상세설명) : Configures EF Core with Npgsql provider and connection pooling
    /// </summary>
    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AlgoTradingDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Enable TimescaleDB support
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);

                // Connection resilience
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);

                // Command timeout
                npgsqlOptions.CommandTimeout(30);

                // Migration assembly
                npgsqlOptions.MigrationsAssembly(typeof(AlgoTradingDbContext).Assembly.FullName);

                // Migration history table schema
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
            });

            // Configure EF Core behavior
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            // Enable lazy loading proxies if needed
            // options.UseLazyLoadingProxies();

            #if DEBUG
            // Enable sensitive data logging in development
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            #endif
        });

        // Health checks for database
        // TODO: Add AspNetCore.HealthChecks.Npgsql NuGet package to enable this
        // services.AddHealthChecks()
        //     .AddNpgSql(
        //         connectionString,
        //         name: "postgresql",
        //         tags: new[] { "db", "postgresql", "timescaledb" });

        return services;
    }

    /// <summary>
    /// description(설명) : Add Repository implementations
    /// Details(상세설명) : Registers all repository interfaces with their implementations
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Trading Context Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<ITradeRepository, TradeRepository>();

        // Strategy Context Repositories
        services.AddScoped<IStrategyRepository, StrategyRepository>();

        // User and Account repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Monitoring repositories (TimescaleDB)
        services.AddScoped<ITradingMetricRepository, TradingMetricRepository>();

        // Audit repositories (TimescaleDB)
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Market Data repositories
        services.AddScoped<IPriceDataRepository, PriceDataRepository>();

        // Unit of Work - Temporary implementation until all repositories are ready
        services.AddScoped<Core.Interfaces.Repositories.IUnitOfWork, UnitOfWork>();

        // TODO: Add more repositories as needed when their implementations are ready
        // services.AddScoped<IAccountRepository, AccountRepository>();
        // services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        // services.AddScoped<IRiskProfileRepository, RiskProfileRepository>();
        // services.AddScoped<IRiskMonitorRepository, RiskMonitorRepository>();
        // services.AddScoped<IStockRepository, StockRepository>();
        // services.AddScoped<IBacktestRepository, BacktestRepository>();

        return services;
    }

    /// <summary>
    /// description(설명) : Add External Services (Broker APIs)
    /// Details(상세설명) : Registers Kiwoom, eBest adapters and market data collectors
    /// </summary>
    private static IServiceCollection AddExternalServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Register external service adapters
        // services.AddSingleton<IKiwoomBrokerAdapter, KiwoomBrokerAdapter>();
        // services.AddSingleton<IEBestBrokerAdapter, EBestBrokerAdapter>();
        // services.AddHttpClient for REST APIs

        return services;
    }

    /// <summary>
    /// description(설명) : Add Caching services (Redis + MemoryCache)
    /// Details(상세설명) : Configures L1 (MemoryCache) and L2 (Redis) caching layers
    /// </summary>
    private static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Add caching
        // L1 Cache - MemoryCache
        // services.AddMemoryCache();

        // L2 Cache - Redis
        // var redisConnection = configuration.GetConnectionString("Redis");
        // services.AddStackExchangeRedisCache(options =>
        // {
        //     options.Configuration = redisConnection;
        //     options.InstanceName = "AlgoTrading:";
        // });

        // services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    /// <summary>
    /// description(설명) : Add Message Bus (RabbitMQ with MassTransit)
    /// Details(상세설명) : Configures RabbitMQ for event-driven architecture
    /// </summary>
    private static IServiceCollection AddMessageBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Add MassTransit + RabbitMQ
        // services.AddMassTransit(x =>
        // {
        //     x.UsingRabbitMq((context, cfg) =>
        //     {
        //         cfg.Host(configuration["RabbitMQ:HostName"], h =>
        //         {
        //             h.Username(configuration["RabbitMQ:UserName"]);
        //             h.Password(configuration["RabbitMQ:Password"]);
        //         });
        //     });
        // });

        return services;
    }

    /// <summary>
    /// description(설명) : Add Background Services
    /// Details(상세설명) : Registers hosted services for market data collection, strategy execution, etc.
    /// </summary>
    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        // TODO: Add background services
        // services.AddHostedService<MarketDataCollectorService>();
        // services.AddHostedService<StrategyExecutorService>();
        // services.AddHostedService<RiskMonitoringService>();

        return services;
    }

    /// <summary>
    /// description(설명) : Add Market Data Providers
    /// Details(상세설명) : Registers market data provider implementations for accessing price data
    /// </summary>
    private static IServiceCollection AddMarketDataProviders(this IServiceCollection services)
    {
        // Database Market Data Provider (default) - For backtesting
        services.AddScoped<Application.Services.MarketData.IMarketDataProvider,
            Services.MarketData.DatabaseMarketDataProvider>();

        // Kiwoom Real-time Market Data Provider - For live trading
        // Uncomment below to use Kiwoom OpenAPI for real-time data
        // services.AddScoped<Services.MarketData.KiwoomMarketDataProvider>();

        // TODO: Add eBest market data provider when ready
        // services.AddScoped<IEBestMarketDataProvider, EBestMarketDataProvider>();

        return services;
    }
}
