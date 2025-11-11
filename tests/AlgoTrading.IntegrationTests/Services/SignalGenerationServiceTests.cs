using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using AppSignal = AlgoTrading.Application.Services.Strategy;

namespace AlgoTrading.IntegrationTests.Services;

/// <summary>
/// Integration tests for SignalGenerationService
/// </summary>
public class SignalGenerationServiceTests
{
    private readonly ILogger<SignalGenerationService> _logger;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IPriceDataRepository _priceDataRepository;
    private readonly SignalGenerationService _signalGenerationService;

    public SignalGenerationServiceTests()
    {
        _logger = Substitute.For<ILogger<SignalGenerationService>>();
        _strategyRepository = Substitute.For<IStrategyRepository>();
        _priceDataRepository = Substitute.For<IPriceDataRepository>();
        _signalGenerationService = new SignalGenerationService(
            _logger,
            _strategyRepository,
            _priceDataRepository);
    }

    [Fact]
    public async Task GenerateSignalAsync_WithActiveStrategy_ShouldReturnTradingSignal()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var stockCode = "005930"; // Samsung Electronics

        var strategy = CreateActiveStrategy("Test Strategy", stockCode);

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        var priceData = GenerateMockPriceData(stockCode, 60);
        _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                stockCode,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(priceData);

        // Act
        var result = await _signalGenerationService.GenerateSignalAsync(
            strategyId,
            stockCode);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.StockCode.Should().Be(stockCode);
        result.Value.StrategyId.Should().Be(strategy.Id.Value); // Use strategy's actual ID
        result.Value.Type.Should().BeOneOf(
            AppSignal.SignalType.Buy,
            AppSignal.SignalType.Sell,
            AppSignal.SignalType.Hold);
        result.Value.Indicators.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateSignalAsync_WithInactiveStrategy_ShouldReturnHoldSignal()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var stockCode = "005930";

        var strategy = Core.Entities.Strategy.TradingStrategy.Create(
            "Test Strategy",
            "Test strategy description",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");
        // Don't activate the strategy

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        // Act
        var result = await _signalGenerationService.GenerateSignalAsync(
            strategyId,
            stockCode);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Type.Should().Be(AppSignal.SignalType.Hold);
        result.Value.Strength.Should().Be(AppSignal.SignalStrength.Weak);
    }

    [Fact]
    public async Task GenerateSignalAsync_WithNonTargetStock_ShouldReturnHoldSignal()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var targetStock = "005930";
        var nonTargetStock = "000660"; // SK Hynix

