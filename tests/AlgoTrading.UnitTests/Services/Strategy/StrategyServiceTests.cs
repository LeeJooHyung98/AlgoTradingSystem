using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Enums;
using AlgoTrading.Core.Interfaces.Repositories;
using AlgoTrading.Core.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AlgoTrading.UnitTests.Services.Strategy;

/// <summary>
/// Unit tests for StrategyService
/// Tests all CRUD operations and business logic
/// </summary>
public class StrategyServiceTests
{
    private readonly IStrategyRepository _strategyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StrategyService> _logger;
    private readonly StrategyService _sut; // System Under Test

    public StrategyServiceTests()
    {
        _strategyRepository = Substitute.For<IStrategyRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<StrategyService>>();

        _sut = new StrategyService(
            _logger,
            _strategyRepository,
            _unitOfWork);
    }

    #region CreateStrategyAsync Tests

    [Fact]
    public async Task CreateStrategyAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var name = "Test Strategy";
        var description = "Test Description";
        var type = Core.Enums.StrategyType.Momentum;
        var createdBy = "TestUser";

        _strategyRepository.GetByNameAsync(name, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(null));

        // Act
        var result = await _sut.CreateStrategyAsync(name, description, type, createdBy);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.Status.Should().Be(Core.Entities.Strategy.StrategyStatus.Draft);

        await _strategyRepository.Received(1).AddAsync(Arg.Any<TradingStrategy>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateStrategyAsync_WithDuplicateName_ShouldReturnError()
    {
        // Arrange
        var name = "Existing Strategy";
        var existingStrategy = TradingStrategy.Create(name, "Description", Core.Entities.Strategy.StrategyType.Momentum, "User");

        _strategyRepository.GetByNameAsync(name, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(existingStrategy));

        // Act
        var result = await _sut.CreateStrategyAsync(name, "New Desc", Core.Enums.StrategyType.Breakout, "NewUser");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.Conflict);
        result.FirstError.Code.Should().Be("Strategy.DuplicateName");

        await _strategyRepository.DidNotReceive().AddAsync(Arg.Any<TradingStrategy>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateStrategyAsync_WithInvalidName_ShouldReturnError(string? invalidName)
    {
        // Act
        var result = await _sut.CreateStrategyAsync(invalidName!, "Description", Core.Enums.StrategyType.Momentum, "User");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.Validation);
    }

    #endregion

    #region ActivateStrategyAsync Tests

    [Fact]
    public async Task ActivateStrategyAsync_WithValidStrategy_ShouldReturnSuccess()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var rule = StrategyRule.Create("Rule1", "Test Rule", Core.Entities.Strategy.RuleType.EntryLong, "condition", 1);
        strategy.AddRule(rule);

        var strategyId = strategy.Id.Value;

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.ActivateStrategyAsync(strategyId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(Core.Entities.Strategy.StrategyStatus.Active);
        result.Value.LastActivatedAt.Should().NotBeNull();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateStrategyAsync_WithoutRules_ShouldReturnError()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        // No rules added
        var strategyId = strategy.Id.Value;

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.ActivateStrategyAsync(strategyId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Strategy.ActivationFailed");
    }

    [Fact]
    public async Task ActivateStrategyAsync_WithNonExistentStrategy_ShouldReturnError()
    {
        // Arrange
        var strategyId = Guid.NewGuid();

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(null));

        // Act
        var result = await _sut.ActivateStrategyAsync(strategyId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }

    #endregion

    #region DeactivateStrategyAsync Tests

    [Fact]
    public async Task DeactivateStrategyAsync_WithActiveStrategy_ShouldReturnSuccess()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var rule = StrategyRule.Create("Rule1", "Test Rule", Core.Entities.Strategy.RuleType.EntryLong, "condition", 1);
        strategy.AddRule(rule);
        strategy.Activate(); // Make it active

        var strategyId = strategy.Id.Value;

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.DeactivateStrategyAsync(strategyId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Status.Should().Be(Core.Entities.Strategy.StrategyStatus.Inactive);
        result.Value.LastDeactivatedAt.Should().NotBeNull();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region AddRuleAsync Tests

    [Fact]
    public async Task AddRuleAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var strategyId = strategy.Id.Value;

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.AddRuleAsync(
            strategyId,
            "Test Rule",
            "Rule Description",
            Core.Enums.RuleType.Entry,
            "condition",
            1);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Rules.Should().HaveCount(1);
        result.Value.Rules.First().Name.Should().Be("Test Rule");

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddRuleAsync_ToActiveStrategy_ShouldReturnError()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var rule = StrategyRule.Create("Rule1", "Test Rule", Core.Entities.Strategy.RuleType.EntryLong, "condition", 1);
        strategy.AddRule(rule);
        strategy.Activate(); // Make it active

        var strategyId = strategy.Id.Value;

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.AddRuleAsync(
            strategyId,
            "New Rule",
            "Description",
            Core.Enums.RuleType.Entry,
            "condition",
            2);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Strategy.AddRuleFailed");
    }

    #endregion

    #region RemoveRuleAsync Tests

    [Fact]
    public async Task RemoveRuleAsync_WithExistingRule_ShouldReturnSuccess()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var rule = StrategyRule.Create("Rule1", "Test Rule", Core.Entities.Strategy.RuleType.EntryLong, "condition", 1);
        strategy.AddRule(rule);

        var strategyId = strategy.Id.Value;
        var ruleId = rule.Id.Value;

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.RemoveRuleAsync(strategyId, ruleId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Rules.Should().BeEmpty();

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region SetRiskParametersAsync Tests

    [Fact]
    public async Task SetRiskParametersAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var strategyId = strategy.Id.Value;

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.SetRiskParametersAsync(strategyId, 15m, 3m, 10m);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.MaxPositionSizePercent.Should().Be(15m);
        result.Value.StopLossPercent.Should().Be(3m);
        result.Value.TakeProfitPercent.Should().Be(10m);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-1, 5, 10)]  // Invalid max position size
    [InlineData(101, 5, 10)] // Invalid max position size
    [InlineData(10, -1, 10)] // Invalid stop loss
    [InlineData(10, 101, 10)] // Invalid stop loss
    [InlineData(10, 5, -1)]  // Invalid take profit
    [InlineData(10, 5, 1001)] // Invalid take profit
    public async Task SetRiskParametersAsync_WithInvalidParameters_ShouldReturnError(
        decimal maxPositionSize, decimal stopLoss, decimal takeProfit)
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var strategyId = strategy.Id.Value;

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.SetRiskParametersAsync(strategyId, maxPositionSize, stopLoss, takeProfit);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Strategy.SetRiskParametersFailed");
    }

