using AlgoTrading.Application.Behaviors;
using AlgoTrading.Application.Services.Trading;
using AlgoTrading.Application.Services.Strategy;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AlgoTrading.Application;

/// <summary>
/// description(설명) : Application Layer Dependency Injection (애플리케이션 계층 의존성 주입)
/// Details(상세설명) : Extension methods to register Application layer services and dependencies (애플리케이션 계층 서비스 및 의존성을 등록하는 확장 메서드)
/// Applied technology patterns(적용기술패턴) : Dependency Injection Pattern, Extension Methods Pattern (의존성 주입 패턴, 확장 메서드 패턴)
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add Application layer services to DI container (DI 컨테이너에 애플리케이션 계층 서비스 추가)
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR (CQRS Command/Query handlers)
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            // Add MediatR pipeline behaviors (order matters!)
            // 1. Logging - Log all requests
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));

            // 2. Validation - Validate before execution
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));

            // 3. Performance - Measure execution time
            config.AddOpenBehavior(typeof(PerformanceBehavior<,>));

            // 4. Transaction - Wrap in database transaction
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        // Add FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Add Mapster for object mapping
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(assembly);
        services.AddSingleton(mappingConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        // Add Application Services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPositionManagementService, PositionManagementService>();

        // Strategy Services
        services.AddScoped<IStrategyService, StrategyService>();
        services.AddScoped<IStrategyOptimizationService, StrategyOptimizationService>();
        services.AddScoped<IBacktestService, BacktestService>();
        services.AddScoped<IPortfolioBacktestService, PortfolioBacktestService>();
        services.AddScoped<ISignalGenerationService, SignalGenerationService>();
        services.AddScoped<ITechnicalIndicatorService, TechnicalIndicatorService>();

        return services;
    }
}
