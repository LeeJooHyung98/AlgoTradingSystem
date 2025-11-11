using AlgoTrading.TechnicalIndicators.Indicators;
using AlgoTrading.TechnicalIndicators.Models;
using FluentAssertions;
using Xunit;

namespace AlgoTrading.TechnicalIndicators.Tests;

public class AdvancedIndicatorsTests
{
    [Fact]
    public void ATR_WithValidData_ShouldCalculateCorrectly()
    {
        // Arrange
        var atr = new AverageTrueRange(5);
        var candles = GenerateSampleCandles(10);

        // Act
        var result = atr.Calculate(candles);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ATR_WithHighVolatility_ShouldReturnHigherValue()
    {
        // Arrange
        var atr = new AverageTrueRange(5);

        var lowVolCandles = GenerateCandlesWithVolatility(10, volatility: 1m);
        var highVolCandles = GenerateCandlesWithVolatility(10, volatility: 5m);

        // Act
        var lowVolResult = atr.Calculate(lowVolCandles);
        var highVolResult = atr.Calculate(highVolCandles);

        // Assert
        highVolResult.Value.Should().BeGreaterThan(lowVolResult.Value);
    }

    [Fact]
    public void ATR_CalculatePositionSize_ShouldReturnCorrectSize()
    {
        // Arrange
        var atrResult = new AtrResult(5m);
        var capital = 100000m;
        var riskPercent = 2m;

        // Act
        var positionSize = atrResult.CalculatePositionSize(capital, riskPercent);

        // Assert
        positionSize.Should().BeGreaterThan(0);
        positionSize.Should().Be(200); // (100000 * 0.02) / (5 * 2) = 200
    }

    [Fact]
    public void Stochastic_WithValidData_ShouldCalculateCorrectly()
    {
        // Arrange
        var stochastic = new StochasticOscillator(kPeriod: 5, dPeriod: 3, smoothK: 3);
        var candles = GenerateSampleCandles(30);

        // Act
        var result = stochastic.Calculate(candles);

        // Assert
        result.Should().NotBeNull();
        result.K.Should().BeInRange(0, 100);
        result.D.Should().BeInRange(0, 100);
    }

    [Fact]
    public void Stochastic_IsOversold_ShouldDetectCorrectly()
    {
        // Arrange
        var oversoldResult = new StochasticResult(k: 15m, d: 18m);
        var normalResult = new StochasticResult(k: 50m, d: 52m);

        // Act & Assert
        oversoldResult.IsOversold().Should().BeTrue();
        normalResult.IsOversold().Should().BeFalse();
    }

    [Fact]
    public void Stochastic_IsOverbought_ShouldDetectCorrectly()
    {
        // Arrange
        var overboughtResult = new StochasticResult(k: 85m, d: 82m);
        var normalResult = new StochasticResult(k: 50m, d: 52m);

        // Act & Assert
        overboughtResult.IsOverbought().Should().BeTrue();
        normalResult.IsOverbought().Should().BeFalse();
    }

    [Fact]
    public void Stochastic_IsBullishCrossover_ShouldDetectCorrectly()
    {
        // Arrange
        var previous = new StochasticResult(k: 45m, d: 50m);
        var current = new StochasticResult(k: 55m, d: 50m);

        // Act & Assert
        current.IsBullishCrossover(previous).Should().BeTrue();
    }

    [Fact]
    public void Stochastic_IsBearishCrossover_ShouldDetectCorrectly()
    {
        // Arrange
        var previous = new StochasticResult(k: 55m, d: 50m);
        var current = new StochasticResult(k: 45m, d: 50m);

        // Act & Assert
        current.IsBearishCrossover(previous).Should().BeTrue();
    }

    [Fact]
    public void VWAP_WithValidData_ShouldCalculateCorrectly()
    {
        // Arrange
        var vwap = new VolumeWeightedAveragePrice();
        var candles = new List<Candle>
        {
            new Candle(DateTime.Now, 100m, 105m, 95m, 102m, 1000),
            new Candle(DateTime.Now.AddMinutes(1), 102m, 108m, 100m, 106m, 2000),
            new Candle(DateTime.Now.AddMinutes(2), 106m, 110m, 104m, 108m, 1500)
        };

        // Act
        var result = vwap.Calculate(candles);

        // Assert
        result.Should().BeGreaterThan(0);
        result.Should().BeGreaterThan(candles.Min(c => c.Low));
        result.Should().BeLessThan(candles.Max(c => c.High));
    }

    [Fact]
    public void VWAP_WithHighVolumeAtHighPrice_ShouldBePulledUpward()
    {
        // Arrange
        var vwap = new VolumeWeightedAveragePrice();

        // Low price with low volume, high price with high volume
        var candles = new List<Candle>
        {
            new Candle(DateTime.Now, 100m, 102m, 98m, 100m, 1000),
            new Candle(DateTime.Now.AddMinutes(1), 110m, 112m, 108m, 110m, 10000) // High volume at high price
        };

        // Act
        var result = vwap.Calculate(candles);

        // Assert
        // VWAP should be closer to 110 than 100 due to volume weighting
        result.Should().BeGreaterThan(105m);
    }

    [Fact]
    public void VWAP_CalculateIntraday_ShouldFilterByDate()
    {
        // Arrange
        var vwap = new VolumeWeightedAveragePrice();
        var tradingDay = DateTime.Today;

        var candles = new List<Candle>
        {
            new Candle(tradingDay.AddHours(9), 100m, 102m, 98m, 100m, 1000),
            new Candle(tradingDay.AddHours(10), 102m, 104m, 100m, 102m, 1500),
            new Candle(tradingDay.AddDays(-1).AddHours(9), 90m, 92m, 88m, 90m, 2000) // Previous day
        };

        // Act
        var result = vwap.CalculateIntraday(candles, tradingDay);

        // Assert
        // Should only use today's data (2 candles)
        result.Should().BeGreaterThan(0);
        result.Should().BeGreaterThan(98m);
        result.Should().BeLessThan(105m);
    }

    // Helper methods
    private List<Candle> GenerateSampleCandles(int count)
    {
        var candles = new List<Candle>();
        var basePrice = 100m;
        var baseTime = DateTime.Now.AddDays(-count);

        for (int i = 0; i < count; i++)
        {
            var open = basePrice + i;
            var close = open + 2;
            var high = close + 1;
            var low = open - 1;

            candles.Add(new Candle(
                baseTime.AddDays(i),
                open,
                high,
                low,
                close,
                100000 + i * 1000));
        }

        return candles;
    }

    private List<Candle> GenerateCandlesWithVolatility(int count, decimal volatility)
    {
        var candles = new List<Candle>();
        var basePrice = 100m;
        var baseTime = DateTime.Now.AddDays(-count);

        for (int i = 0; i < count; i++)
        {
            var open = basePrice;
            var high = open + volatility;
            var low = open - volatility;
            var close = open + (volatility / 2);

            candles.Add(new Candle(
                baseTime.AddDays(i),
                open,
                high,
                low,
                close,
                100000));
        }

        return candles;
    }
}
