using AlgoTrading.TechnicalIndicators.Indicators;
using FluentAssertions;
using Xunit;

namespace AlgoTrading.TechnicalIndicators.Tests;

public class MovingAveragesTests
{
    [Fact]
    public void SMA_WithValidData_ShouldCalculateCorrectly()
    {
        // Arrange
        var sma = new SimpleMovingAverage(5);
        var data = new[] { 10m, 20m, 30m, 40m, 50m };

        // Act
        var result = sma.Calculate(data);

        // Assert
        result.Should().Be(30m); // (10 + 20 + 30 + 40 + 50) / 5
    }

    [Fact]
    public void SMA_WithInsufficientData_ShouldThrowException()
    {
        // Arrange
        var sma = new SimpleMovingAverage(10);
        var data = new[] { 10m, 20m, 30m };

        // Act & Assert
        var act = () => sma.Calculate(data);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient data*");
    }

    [Fact]
    public void SMA_WithMoreDataThanPeriod_ShouldUseLastNValues()
    {
        // Arrange
        var sma = new SimpleMovingAverage(3);
        var data = new[] { 10m, 20m, 30m, 40m, 50m };

        // Act
        var result = sma.Calculate(data);

        // Assert
        result.Should().Be(40m); // (30 + 40 + 50) / 3
    }

    [Fact]
    public void EMA_WithValidData_ShouldCalculateCorrectly()
    {
        // Arrange
        var ema = new ExponentialMovingAverage(5);
        var data = new[] { 10m, 15m, 20m, 25m, 30m, 35m };

        // Act
        var result = ema.Calculate(data);

        // Assert
        result.Should().BeGreaterThan(20m);
        result.Should().BeLessThan(35m);
        // EMA should be closer to recent values than SMA
        var sma = new SimpleMovingAverage(5);
        var smaResult = sma.Calculate(data);
        result.Should().BeGreaterThanOrEqualTo(smaResult); // EMA reacts faster to recent prices
    }

    [Fact]
    public void EMA_Update_ShouldCalculateIncrementally()
    {
        // Arrange
        var ema = new ExponentialMovingAverage(3);
        var data = new[] { 10m, 15m, 20m, 25m, 30m };

        // Act - Calculate full series
        var fullResult = ema.Calculate(data);

        // Act - Calculate incrementally
        ema.Reset();
        decimal incrementalResult = 0;
        foreach (var value in data)
        {
            incrementalResult = ema.Update(value);
        }

        // Assert - Both methods should yield same result
        incrementalResult.Should().BeApproximately(fullResult, 0.01m);
    }

    [Fact]
    public void WMA_WithValidData_ShouldGiveMoreWeightToRecentPrices()
    {
        // Arrange
        var wma = new WeightedMovingAverage(5);
        var data = new[] { 10m, 10m, 10m, 10m, 50m };

        // Act
        var result = wma.Calculate(data);

        // Assert
        // WMA = (10*1 + 10*2 + 10*3 + 10*4 + 50*5) / (1+2+3+4+5) = 350/15 = 23.333...
        result.Should().BeApproximately(23.333m, 0.01m);

        // Compare with SMA which would be (10+10+10+10+50)/5 = 18
        var sma = new SimpleMovingAverage(5);
        var smaResult = sma.Calculate(data);
        result.Should().BeGreaterThan(smaResult); // WMA gives more weight to recent spike
    }

    [Fact]
    public void HMA_WithValidData_ShouldCalculateCorrectly()
    {
        // Arrange
        var hma = new HullMovingAverage(9);
        var data = Enumerable.Range(1, 50).Select(i => (decimal)i).ToArray();

        // Act
        var result = hma.Calculate(data);

        // Assert
        result.Should().BeGreaterThan(0);
        result.Should().BeLessThanOrEqualTo(50m);
        // HMA should be more responsive to trend changes
    }

    [Fact]
    public void MovingAverages_WithZeroPeriod_ShouldThrowException()
    {
        // Arrange & Act & Assert
        var actSMA = () => new SimpleMovingAverage(0);
        actSMA.Should().Throw<ArgumentException>();

        var actEMA = () => new ExponentialMovingAverage(0);
        actEMA.Should().Throw<ArgumentException>();

        var actWMA = () => new WeightedMovingAverage(0);
        actWMA.Should().Throw<ArgumentException>();

        var actHMA = () => new HullMovingAverage(0);
        actHMA.Should().Throw<ArgumentException>();
    }
}
