using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Enums;
using AlgoTrading.Core.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AlgoTrading.UnitTests.Services.Strategy;

/// <summary>
/// Unit tests for StrategyOptimizationService
/// Tests optimization algorithms and parameter search
/// </summary>
public class StrategyOptimizationServiceTests
{
    private readonly ILogger<StrategyOptimizationService> _logger;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IStrategyService _strategyService;
    private readonly IBacktestService _backtestService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly StrategyOptimizationService _sut;

    public StrategyOptimizationServiceTests()
    {
        _logger = Substitute.For<ILogger<StrategyOptimizationService>>();
        _strategyRepository = Substitute.For<IStrategyRepository>();
        _strategyService = Substitute.For<IStrategyService>();
        _backtestService = Substitute.For<IBacktestService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _sut = new StrategyOptimizationService(
            _logger,
            _strategyRepository,
            _strategyService,
            _backtestService,
            _unitOfWork);
    }

    #region GridSearchAsync Tests

    [Fact]
    public async Task GridSearchAsync_WithValidParameters_ShouldReturnOptimizationResult()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        var parameterRanges = new Dictionary<string, ParameterRange>
        {
            { "Period", new ParameterRange(5, 15, 5, ParameterType.Integer) } // 5, 10, 15
        };

        var backtestResult = new BacktestResult
        {
            BacktestId = Guid.NewGuid(),
            StrategyId = strategyId,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            InitialCapital = 10000000,
            FinalCapital = 11000000,
            Metrics = new PerformanceMetrics
            {
                TotalReturn = 10m,
                SharpeRatio = 1.5m,
                MaxDrawdown = 500000,
                TotalTrades = 50,
                WinRate = 60m
            }
        };

        _backtestService.RunBacktestAsync(
            Arg.Any<Guid>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>(),
            Arg.Any<CancellationToken>())
            .Returns(backtestResult);

        // Act
        var result = await _sut.GridSearchAsync(
            strategyId,
            parameterRanges,
            OptimizationObjective.MaxReturn,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.OptimalParameters.Should().ContainKey("Period");
        result.Value.EvaluatedCombinations.Should().Be(3); // 5, 10, 15
        result.Value.Type.Should().Be(Core.Enums.OptimizationType.GridSearch);
    }

    [Fact]
    public async Task GridSearchAsync_WithNonExistentStrategy_ShouldReturnError()
    {
        // Arrange
        var strategyId = Guid.NewGuid();

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(null));

        var parameterRanges = new Dictionary<string, ParameterRange>
        {
            { "Period", new ParameterRange(5, 15, 5, ParameterType.Integer) }
        };

        // Act
        var result = await _sut.GridSearchAsync(
            strategyId,
            parameterRanges,
            OptimizationObjective.MaxReturn,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }

    [Fact]
    public async Task GridSearchAsync_WithEmptyParameterRanges_ShouldReturnError()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        var emptyRanges = new Dictionary<string, ParameterRange>();

        // Act
        var result = await _sut.GridSearchAsync(
            strategyId,
            emptyRanges,
            OptimizationObjective.MaxReturn,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Optimization.NoParameters");
    }

    #endregion

    #region RandomSearchAsync Tests

    [Fact]
    public async Task RandomSearchAsync_WithValidParameters_ShouldReturnOptimizationResult()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        var parameterRanges = new Dictionary<string, ParameterRange>
        {
            { "Period", new ParameterRange(5, 50, 1, ParameterType.Integer) }
        };

        var backtestResult = new BacktestResult
        {
            BacktestId = Guid.NewGuid(),
            StrategyId = strategyId,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            InitialCapital = 10000000,
            FinalCapital = 11500000,
            Metrics = new PerformanceMetrics
            {
                TotalReturn = 15m,
                SharpeRatio = 2.0m,
                MaxDrawdown = 300000,
                TotalTrades = 40,
                WinRate = 65m
            }
        };

        _backtestService.RunBacktestAsync(
            Arg.Any<Guid>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>(),
            Arg.Any<CancellationToken>())
            .Returns(backtestResult);

