using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AlgoTrading.IntegrationTests.Services;

/// <summary>
/// Integration tests for BacktestService
/// </summary>
public class BacktestServiceTests
{
    private readonly ILogger<BacktestService> _logger;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IPriceDataRepository _priceDataRepository;
    private readonly BacktestService _backtestService;

    public BacktestServiceTests()
    {
        _logger = Substitute.For<ILogger<BacktestService>>();
        _strategyRepository = Substitute.For<IStrategyRepository>();
        _priceDataRepository = Substitute.For<IPriceDataRepository>();
        _backtestService = new BacktestService(_logger, _strategyRepository, _priceDataRepository);
    }

    [Fact]
    public async Task RunBacktestAsync_WithValidStrategy_ShouldReturnBacktestResult()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var initialCapital = 10000000m; // 10 million KRW

        var strategy = Core.Entities.Strategy.TradingStrategy.Create(
            "Test Strategy",
            "Test strategy description",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");

        strategy.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("005930")
        }); // Samsung Electronics
        strategy.SetRiskParameters(10, 5, 10); // 10% max position, 5% stop loss, 10% take profit

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        var priceData = GenerateMockPriceData("005930", startDate, endDate);
        _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                "005930",
                startDate,
                endDate,
                Arg.Any<CancellationToken>())
            .Returns(priceData);

        // Act
        var result = await _backtestService.RunBacktestAsync(
            strategyId,
            startDate,
            endDate,
            initialCapital);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.StrategyId.Should().Be(strategyId);
        result.Value.InitialCapital.Should().Be(initialCapital);
        result.Value.Metrics.Should().NotBeNull();
    }

    [Fact]
    public async Task RunBacktestAsync_WithInvalidStrategy_ShouldReturnNotFoundError()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var initialCapital = 10000000m;

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns((Core.Entities.Strategy.TradingStrategy?)null);

        // Act
        var result = await _backtestService.RunBacktestAsync(
            strategyId,
            startDate,
            endDate,
            initialCapital);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }

    [Fact]
    public async Task RunBacktestAsync_WithInvalidDateRange_ShouldReturnValidationError()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 31);
        var endDate = new DateTime(2024, 1, 1); // End date before start date
        var initialCapital = 10000000m;

        var strategy = Core.Entities.Strategy.TradingStrategy.Create(
            "Test Strategy",
            "Test strategy description",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        // Act
        var result = await _backtestService.RunBacktestAsync(
            strategyId,
            startDate,
            endDate,
            initialCapital);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.Validation);
    }

    [Fact]
    public async Task RunBacktestAsync_WithZeroCapital_ShouldReturnValidationError()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var initialCapital = 0m;

        var strategy = Core.Entities.Strategy.TradingStrategy.Create(
            "Test Strategy",
            "Test strategy description",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        // Act
        var result = await _backtestService.RunBacktestAsync(
            strategyId,
            startDate,
            endDate,
            initialCapital);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.Validation);
    }

    [Fact]
    public void CalculatePerformanceMetrics_WithWinningTrades_ShouldCalculateCorrectMetrics()
    {
        // Arrange
        var initialCapital = 10000000m;
        var trades = new List<BacktestTrade>
        {
            new BacktestTrade
            {
                StockCode = "005930",
                EntryTime = new DateTime(2024, 1, 2),
                ExitTime = new DateTime(2024, 1, 10),
                EntryPrice = 50000m,
                ExitPrice = 55000m,
                Quantity = 100,
                RealizedPL = 500000m, // 10% gain
                RealizedPLPercent = 0.10m
            },
            new BacktestTrade
            {
                StockCode = "005930",
                EntryTime = new DateTime(2024, 1, 15),
                ExitTime = new DateTime(2024, 1, 20),
                EntryPrice = 55000m,
                ExitPrice = 52000m,
                Quantity = 100,
                RealizedPL = -300000m, // ~5.5% loss
                RealizedPLPercent = -0.0545m
            }
        };

        // Act
        var metrics = _backtestService.CalculatePerformanceMetrics(trades, initialCapital);

        // Assert
        metrics.Should().NotBeNull();
        metrics.TotalTrades.Should().Be(2);
        metrics.WinningTrades.Should().Be(1);
        metrics.LosingTrades.Should().Be(1);
        metrics.WinRate.Should().Be(0.5m); // 50%
        metrics.TotalReturn.Should().Be(0.02m); // 2% overall return
        metrics.ProfitFactor.Should().BeGreaterThan(1); // More profit than loss
    }

    [Fact]
    public async Task RunWalkForwardBacktestAsync_WithValidParameters_ShouldReturnResult()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 3, 1); // 60 days
        var initialCapital = 10000000m;
        var inSampleDays = 20;
        var outOfSampleDays = 10;

        var strategy = Core.Entities.Strategy.TradingStrategy.Create(
            "Test Strategy",
            "Test strategy description",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");

        strategy.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("005930")
        });
        strategy.SetRiskParameters(10, 5, 10);

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        var priceData = GenerateMockPriceData("005930", startDate, endDate);
        _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                Arg.Any<string>(),
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(priceData);

        // Act
        var result = await _backtestService.RunWalkForwardBacktestAsync(
            strategyId,
            startDate,
            endDate,
            initialCapital,
            inSampleDays,
            outOfSampleDays);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.PeriodResults.Should().NotBeEmpty();
    }

    /// <summary>
    /// Generate mock price data for testing
    /// </summary>
    private List<PriceData> GenerateMockPriceData(string stockCode, DateTime startDate, DateTime endDate)
    {
        var priceDataList = new List<PriceData>();
        var currentDate = startDate;
        var currentPrice = 50000m;
        var random = new Random(42); // Fixed seed for reproducibility

        while (currentDate <= endDate)
        {
            // Skip weekends
            if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                // Generate random price movement (-2% to +2%)
                var priceChange = (decimal)(random.NextDouble() * 0.04 - 0.02);
                currentPrice *= (1 + priceChange);

                var openPrice = currentPrice * (1 + (decimal)(random.NextDouble() * 0.01 - 0.005));
                var highPrice = currentPrice * (1 + (decimal)(random.NextDouble() * 0.02));
                var lowPrice = currentPrice * (1 - (decimal)(random.NextDouble() * 0.02));
                var volume = random.Next(1000000, 5000000);

                var priceData = PriceData.Create(
                    new Core.ValueObjects.StockCode(stockCode),
                    currentPrice,
                    openPrice,
                    highPrice,
                    lowPrice,
                    currentPrice * 0.99m, // Previous close
                    volume,
                    new Core.ValueObjects.Money(currentPrice * volume, "KRW"),
                    currentDate.Date.AddHours(15)); // 3 PM close

                priceDataList.Add(priceData);
            }

            currentDate = currentDate.AddDays(1);
        }

        return priceDataList;
    }
}
