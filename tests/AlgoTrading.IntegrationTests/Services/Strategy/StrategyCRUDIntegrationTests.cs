using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.IntegrationTests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FluentAssertions;
using Xunit;

namespace AlgoTrading.IntegrationTests.Services.Strategy;

/// <summary>
/// Integration tests for Strategy CRUD operations
/// Tests with real EF Core InMemory database
/// </summary>
public class StrategyCRUDIntegrationTests : IDisposable
{
    private readonly StrategyTestDbContext _context;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStrategyService _strategyService;
    private readonly ILogger<StrategyService> _logger;

    public StrategyCRUDIntegrationTests()
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

        _logger = Substitute.For<ILogger<StrategyService>>();

        _strategyService = new StrategyService(
            _logger,
            _strategyRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task CreateStrategy_WithValidData_ShouldPersistToDatabase()
    {
        // Arrange
        var name = "Test Strategy";
        var description = "Test Description";
        var type = Core.Enums.StrategyType.Momentum;
        var createdBy = "TestUser";

        // Act
        var result = await _strategyService.CreateStrategyAsync(name, description, type, createdBy);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.Status.Should().Be(StrategyStatus.Draft);

        // Verify persistence
        var savedStrategy = await _context.TradingStrategies
            .FirstOrDefaultAsync(s => s.Name == name);

        savedStrategy.Should().NotBeNull();
        savedStrategy!.Description.Should().Be(description);
    }

    [Fact]
    public async Task GetStrategyById_WithExistingStrategy_ShouldReturnStrategy()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", StrategyType.Momentum, "User");
        await _strategyRepository.AddAsync(strategy);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _strategyService.GetStrategyByIdAsync(strategy.Id.Value);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(strategy.Id);
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task DeleteStrategy_WithExistingStrategy_ShouldRemoveFromDatabase()
    {
        // Arrange
        var strategy = TradingStrategy.Create("ToDelete", "Desc", StrategyType.Momentum, "User");
        await _strategyRepository.AddAsync(strategy);
        await _unitOfWork.SaveChangesAsync();

        var strategyId = strategy.Id.Value;

        // Act
        var result = await _strategyService.DeleteStrategyAsync(strategyId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();

        // Verify deletion
        var deletedStrategy = await _context.TradingStrategies
            .FirstOrDefaultAsync(s => s.Id.Value == strategyId);

        deletedStrategy.Should().BeNull();
    }

    [Fact]
    public async Task ActivateStrategy_WithRules_ShouldChangeStatus()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", StrategyType.Momentum, "User");
        var rule = StrategyRule.Create("Entry", "Entry Rule", RuleType.EntryLong, "price > ma", 1);
        strategy.AddRule(rule);

        await _strategyRepository.AddAsync(strategy);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _strategyService.ActivateStrategyAsync(strategy.Id.Value);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(StrategyStatus.Active);
        result.Value.LastActivatedAt.Should().NotBeNull();

        // Verify persistence
        var activatedStrategy = await _context.TradingStrategies
            .FirstOrDefaultAsync(s => s.Id == strategy.Id);

        activatedStrategy.Should().NotBeNull();
        activatedStrategy!.Status.Should().Be(StrategyStatus.Active);
    }

    [Fact]
    public async Task GetAllStrategies_ShouldReturnAllStrategies()
    {
        // Arrange
        var strategy1 = TradingStrategy.Create("Strategy1", "Desc1", StrategyType.Momentum, "User");
        var strategy2 = TradingStrategy.Create("Strategy2", "Desc2", StrategyType.MeanReversion, "User");

        await _strategyRepository.AddAsync(strategy1);
        await _strategyRepository.AddAsync(strategy2);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _strategyService.GetAllStrategiesAsync();

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(s => s.Name == "Strategy1");
        result.Value.Should().Contain(s => s.Name == "Strategy2");
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
