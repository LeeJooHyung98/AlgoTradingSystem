using AlgoTrading.TechnicalIndicators.Indicators;
using FluentAssertions;
using Xunit;

namespace AlgoTrading.TechnicalIndicators.Tests;

public class BollingerBandsTests
{
    [Fact]
    public void BollingerBands_WithValidData_ShouldCalculateCorrectly()
    {
        // Arrange
        var bb = new BollingerBands(period: 5, standardDeviations: 2);
        var data = new[] { 10m, 12m, 14m, 16m, 18m };

        // Act
        var result = bb.Calculate(data);

        // Assert
        result.Should().NotBeNull();
        result.MiddleBand.Should().Be(14m); // SMA of [10, 12, 14, 16, 18] = 14
        result.UpperBand.Should().BeGreaterThan(result.MiddleBand);
        result.LowerBand.Should().BeLessThan(result.MiddleBand);
        result.BandWidth.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BollingerBands_BandWidth_ShouldReflectVolatility()
    {
        // Arrange - Low volatility data
        var bbLow = new BollingerBands(period: 5, standardDeviations: 2);
        var lowVolData = new[] { 100m, 101m, 100m, 101m, 100m };

        // Arrange - High volatility data
        var bbHigh = new BollingerBands(period: 5, standardDeviations: 2);
        var highVolData = new[] { 80m, 90m, 100m, 110m, 120m };

        // Act
        var lowVolResult = bbLow.Calculate(lowVolData);
        var highVolResult = bbHigh.Calculate(highVolData);

        // Assert - Higher volatility should have wider bands
        highVolResult.BandWidth.Should().BeGreaterThan(lowVolResult.BandWidth);
    }

    [Fact]
    public void BollingerBands_PercentB_ShouldIndicatePosition()
    {
        // Arrange
        var bb = new BollingerBands(period: 5, standardDeviations: 2);
        var data = new[] { 10m, 12m, 14m, 16m, 18m };

        // Act
        var result = bb.Calculate(data);

        // Assert
        // Current price (18) is above middle band (14), so %B should be > 0.5
        result.PercentB.Should().BeGreaterThan(0.5m);
        result.PercentB.Should().BeLessThanOrEqualTo(1m);
    }

    [Fact]
    public void BollingerBands_WithInsufficientData_ShouldThrowException()
    {
        // Arrange
        var bb = new BollingerBands(period: 20);
        var data = new[] { 10m, 12m, 14m };

        // Act & Assert
        var act = () => bb.Calculate(data);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient data*");
    }

    [Fact]
    public void BollingerBands_WithCustomStdDev_ShouldAdjustBandWidth()
    {
        // Arrange
        var data = new[] { 10m, 12m, 14m, 16m, 18m, 20m };

        var bb2 = new BollingerBands(period: 5, standardDeviations: 2);
        var bb3 = new BollingerBands(period: 5, standardDeviations: 3);

        // Act
        var result2 = bb2.Calculate(data);
        var result3 = bb3.Calculate(data);

        // Assert
        result2.MiddleBand.Should().Be(result3.MiddleBand); // Same SMA
        result3.BandWidth.Should().BeGreaterThan(result2.BandWidth); // Wider bands with more std devs
    }

    [Fact]
    public void BollingerBands_WithZeroOrNegativeParameters_ShouldThrowException()
    {
        // Arrange & Act & Assert
        var actPeriod = () => new BollingerBands(period: 0);
        actPeriod.Should().Throw<ArgumentException>();

        var actStdDev = () => new BollingerBands(period: 20, standardDeviations: 0);
        actStdDev.Should().Throw<ArgumentException>();
    }
}