        // Act
        var result = await _sut.RandomSearchAsync(
            strategyId,
            parameterRanges,
            OptimizationObjective.MaxSharpe,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            iterations: 10,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.EvaluatedCombinations.Should().Be(10);
        result.Value.Type.Should().Be(OptimizationType.RandomSearch);
        result.Value.Score.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RandomSearchAsync_WithInvalidIterations_ShouldReturnError(int invalidIterations)
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        var parameterRanges = new Dictionary<string, ParameterRange>
        {
            { "Period", new ParameterRange(5, 50, 1, ParameterType.Integer) }
        };

        // Act
        var result = await _sut.RandomSearchAsync(
            strategyId,
            parameterRanges,
            OptimizationObjective.MaxReturn,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            iterations: invalidIterations,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("Optimization.InvalidIterations");
    }

    #endregion

    #region GeneticAlgorithmAsync Tests

    [Fact]
    public async Task GeneticAlgorithmAsync_WithValidParameters_ShouldReturnOptimizationResult()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        var parameterRanges = new Dictionary<string, ParameterRange>
        {
            { "FastPeriod", new ParameterRange(5, 20, 1, ParameterType.Integer) },
            { "SlowPeriod", new ParameterRange(20, 50, 1, ParameterType.Integer) }
        };

        var backtestResult = new BacktestResult
        {
            BacktestId = Guid.NewGuid(),
            StrategyId = strategyId,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            InitialCapital = 10000000,
            FinalCapital = 12000000,
            Metrics = new PerformanceMetrics
            {
                TotalReturn = 20m,
                SharpeRatio = 2.5m,
                MaxDrawdown = 400000,
                TotalTrades = 60,
                WinRate = 70m
            }
        };

        _backtestService.RunBacktestAsync(
            Arg.Any<Guid>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>(),
            Arg.Any<CancellationToken>())
            .Returns(backtestResult);

        // Act
        var result = await _sut.GeneticAlgorithmAsync(
            strategyId,
            parameterRanges,
            OptimizationObjective.MaxSharpe,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            populationSize: 10,
            generations: 5,
            mutationRate: 0.1,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Type.Should().Be(OptimizationType.GeneticAlgorithm);
        result.Value.OptimalParameters.Should().HaveCount(2);
        result.Value.OptimalParameters.Should().ContainKey("FastPeriod");
        result.Value.OptimalParameters.Should().ContainKey("SlowPeriod");
    }

    [Theory]
    [InlineData(1, 5, 0.1)]      // Invalid population size
    [InlineData(10, 0, 0.1)]     // Invalid generations
    [InlineData(10, 5, -0.1)]    // Invalid mutation rate
    [InlineData(10, 5, 1.5)]     // Invalid mutation rate
    public async Task GeneticAlgorithmAsync_WithInvalidParameters_ShouldReturnError(
        int populationSize, int generations, double mutationRate)
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        var parameterRanges = new Dictionary<string, ParameterRange>
        {
            { "Period", new ParameterRange(5, 50, 1, ParameterType.Integer) }
        };

        // Act
        var result = await _sut.GeneticAlgorithmAsync(
            strategyId,
            parameterRanges,
            OptimizationObjective.MaxReturn,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            populationSize: populationSize,
            generations: generations,
            mutationRate: mutationRate,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().StartWith("Optimization.");
    }

    #endregion

    #region WalkForwardAnalysisAsync Tests

    [Fact]
    public async Task WalkForwardAnalysisAsync_WithValidParameters_ShouldReturnOptimizationResult()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        var parameterRanges = new Dictionary<string, ParameterRange>
        {
            { "Period", new ParameterRange(10, 30, 10, ParameterType.Integer) }
        };

        var backtestResult = new BacktestResult
        {
            BacktestId = Guid.NewGuid(),
            StrategyId = strategyId,
            StartDate = DateTime.UtcNow.AddDays(-60),
            EndDate = DateTime.UtcNow.AddDays(-30),
            InitialCapital = 10000000,
            FinalCapital = 10500000,
            Metrics = new PerformanceMetrics
            {
                TotalReturn = 5m,
                SharpeRatio = 1.2m,
                MaxDrawdown = 200000,
                TotalTrades = 20,
                WinRate = 55m
            }
        };

        var walkForwardResult = new WalkForwardBacktestResult
        {
            BacktestId = Guid.NewGuid(),
            StrategyId = strategyId,
            StartDate = DateTime.UtcNow.AddDays(-90),
            EndDate = DateTime.UtcNow,
            InitialCapital = 10000000,
            FinalCapital = 11000000,
            OverallMetrics = backtestResult.Metrics,
            PeriodResults = new List<WalkForwardPeriodResult>
            {
                new WalkForwardPeriodResult
                {
                    InSampleStart = DateTime.UtcNow.AddDays(-90),
                    InSampleEnd = DateTime.UtcNow.AddDays(-60),
                    OutOfSampleStart = DateTime.UtcNow.AddDays(-60),
                    OutOfSampleEnd = DateTime.UtcNow.AddDays(-30),
                    InSampleMetrics = backtestResult.Metrics,
                    OutOfSampleMetrics = backtestResult.Metrics
                }
            }
        };

        _backtestService.RunBacktestAsync(
            Arg.Any<Guid>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>(),
            Arg.Any<CancellationToken>())
            .Returns(backtestResult);

        _backtestService.RunWalkForwardBacktestAsync(
            Arg.Any<Guid>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(walkForwardResult);

        // Act
        var result = await _sut.WalkForwardAnalysisAsync(
            strategyId,
            parameterRanges,
            OptimizationObjective.MaxReturn,
            DateTime.UtcNow.AddDays(-90),
            DateTime.UtcNow,
            inSampleMonths: 1,
            outOfSampleMonths: 1,
            cancellationToken: CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Periods.Should().NotBeEmpty();
    }

    #endregion

    #region ParameterSensitivityAsync Tests

    [Fact]
    public async Task ParameterSensitivityAsync_WithValidParameters_ShouldReturnSensitivityResult()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var strategy = TradingStrategy.Create("Test", "Desc", Core.Entities.Strategy.StrategyType.Momentum, "User");

        _strategyRepository.GetByIdAsync(Arg.Is<StrategyId>(id => id.Value == strategyId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<TradingStrategy?>(strategy));

        var parameterName = "Period";
        var parameterRange = new ParameterRange(10, 30, 5, ParameterType.Integer);

        var backtestResult = new BacktestResult
        {
            BacktestId = Guid.NewGuid(),
            StrategyId = strategyId,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow,
            InitialCapital = 10000000,
            FinalCapital = 10800000,
            Metrics = new PerformanceMetrics
            {
                TotalReturn = 8m,
                SharpeRatio = 1.5m,
                MaxDrawdown = 250000,
                TotalTrades = 30,
                WinRate = 60m
            }
        };

        _backtestService.RunBacktestAsync(
            Arg.Any<Guid>(),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<decimal>(),
            Arg.Any<CancellationToken>())
            .Returns(backtestResult);

        // Act
        var result = await _sut.ParameterSensitivityAsync(
            strategyId,
            parameterName,
            parameterRange,
            OptimizationObjective.MaxReturn,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow,
            CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.ParameterName.Should().Be(parameterName);
        result.Value.Results.Should().HaveCountGreaterThan(0);
    }

    #endregion
}