    #endregion

    #region CloneStrategyAsync Tests

    [Fact]
    public async Task CloneStrategyAsync_WithExistingStrategy_ShouldReturnSuccess()
    {
        // Arrange
        var originalStrategy = TradingStrategy.Create("Original", "Original Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var rule = StrategyRule.Create("Rule1", "Test Rule", Core.Entities.Strategy.RuleType.EntryLong, "condition", 1);
        originalStrategy.AddRule(rule);
        originalStrategy.SetRiskParameters(15m, 3m, 10m);

        var originalId = originalStrategy.Id.Value;
        var newName = "Cloned Strategy";

        _strategyRepository.GetWithRulesAsync(originalId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(originalStrategy));

        _strategyRepository.GetByNameAsync(newName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(null));

        // Act
        var result = await _sut.CloneStrategyAsync(originalId, newName, "CloneUser");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be(newName);
        result.Value.Rules.Should().HaveCount(1);
        result.Value.MaxPositionSizePercent.Should().Be(15m);
        result.Value.StopLossPercent.Should().Be(3m);
        result.Value.TakeProfitPercent.Should().Be(10m);

        await _strategyRepository.Received(1).AddAsync(Arg.Any<TradingStrategy>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region ValidateStrategyAsync Tests

    [Fact]
    public async Task ValidateStrategyAsync_WithValidStrategy_ShouldReturnSuccess()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var entryRule = StrategyRule.Create("Entry", "Entry Rule", Core.Entities.Strategy.RuleType.EntryLong, "condition", 1);
        var exitRule = StrategyRule.Create("Exit", "Exit Rule", Core.Entities.Strategy.RuleType.ExitLong, "condition", 2);
        strategy.AddRule(entryRule);
        strategy.AddRule(exitRule);

        var strategyId = strategy.Id.Value;

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.ValidateStrategyAsync(strategyId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateStrategyAsync_WithoutEntryRule_ShouldReturnError()
    {
        // Arrange
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");
        var exitRule = StrategyRule.Create("Exit", "Exit Rule", Core.Entities.Strategy.RuleType.ExitLong, "condition", 1);
        strategy.AddRule(exitRule); // Only exit rule, no entry rule

        var strategyId = strategy.Id.Value;

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        // Act
        var result = await _sut.ValidateStrategyAsync(strategyId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Strategy.ValidationFailed");
        result.FirstError.Description.Should().Contain("entry rule");
    }

    #endregion

    // Note: UpdatePerformanceAsync tests removed as this method doesn't exist in StrategyService
    // Strategy statistics are updated via UpdateStrategyStatisticsAsync which is tested in integration tests
}
