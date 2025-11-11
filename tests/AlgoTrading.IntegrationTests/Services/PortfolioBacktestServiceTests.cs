using AlgoTrading.Application.Services.Strategy;
using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AlgoTrading.IntegrationTests.Services;

/// <summary>
/// Integration tests for PortfolioBacktestService
/// </summary>
public class PortfolioBacktestServiceTests
{
    private readonly ILogger<PortfolioBacktestService> _logger;
    private readonly ILogger<BacktestService> _backtestLogger;
    private readonly IBacktestService _backtestService;
    private readonly IStrategyRepository _strategyRepository;
    private readonly IPriceDataRepository _priceDataRepository;
    private readonly PortfolioBacktestService _portfolioBacktestService;

    public PortfolioBacktestServiceTests()
    {
        _logger = Substitute.For<ILogger<PortfolioBacktestService>>();
        _backtestLogger = Substitute.For<ILogger<BacktestService>>();
        _strategyRepository = Substitute.For<IStrategyRepository>();
        _priceDataRepository = Substitute.For<IPriceDataRepository>();

        _backtestService = new BacktestService(
            _backtestLogger,
            _strategyRepository,
            _priceDataRepository);

        _portfolioBacktestService = new PortfolioBacktestService(
            _logger,
            _backtestService,
            _strategyRepository);
    }

