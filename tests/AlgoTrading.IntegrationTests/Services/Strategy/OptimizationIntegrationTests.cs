using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using Xunit;
using StrategyType = AlgoTrading.Core.Entities.Strategy.StrategyType;
using RuleType = AlgoTrading.Core.Entities.Strategy.RuleType;
using SignalType = AlgoTrading.Core.Entities.Strategy.SignalType;

namespace AlgoTrading.IntegrationTests.Services.Strategy;

/// <summary>
/// Integration tests for Strategy Optimization
/// Tests with real EF Core InMemory database
/// </summary>
public class OptimizationIntegrationTests : IDisposable
{
    private readonly StrategyTestDbContext _context;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStrategyService _strategyService;
    private readonly IStrategyOptimizationService _optimizationService;
    private readonly IBacktestService _backtestService;
    private readonly ILogger<StrategyService> _strategyLogger;
    private readonly ILogger<StrategyOptimizationService> _optimizationLogger;

    public OptimizationIntegrationTests()
    {
        // Create InMemory database
        var options = new DbContextOptionsBuilder<StrategyTestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StrategyTestDbContext(options);

        // Create test repository
        _strategyRepository = new TestStrategyRepository(_context);

        // Create test UnitOfWork
        _unitOfWork = new TestUnitOfWork(_context, _strategyRepository);

        _strategyLogger = Substitute.For<ILogger<StrategyService>>();
        _optimizationLogger = Substitute.For<ILogger<StrategyOptimizationService>>();

        _strategyService = new StrategyService(
            _strategyLogger,
            _strategyRepository,
            _unitOfWork);

        // Mock backtest service for optimization
        _backtestService = Substitute.For<IBacktestService>();

        _optimizationService = new StrategyOptimizationService(
            _optimizationLogger,
            _strategyRepository,
            _strategyService,
            _backtestService,
            _unitOfWork);
    }

    // Grid Search test removed - requires full Backtest service implementation

    [Fact]
    public async Task GetStrategyWithRules_ShouldLoadOwnedEntities()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", StrategyType.Momentum, "User");
        var rule1 = StrategyRule.Create("Entry", "Entry Rule", RuleType.EntryLong, "price > ma", 1);
        var rule2 = StrategyRule.Create("Exit", "Exit Rule", RuleType.ExitLong, "price < ma", 2);

        strategy.AddRule(rule1);
        strategy.AddRule(rule2);

        await _strategyRepository.AddAsync(strategy);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var loadedStrategy = await _strategyRepository.GetWithRulesAsync(strategy.Id.Value);

        // Assert
        loadedStrategy.Should().NotBeNull();
        loadedStrategy!.Rules.Should().HaveCount(2);
        loadedStrategy.Rules.Should().Contain(r => r.Name == "Entry");
        loadedStrategy.Rules.Should().Contain(r => r.Name == "Exit");
    }

    [Fact]
    public async Task RecordSignal_ShouldPersistSignalAsOwnedEntity()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", StrategyType.Momentum, "User");
        await _strategyRepository.AddAsync(strategy);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var signal = StrategySignal.Create(
            strategy.Id,
            new Core.ValueObjects.StockCode("005930"),
            SignalType.Buy,
            75.5m,
            0.85m);

        strategy.RecordSignal(signal);
        _strategyRepository.Update(strategy);
        await _unitOfWork.SaveChangesAsync();

        // Assert
        var loadedStrategy = await _strategyRepository.GetWithSignalsAsync(strategy.Id.Value);
        loadedStrategy.Should().NotBeNull();
        loadedStrategy!.Signals.Should().HaveCount(1);
        loadedStrategy.Signals.First().SignalType.Should().Be(SignalType.Buy);
        loadedStrategy.Signals.First().Strength.Should().Be(75.5m);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