        var strategy = CreateActiveStrategy("Test Strategy", targetStock);

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        // Act
        var result = await _signalGenerationService.GenerateSignalAsync(
            strategyId,
            nonTargetStock);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Type.Should().Be(AppSignal.SignalType.Hold);
    }

    [Fact]
    public async Task GenerateSignalAsync_WithNonExistentStrategy_ShouldReturnNotFoundError()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var stockCode = "005930";

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns((Core.Entities.Strategy.TradingStrategy?)null);

        // Act
        var result = await _signalGenerationService.GenerateSignalAsync(
            strategyId,
            stockCode);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }

    [Fact]
    public async Task GenerateSignalAsync_WithNoPriceData_ShouldReturnNotFoundError()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var stockCode = "005930";

        var strategy = CreateActiveStrategy("Test Strategy", stockCode);

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                stockCode,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<PriceData>()); // Empty list

        // Act
        var result = await _signalGenerationService.GenerateSignalAsync(
            strategyId,
            stockCode);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.NotFound);
    }

    [Fact]
    public async Task GenerateSignalsAsync_WithMultipleStocks_ShouldReturnMultipleSignals()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var stockCodes = new List<string> { "005930", "000660", "035720" };

        var strategy = Core.Entities.Strategy.TradingStrategy.Create(
            "Test Strategy",
            "Test strategy description",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");
        strategy.SetTargetStocks(new List<Core.ValueObjects.StockCode>()); // Empty means all stocks
        strategy.SetRiskParameters(10, 5, 10);

        var entryRule = Core.Entities.Strategy.StrategyRule.Create(
            "Test Entry Rule",
            "Test entry condition",
            Core.Entities.Strategy.RuleType.EntryLong,
            "test_condition",
            1);
        strategy.AddRule(entryRule);
        strategy.Activate();

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        foreach (var stockCode in stockCodes)
        {
            var priceData = GenerateMockPriceData(stockCode, 60);
            _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                    stockCode,
                    Arg.Any<DateTime>(),
                    Arg.Any<DateTime>(),
                    Arg.Any<CancellationToken>())
                .Returns(priceData);
        }

        // Act
        var result = await _signalGenerationService.GenerateSignalsAsync(
            strategyId,
            stockCodes);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task EvaluateEntryConditionsAsync_WithBullishMarket_ShouldReturnTrue()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var stockCode = "005930";

        var strategy = Core.Entities.Strategy.TradingStrategy.Create(
            "Test Strategy",
            "Test strategy description",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");

        strategy.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode(stockCode)
        });
        strategy.SetRiskParameters(10, 5, 10);

        var entryRule = Core.Entities.Strategy.StrategyRule.Create(
            "SMA Crossover Entry",
            "Enter when price crosses above SMA20",
            Core.Entities.Strategy.RuleType.EntryLong,
            "price > sma20",
            1);
        strategy.AddRule(entryRule);
        strategy.Activate();

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        // Generate bullish price data (uptrend above SMA)
        var priceData = GenerateBullishPriceData(stockCode, 60);
        _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                stockCode,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(priceData);

        // Act
        var result = await _signalGenerationService.EvaluateEntryConditionsAsync(
            strategyId,
            stockCode);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue(); // Bullish market should meet entry conditions
    }

    [Fact]
    public async Task EvaluateExitConditionsAsync_WithBearishMarket_ShouldReturnTrue()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        var stockCode = "005930";
        var positionId = Guid.NewGuid();

        var strategy = Core.Entities.Strategy.TradingStrategy.Create(
            "Test Strategy",
            "Test strategy description",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");

        strategy.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode(stockCode)
        });
        strategy.SetRiskParameters(10, 5, 10);

        var exitRule = Core.Entities.Strategy.StrategyRule.Create(
            "SMA Crossover Exit",
            "Exit when price crosses below SMA20",
            Core.Entities.Strategy.RuleType.ExitLong,
            "price < sma20",
            1);
        strategy.AddRule(exitRule);
        strategy.Activate();

        _strategyRepository.GetWithRulesAsync(strategyId, Arg.Any<CancellationToken>())
            .Returns(strategy);

        // Generate bearish price data (downtrend below SMA)
        var priceData = GenerateBearishPriceData(stockCode, 60);
        _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                stockCode,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(priceData);

        var latestPrice = priceData.Last();
        _priceDataRepository.GetLatestByStockCodeAsync(stockCode, Arg.Any<CancellationToken>())
            .Returns(latestPrice);

        // Act
        var result = await _signalGenerationService.EvaluateExitConditionsAsync(
            strategyId,
            stockCode,
            positionId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue(); // Bearish market should meet exit conditions
    }

    /// <summary>
    /// Create an active trading strategy for testing
    /// </summary>
    private Core.Entities.Strategy.TradingStrategy CreateActiveStrategy(string name, string stockCode)
    {
        var strategy = Core.Entities.Strategy.TradingStrategy.Create(
            name,
            "Test strategy description",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");

        strategy.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode(stockCode)
        });
        strategy.SetRiskParameters(10, 5, 10); // 10% max position, 5% stop loss, 10% take profit

        var entryRule = Core.Entities.Strategy.StrategyRule.Create(
            "Test Entry Rule",
            "Test entry condition",
            Core.Entities.Strategy.RuleType.EntryLong,
            "test_condition",
            1);
        strategy.AddRule(entryRule);

        strategy.Activate();

        return strategy;
    }

    /// <summary>
    /// Generate mock price data for testing
    /// </summary>
    private List<PriceData> GenerateMockPriceData(string stockCode, int days)
    {
        var priceDataList = new List<PriceData>();
        var startDate = DateTime.UtcNow.AddDays(-days);
        var currentPrice = 50000m;
        var random = new Random(42);

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);

            // Skip weekends
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // Random price movement
            var priceChange = (decimal)(random.NextDouble() * 0.04 - 0.02);
            currentPrice *= (1 + priceChange);

            var priceData = PriceData.Create(
                new Core.ValueObjects.StockCode(stockCode),
                currentPrice,
                currentPrice * 0.995m,
                currentPrice * 1.01m,
                currentPrice * 0.99m,
                currentPrice * 0.99m,
                random.Next(1000000, 5000000),
                new Core.ValueObjects.Money(currentPrice * 1000000, "KRW"),
                date);

            priceDataList.Add(priceData);
        }

        return priceDataList;
    }

    /// <summary>
    /// Generate bullish (uptrend) price data for testing
    /// </summary>
    private List<PriceData> GenerateBullishPriceData(string stockCode, int days)
    {
        var priceDataList = new List<PriceData>();
        var startDate = DateTime.UtcNow.AddDays(-days);
        var currentPrice = 40000m; // Start lower
        var random = new Random(42);

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // Bullish trend: mostly positive movement
            var priceChange = (decimal)(random.NextDouble() * 0.03 + 0.001); // 0.1% to 3.1% gain
            currentPrice *= (1 + priceChange);

            var priceData = PriceData.Create(
                new Core.ValueObjects.StockCode(stockCode),
                currentPrice,
                currentPrice * 0.995m,
                currentPrice * 1.01m,
                currentPrice * 0.99m,
                currentPrice * 0.98m,
                random.Next(1000000, 5000000),
                new Core.ValueObjects.Money(currentPrice * 1000000, "KRW"),
                date);

            priceDataList.Add(priceData);
        }

        return priceDataList;
    }

    /// <summary>
    /// Generate bearish (downtrend) price data for testing
    /// </summary>
    private List<PriceData> GenerateBearishPriceData(string stockCode, int days)
    {
        var priceDataList = new List<PriceData>();
        var startDate = DateTime.UtcNow.AddDays(-days);
        var currentPrice = 60000m; // Start higher
        var random = new Random(42);

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // Bearish trend: mostly negative movement
            var priceChange = (decimal)(random.NextDouble() * -0.03 - 0.001); // -3.1% to -0.1% loss
            currentPrice *= (1 + priceChange);

            var priceData = PriceData.Create(
                new Core.ValueObjects.StockCode(stockCode),
                currentPrice,
                currentPrice * 1.005m,
                currentPrice * 1.01m,
                currentPrice * 0.99m,
                currentPrice * 1.02m,
                random.Next(1000000, 5000000),
                new Core.ValueObjects.Money(currentPrice * 1000000, "KRW"),
                date);

            priceDataList.Add(priceData);
        }

        return priceDataList;
    }
}
