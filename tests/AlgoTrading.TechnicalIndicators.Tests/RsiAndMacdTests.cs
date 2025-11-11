using AlgoTrading.TechnicalIndicators.Indicators;
using AlgoTrading.TechnicalIndicators.Models;
using FluentAssertions;
using Xunit;

namespace AlgoTrading.TechnicalIndicators.Tests;

public class RsiAndMacdTests
{
    [Fact]
    public void RSI_WithUptrend_ShouldBeAbove50()
    {
        // Arrange - Uptrend data
        var rsi = new RelativeStrengthIndex(14);
        var data = Enumerable.Range(1, 30).Select(i => (decimal)i).ToArray();

        // Act
        var result = rsi.Calculate(data);

        // Assert
        result.Should().BeGreaterThan(50m);
        result.Should().BeLessThanOrEqualTo(100m);
    }

    [Fact]
    public void RSI_WithDowntrend_ShouldBeBelow50()
    {
        // Arrange - Downtrend data
        var rsi = new RelativeStrengthIndex(14);
        var data = Enumerable.Range(1, 30).Reverse().Select(i => (decimal)i).ToArray();

        // Act
        var result = rsi.Calculate(data);

        // Assert
        result.Should().BeLessThan(50m);
        result.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public void RSI_WithAllGains_ShouldReturn100()
    {
        // Arrange
        var rsi = new RelativeStrengthIndex(5);
        var data = new[] { 10m, 11m, 12m, 13m, 14m, 15m };

        // Act
        var result = rsi.Calculate(data);

        // Assert
        result.Should().Be(100m);
    }

    [Fact]
    public void RSI_IsOversold_ShouldDetectCorrectly()
    {
        // Arrange
        var oversoldRsi = 25m;
        var normalRsi = 50m;

        // Act & Assert
        RelativeStrengthIndex.IsOversold(oversoldRsi).Should().BeTrue();
        RelativeStrengthIndex.IsOversold(normalRsi).Should().BeFalse();
    }

    [Fact]
    public void RSI_IsOverbought_ShouldDetectCorrectly()
    {
        // Arrange
        var overboughtRsi = 75m;
        var normalRsi = 50m;

        // Act & Assert
        RelativeStrengthIndex.IsOverbought(overboughtRsi).Should().BeTrue();
        RelativeStrengthIndex.IsOverbought(normalRsi).Should().BeFalse();
    }

    [Fact]
    public void RSI_CalculateSeries_ShouldReturnMultipleValues()
    {
        // Arrange
        var rsi = new RelativeStrengthIndex(5);
        var data = Enumerable.Range(1, 20).Select(i => (decimal)i).ToArray();

        // Act
        var results = rsi.CalculateSeries(data);

        // Assert
        results.Should().NotBeEmpty();
        results.Count.Should().Be(15); // 20 prices -> 19 changes -> First RSI at position 5 -> 15 RSI values
        results.All(r => r >= 0 && r <= 100).Should().BeTrue();
    }

    [Fact]
    public void MACD_WithUptrend_ShouldCalculateCorrectly()
    {
        // Arrange
        var macd = new Macd(fastPeriod: 5, slowPeriod: 10, signalPeriod: 3);
        var data = Enumerable.Range(1, 50).Select(i => (decimal)i).ToArray();

        // Act
        var result = macd.Calculate(data);

        // Assert
        result.Should().NotBeNull();
        result.Macd.Should().BeGreaterThan(0); // MACD should be positive in uptrend
        result.Signal.Should().BeGreaterThan(0); // Signal should also be positive
        // Histogram is MACD - Signal (can be slightly negative due to rounding)
        Math.Abs(result.Histogram).Should().BeLessThan(1); // Should be close to zero
    }

    [Fact]
    public void MACD_WithDowntrend_ShouldShowNegativeHistogram()
    {
        // Arrange
        var macd = new Macd(fastPeriod: 5, slowPeriod: 10, signalPeriod: 3);
        var data = Enumerable.Range(1, 50).Reverse().Select(i => (decimal)i).ToArray();

        // Act
        var result = macd.Calculate(data);

        // Assert
        result.Should().NotBeNull();
        result.Macd.Should().BeLessThan(0);
        result.Histogram.Should().BeLessThan(0); // Negative histogram in downtrend
    }

    [Fact]
    public void MACD_IsBullishCrossover_ShouldDetectCorrectly()
    {
        // Arrange
        var previous = new MacdResult(macd: -1m, signal: 0m);
        var current = new MacdResult(macd: 1m, signal: 0m);

        // Act & Assert
        current.IsBullishCrossover(previous).Should().BeTrue();
    }

    [Fact]
    public void MACD_IsBearishCrossover_ShouldDetectCorrectly()
    {
        // Arrange
        var previous = new MacdResult(macd: 1m, signal: 0m);
        var current = new MacdResult(macd: -1m, signal: 0m);

        // Act & Assert
        current.IsBearishCrossover(previous).Should().BeTrue();
    }

    [Fact]
    public void MACD_CalculateSeries_ShouldReturnMultipleValues()
    {
        // Arrange
        var macd = new Macd(fastPeriod: 12, slowPeriod: 26, signalPeriod: 9);
        var data = Enumerable.Range(1, 100).Select(i => (decimal)i).ToArray();

        // Act
        var results = macd.CalculateSeries(data);

        // Assert
        results.Should().NotBeEmpty();
        results.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RSI_WithInsufficientData_ShouldThrowException()
    {
        // Arrange
        var rsi = new RelativeStrengthIndex(14);
        var data = new[] { 10m, 11m, 12m };

        // Act & Assert
        var act = () => rsi.Calculate(data);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient data*");
    }

    [Fact]
    public void MACD_WithInsufficientData_ShouldThrowException()
    {
        // Arrange
        var macd = new Macd(12, 26, 9);
        var data = new[] { 10m, 11m, 12m };

        // Act & Assert
        var act = () => macd.Calculate(data);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient data*");
    }
}