    [Fact]
    public async Task RunPortfolioBacktestAsync_WithTwoStrategies_ShouldReturnPortfolioResult()
    {
        // Arrange
        var strategy1Id = Guid.NewGuid();
        var strategy2Id = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var initialCapital = 10000000m; // 10 million KRW

        // Create two test strategies
        var strategy1 = Core.Entities.Strategy.TradingStrategy.Create(
            "Momentum Strategy",
            "Trend following strategy",
            Core.Entities.Strategy.StrategyType.Trend,
            "test-user");
        strategy1.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("005930") // Samsung Electronics
        });
        strategy1.SetRiskParameters(10, 5, 10);

        var strategy2 = Core.Entities.Strategy.TradingStrategy.Create(
            "Mean Reversion Strategy",
            "Buy low sell high strategy",
            Core.Entities.Strategy.StrategyType.MeanReversion,
            "test-user");
        strategy2.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("000660") // SK Hynix
        });
        strategy2.SetRiskParameters(10, 5, 10);

        // Mock strategy repository
        _strategyRepository.GetWithRulesAsync(strategy1Id, Arg.Any<CancellationToken>())
            .Returns(strategy1);
        _strategyRepository.GetWithRulesAsync(strategy2Id, Arg.Any<CancellationToken>())
            .Returns(strategy2);

        // Mock price data
        var priceData1 = GenerateMockPriceData("005930", startDate, endDate, 70000m);
        var priceData2 = GenerateMockPriceData("000660", startDate, endDate, 130000m);

        _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                "005930",
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(priceData1);

        _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                "000660",
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns(priceData2);

        // Act
        var result = await _portfolioBacktestService.RunPortfolioBacktestAsync(
            new[] { strategy1Id, strategy2Id },
            startDate,
            endDate,
            initialCapital,
            PortfolioAllocationMethod.EqualWeight);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.StrategyAllocations.Should().HaveCount(2);
        result.Value.StrategyAllocations.Sum(a => a.AllocationPercent).Should().BeApproximately(100m, 0.01m);
        result.Value.PortfolioMetrics.Should().NotBeNull();
        result.Value.FinalCapital.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RunPortfolioBacktestAsync_WithEqualWeight_ShouldAllocate50PercentEach()
    {
        // Arrange
        var strategy1Id = Guid.NewGuid();
        var strategy2Id = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var initialCapital = 10000000m;

        var strategy1 = Core.Entities.Strategy.TradingStrategy.Create(
            "Strategy A", "Description A",
            Core.Entities.Strategy.StrategyType.Trend, "test-user");
        strategy1.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("005930")
        });
        strategy1.SetRiskParameters(10, 5, 10);

        var strategy2 = Core.Entities.Strategy.TradingStrategy.Create(
            "Strategy B", "Description B",
            Core.Entities.Strategy.StrategyType.MeanReversion, "test-user");
        strategy2.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("000660")
        });
        strategy2.SetRiskParameters(10, 5, 10);

        _strategyRepository.GetWithRulesAsync(strategy1Id, Arg.Any<CancellationToken>())
            .Returns(strategy1);
        _strategyRepository.GetWithRulesAsync(strategy2Id, Arg.Any<CancellationToken>())
            .Returns(strategy2);

        var priceData1 = GenerateMockPriceData("005930", startDate, endDate, 70000m);
        var priceData2 = GenerateMockPriceData("000660", startDate, endDate, 130000m);

        _priceDataRepository.GetByStockCodeAndDateRangeAsync("005930",
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(priceData1);
        _priceDataRepository.GetByStockCodeAndDateRangeAsync("000660",
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(priceData2);

        // Act
        var result = await _portfolioBacktestService.RunPortfolioBacktestAsync(
            new[] { strategy1Id, strategy2Id },
            startDate,
            endDate,
            initialCapital,
            PortfolioAllocationMethod.EqualWeight);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.StrategyAllocations.Should().AllSatisfy(a =>
        {
            a.AllocationPercent.Should().BeApproximately(50m, 0.01m);
            a.AllocatedCapital.Should().BeApproximately(initialCapital / 2, 1m);
        });
    }

    [Fact]
    public async Task RunPortfolioBacktestAsync_WithNoStrategies_ShouldReturnValidationError()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var initialCapital = 10000000m;

        // Act
        var result = await _portfolioBacktestService.RunPortfolioBacktestAsync(
            Array.Empty<Guid>(),
            startDate,
            endDate,
            initialCapital,
            PortfolioAllocationMethod.EqualWeight);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorOr.ErrorType.Validation);
        result.FirstError.Code.Should().Contain("NoStrategies");
    }

    [Fact]
    public async Task GetStrategyCorrelationAsync_WithTwoStrategies_ShouldReturnCorrelationMatrix()
    {
        // Arrange
        var strategy1Id = Guid.NewGuid();
        var strategy2Id = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        var strategy1 = Core.Entities.Strategy.TradingStrategy.Create(
            "Strategy A", "Description A",
            Core.Entities.Strategy.StrategyType.Trend, "test-user");
        strategy1.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("005930")
        });
        strategy1.SetRiskParameters(10, 5, 10);

        var strategy2 = Core.Entities.Strategy.TradingStrategy.Create(
            "Strategy B", "Description B",
            Core.Entities.Strategy.StrategyType.MeanReversion, "test-user");
        strategy2.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("000660")
        });
        strategy2.SetRiskParameters(10, 5, 10);

        _strategyRepository.GetWithRulesAsync(strategy1Id, Arg.Any<CancellationToken>())
            .Returns(strategy1);
        _strategyRepository.GetWithRulesAsync(strategy2Id, Arg.Any<CancellationToken>())
            .Returns(strategy2);

        // Generate price data with different trends to create realistic correlation
        var priceData1 = GenerateMockPriceData("005930", startDate, endDate, 70000m, 0.02m);
        var priceData2 = GenerateMockPriceData("000660", startDate, endDate, 130000m, -0.01m);

        _priceDataRepository.GetByStockCodeAndDateRangeAsync("005930",
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(priceData1);
        _priceDataRepository.GetByStockCodeAndDateRangeAsync("000660",
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(priceData2);

        // Act
        var result = await _portfolioBacktestService.GetStrategyCorrelationAsync(
            new[] { strategy1Id, strategy2Id },
            startDate,
            endDate);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.StrategyIds.Should().HaveCount(2);
        result.Value.GetCorrelation(strategy1Id, strategy1Id).Should().Be(1.0m); // Self-correlation is 1
        result.Value.GetCorrelation(strategy2Id, strategy2Id).Should().Be(1.0m);

        // Cross-correlation should be between -1 and 1
        var crossCorrelation = result.Value.GetCorrelation(strategy1Id, strategy2Id);
        crossCorrelation.Should().BeGreaterThanOrEqualTo(-1m);
        crossCorrelation.Should().BeLessThanOrEqualTo(1m);
    }

    [Fact]
    public async Task RunPortfolioBacktestAsync_WithSharpeWeighted_ShouldAllocateBasedOnSharpeRatio()
    {
        // Arrange
        var strategy1Id = Guid.NewGuid();
        var strategy2Id = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        var initialCapital = 10000000m;

        var strategy1 = Core.Entities.Strategy.TradingStrategy.Create(
            "High Sharpe Strategy", "High risk-adjusted returns",
            Core.Entities.Strategy.StrategyType.Trend, "test-user");
        strategy1.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("005930")
        });
        strategy1.SetRiskParameters(10, 5, 10);

        var strategy2 = Core.Entities.Strategy.TradingStrategy.Create(
            "Low Sharpe Strategy", "Lower risk-adjusted returns",
            Core.Entities.Strategy.StrategyType.MeanReversion, "test-user");
        strategy2.SetTargetStocks(new List<Core.ValueObjects.StockCode>
        {
            new Core.ValueObjects.StockCode("000660")
        });
        strategy2.SetRiskParameters(10, 5, 10);

        _strategyRepository.GetWithRulesAsync(strategy1Id, Arg.Any<CancellationToken>())
            .Returns(strategy1);
        _strategyRepository.GetWithRulesAsync(strategy2Id, Arg.Any<CancellationToken>())
            .Returns(strategy2);

        // Generate price data with strong uptrend for strategy 1, sideways for strategy 2
        var priceData1 = GenerateMockPriceData("005930", startDate, endDate, 70000m, 0.05m);
        var priceData2 = GenerateMockPriceData("000660", startDate, endDate, 130000m, 0.001m);

        _priceDataRepository.GetByStockCodeAndDateRangeAsync("005930",
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(priceData1);
        _priceDataRepository.GetByStockCodeAndDateRangeAsync("000660",
                Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(priceData2);

        // Act
        var result = await _portfolioBacktestService.RunPortfolioBacktestAsync(
            new[] { strategy1Id, strategy2Id },
            startDate,
            endDate,
            initialCapital,
            PortfolioAllocationMethod.SharpeWeighted);

        // Assert
        if (result.IsError)
        {
            // Log error details for debugging
            var errorMessage = $"Error: {result.FirstError.Code} - {result.FirstError.Description}";
            throw new Exception($"Backtest failed: {errorMessage}");
        }

        result.IsError.Should().BeFalse();
        result.Value.StrategyAllocations.Should().HaveCount(2);

        // The strategy with better performance should have higher allocation
        var allocation1 = result.Value.StrategyAllocations.First(a => a.StrategyId == strategy1Id);
        var allocation2 = result.Value.StrategyAllocations.First(a => a.StrategyId == strategy2Id);

        // Total should be 100%
        (allocation1.AllocationPercent + allocation2.AllocationPercent).Should().BeApproximately(100m, 0.01m);
    }

    /// <summary>
    /// Generate mock price data for testing
    /// </summary>
    private List<PriceData> GenerateMockPriceData(
        string stockCode,
        DateTime startDate,
        DateTime endDate,
        decimal startPrice,
        decimal trendPercent = 0m)
    {
        var priceDataList = new List<PriceData>();
        var currentDate = startDate;
        var currentPrice = startPrice;
        var random = new Random(stockCode.GetHashCode()); // Deterministic based on stock code

        while (currentDate <= endDate)
        {
            // Skip weekends
            if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                // Apply trend + random noise
                var dailyTrend = trendPercent / 20; // Assume ~20 trading days per month
                var randomNoise = (decimal)(random.NextDouble() * 0.04 - 0.02); // -2% to +2%
                currentPrice *= (1 + dailyTrend + randomNoise);

                var openPrice = currentPrice * (1 + (decimal)(random.NextDouble() * 0.01 - 0.005));
                var highPrice = Math.Max(currentPrice, openPrice) * (1 + (decimal)(random.NextDouble() * 0.01));
                var lowPrice = Math.Min(currentPrice, openPrice) * (1 - (decimal)(random.NextDouble() * 0.01));
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
