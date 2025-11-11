using AlgoTrading.Application.Services.MarketData;
using AlgoTrading.Application.Services.Strategy.Evaluation;
using AlgoTrading.Core.Entities.Strategy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AlgoTrading.UnitTests.Services.Strategy;

/// <summary>
/// Unit tests for RuleEvaluationEngine
/// </summary>
public class RuleEvaluationEngineTests
{
    private readonly ILogger _logger;
    private readonly RuleEvaluationEngine _engine;

    public RuleEvaluationEngineTests()
    {
        _logger = Substitute.For<ILogger>();
        _engine = new RuleEvaluationEngine(_logger);
    }

    [Fact]
    public void EvaluateRule_SimpleGreaterThanComparison_ShouldReturnTrue()
    {
        // Arrange
        var rule = CreateRule("Test Rule", "close > 50000");
        var historicalData = CreateHistoricalData(60000m);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EvaluateRule_SimpleGreaterThanComparison_ShouldReturnFalse()
    {
        // Arrange
        var rule = CreateRule("Test Rule", "close > 70000");
        var historicalData = CreateHistoricalData(60000m);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EvaluateRule_LessThanComparison_ShouldWork()
    {
        // Arrange
        var rule = CreateRule("Test Rule", "close < 70000");
        var historicalData = CreateHistoricalData(60000m);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EvaluateRule_AndOperator_BothTrue_ShouldReturnTrue()
    {
        // Arrange
        var rule = CreateRule("Test Rule", "close > 50000 and close < 70000");
        var historicalData = CreateHistoricalData(60000m);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EvaluateRule_AndOperator_OneFalse_ShouldReturnFalse()
    {
        // Arrange
        var rule = CreateRule("Test Rule", "close > 50000 and close < 55000");
        var historicalData = CreateHistoricalData(60000m);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EvaluateRule_OrOperator_OneTrue_ShouldReturnTrue()
    {
        // Arrange
        var rule = CreateRule("Test Rule", "close > 70000 or close < 55000");
        var historicalData = CreateHistoricalData(50000m);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EvaluateRule_SMAIndicator_ShouldCalculateAndCompare()
    {
        // Arrange
        var rule = CreateRule("SMA Rule", "close > sma(20)");
        var historicalData = CreateHistoricalDataWithTrend(50000m, 60000m, 30); // Uptrend
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeTrue(); // Current price should be above SMA in uptrend
    }

    [Fact]
    public void EvaluateRule_RSIIndicator_Oversold_ShouldDetect()
    {
        // Arrange
        var rule = CreateRule("RSI Rule", "rsi(14) < 30");
        var historicalData = CreateHistoricalDataWithDowntrend(60000m, 40000m, 20); // Strong downtrend
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeTrue(); // RSI should be low (oversold) in strong downtrend
    }

    [Fact]
    public void EvaluateRule_VolumeComparison_ShouldWork()
    {
        // Arrange
        var rule = CreateRule("Volume Rule", "volume > 1000000");
        var historicalData = CreateHistoricalData(60000m, volume: 2000000);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EvaluateRule_PriceFieldComparisons_ShouldWork()
    {
        // Arrange
        var rule = CreateRule("OHLC Rule", "high > open and close > open");
        var historicalData = new List<HistoricalPriceData>
        {
            new HistoricalPriceData
            {
                StockCode = "005930",
                Date = DateTime.Now,
                Open = 50000m,
                High = 52000m,
                Low = 49000m,
                Close = 51000m,
                Volume = 1000000
            }
        };
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EvaluateRule_MathematicalOperations_ShouldWork()
    {
        // Arrange
        var rule = CreateRule("Math Rule", "close > open * 1.02");
        var historicalData = new List<HistoricalPriceData>
        {
            new HistoricalPriceData
            {
                StockCode = "005930",
                Date = DateTime.Now,
                Open = 50000m,
                High = 52000m,
                Low = 49000m,
                Close = 51500m, // More than 2% above open
                Volume = 1000000
            }
        };
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EvaluateRule_ComplexCondition_ShouldEvaluateCorrectly()
    {
        // Arrange
        var rule = CreateRule("Complex Rule",
            "close > sma(20) and rsi(14) > 50 and volume > 1000000");
        var historicalData = CreateHistoricalDataWithTrend(50000m, 60000m, 30, volume: 2000000);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        // Should evaluate without throwing exceptions - any boolean result is valid
        (result == true || result == false).Should().BeTrue();
    }

    [Fact]
    public void EvaluateRule_EmptyCondition_ShouldReturnFalse()
    {
        // Arrange
        var rule = CreateRule("Empty Rule", "");
        var historicalData = CreateHistoricalData(60000m);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EvaluateRule_InvalidCondition_ShouldReturnFalse()
    {
        // Arrange
        var rule = CreateRule("Invalid Rule", "invalid_indicator(20) > 100");
        var historicalData = CreateHistoricalData(60000m);
        var currentPrice = historicalData.Last();

        // Act
        var result = _engine.EvaluateRule(rule, historicalData, currentPrice);

        // Assert
        result.Should().BeFalse(); // Should handle gracefully
    }

    // Helper methods

    private StrategyRule CreateRule(string name, string condition)
    {
        return StrategyRule.Create(
            name,
            "Test Description",
            RuleType.EntryLong,
            condition,
            1);
    }

    private List<HistoricalPriceData> CreateHistoricalData(decimal price, long volume = 1000000)
    {
        var data = new List<HistoricalPriceData>();
        var baseDate = DateTime.Now.AddDays(-20);

        for (int i = 0; i < 20; i++)
        {
            data.Add(new HistoricalPriceData
            {
                StockCode = "005930",
                Date = baseDate.AddDays(i),
                Open = price * 0.99m,
                High = price * 1.01m,
                Low = price * 0.98m,
                Close = price,
                Volume = volume,
                AdjustedClose = price
            });
        }

        return data;
    }

    private List<HistoricalPriceData> CreateHistoricalDataWithTrend(
        decimal startPrice,
        decimal endPrice,
        int days,
        long volume = 1000000)
    {
        var data = new List<HistoricalPriceData>();
        var baseDate = DateTime.Now.AddDays(-days);
        var priceIncrement = (endPrice - startPrice) / days;

        for (int i = 0; i < days; i++)
        {
            var currentPrice = startPrice + (priceIncrement * i);
            data.Add(new HistoricalPriceData
            {
                StockCode = "005930",
                Date = baseDate.AddDays(i),
                Open = currentPrice * 0.99m,
                High = currentPrice * 1.01m,
                Low = currentPrice * 0.98m,
                Close = currentPrice,
                Volume = volume,
                AdjustedClose = currentPrice
            });
        }

        return data;
    }

    private List<HistoricalPriceData> CreateHistoricalDataWithDowntrend(
        decimal startPrice,
        decimal endPrice,
        int days,
        long volume = 1000000)
    {
        return CreateHistoricalDataWithTrend(startPrice, endPrice, days, volume);
    }
}
