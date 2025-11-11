using AlgoTrading.Application.Services.MarketData;
using AlgoTrading.Application.Services.Strategy.Evaluation;
using FluentAssertions;
using Xunit;

namespace AlgoTrading.UnitTests.Services.Strategy;

/// <summary>
/// Unit tests for TechnicalIndicatorCalculator
/// </summary>
public class TechnicalIndicatorCalculatorTests
{
    private readonly TechnicalIndicatorCalculator _calculator;

    public TechnicalIndicatorCalculatorTests()
    {
        _calculator = new TechnicalIndicatorCalculator();
    }

    [Fact]
    public void CalculateSMA_WithSufficientData_ShouldReturnCorrectAverage()
    {
        // Arrange
        var data = CreatePriceData(new[] { 100m, 110m, 120m, 130m, 140m });

        // Act
        var sma = _calculator.CalculateSMA(data, 5);

        // Assert
        sma.Should().Be(120m); // Average of 100, 110, 120, 130, 140
    }

    [Fact]
    public void CalculateSMA_WithInsufficientData_ShouldReturnZero()
    {
        // Arrange
        var data = CreatePriceData(new[] { 100m, 110m });

        // Act
        var sma = _calculator.CalculateSMA(data, 5);

        // Assert
        sma.Should().Be(0m);
    }

    [Fact]
    public void CalculateEMA_WithSufficientData_ShouldReturnExponentialAverage()
    {
        // Arrange
        var data = CreatePriceData(new[] { 100m, 110m, 120m, 130m, 140m, 150m, 160m, 170m, 180m, 190m });

        // Act
        var ema = _calculator.CalculateEMA(data, 5);

        // Assert
        ema.Should().BeGreaterThan(0m);
        ema.Should().BeLessThanOrEqualTo(190m);
        ema.Should().BeGreaterThan(100m); // EMA should be weighted towards recent prices
    }

    [Fact]
    public void CalculateRSI_WithStrongUptrend_ShouldReturnHighValue()
    {
        // Arrange - Strong uptrend
        var data = CreatePriceData(new[]
        {
            100m, 105m, 110m, 115m, 120m, 125m, 130m, 135m, 140m, 145m,
            150m, 155m, 160m, 165m, 170m, 175m
        });

        // Act
        var rsi = _calculator.CalculateRSI(data, 14);

        // Assert
        rsi.Should().BeGreaterThan(70m); // Overbought territory
        rsi.Should().BeLessThanOrEqualTo(100m);
    }

    [Fact]
    public void CalculateRSI_WithStrongDowntrend_ShouldReturnLowValue()
    {
        // Arrange - Strong downtrend
        var data = CreatePriceData(new[]
        {
            175m, 170m, 165m, 160m, 155m, 150m, 145m, 140m, 135m, 130m,
            125m, 120m, 115m, 110m, 105m, 100m
        });

        // Act
        var rsi = _calculator.CalculateRSI(data, 14);

        // Assert
        rsi.Should().BeLessThan(30m); // Oversold territory
        rsi.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public void CalculateRSI_WithSidewaysMarket_ShouldReturnMidValue()
    {
        // Arrange - Sideways market
        var data = CreatePriceData(new[]
        {
            100m, 102m, 99m, 101m, 100m, 103m, 98m, 102m, 100m, 101m,
            99m, 102m, 100m, 101m, 99m, 100m
        });

        // Act
        var rsi = _calculator.CalculateRSI(data, 14);

        // Assert
        rsi.Should().BeGreaterThan(40m);
        rsi.Should().BeLessThan(60m); // Should be around neutral (50)
    }

    [Fact]
    public void CalculateMACD_ShouldReturnThreeValues()
    {
        // Arrange
        var data = CreatePriceData(Enumerable.Range(1, 50).Select(i => 100m + i * 2m).ToArray());

        // Act
        var (macd, signal, histogram) = _calculator.CalculateMACD(data);

        // Assert
        macd.Should().NotBe(0m);
        signal.Should().NotBe(0m);
        histogram.Should().Be(macd - signal);
    }

    [Fact]
    public void CalculateBollingerBands_ShouldReturnThreeBands()
    {
        // Arrange
        var data = CreatePriceData(Enumerable.Range(1, 30).Select(i => 100m + i).ToArray());

        // Act
        var (upper, middle, lower) = _calculator.CalculateBollingerBands(data, 20, 2);

        // Assert
        upper.Should().BeGreaterThan(middle);
        middle.Should().BeGreaterThan(lower);
        lower.Should().BeGreaterThan(0m);
    }

    [Fact]
    public void CalculateStochastic_WithInsufficientData_ShouldReturnNeutral()
    {
        // Arrange
        var data = CreatePriceData(new[] { 100m, 110m, 120m });

        // Act
        var (k, d) = _calculator.CalculateStochastic(data, 14, 3);

        // Assert - With insufficient data, should return neutral value (50)
        k.Should().Be(50m);
        d.Should().Be(50m);
    }

    [Fact]
    public void CalculateATR_WithSufficientData_ShouldReturnPositiveValue()
    {
        // Arrange - Need at least period + 1 data points (15 for period 14)
        var data = CreatePriceDataWithOHLC(
            new[]
            {
                (100m, 105m, 95m, 102m),
                (102m, 108m, 98m, 105m),
                (105m, 110m, 100m, 108m),
                (108m, 115m, 105m, 112m),
                (112m, 118m, 110m, 115m),
                (115m, 120m, 113m, 118m),
                (118m, 125m, 115m, 122m),
                (122m, 128m, 120m, 125m),
                (125m, 130m, 123m, 128m),
                (128m, 135m, 125m, 132m),
                (132m, 138m, 130m, 135m),
                (135m, 140m, 133m, 138m),
                (138m, 145m, 135m, 142m),
                (142m, 148m, 140m, 145m),
                (145m, 150m, 143m, 148m) // Added 15th data point
            });

        // Act
        var atr = _calculator.CalculateATR(data, 14);

        // Assert
        atr.Should().BeGreaterThan(0m);
        atr.Should().BeLessThan(50m); // Reasonable range for ATR
    }

    [Fact]
    public void CalculateROC_WithPositiveTrend_ShouldReturnPositiveValue()
    {
        // Arrange
        var data = CreatePriceData(new[] { 100m, 105m, 110m, 115m, 120m, 125m, 130m, 135m, 140m, 145m });

        // Act
        var roc = _calculator.CalculateROC(data, 5);

        // Assert
        roc.Should().BeGreaterThan(0m); // Positive rate of change
    }

    [Fact]
    public void CalculateAverageVolume_ShouldReturnCorrectAverage()
    {
        // Arrange
        var data = new List<HistoricalPriceData>();
        for (int i = 0; i < 20; i++)
        {
            data.Add(new HistoricalPriceData
            {
                StockCode = "005930",
                Date = DateTime.Now.AddDays(-i),
                Close = 100m,
                Volume = 1000000 + (i * 10000)
            });
        }

        // Act
        var avgVolume = _calculator.CalculateAverageVolume(data, 20);

        // Assert
        avgVolume.Should().BeGreaterThan(1000000m);
        avgVolume.Should().BeLessThan(1200000m);
    }

    [Fact]
    public void CalculateSMA_ForTwoPeriodsComparison_ShouldWork()
    {
        // Arrange - Test golden cross setup
        var data = CreatePriceData(Enumerable.Range(1, 60).Select(i => 100m + i * 2m).ToArray());

        // Act
        var shortSMA = _calculator.CalculateSMA(data, 20);
        var longSMA = _calculator.CalculateSMA(data, 50);

        // Assert - In uptrend, short SMA should be above long SMA (golden cross)
        shortSMA.Should().BeGreaterThan(longSMA);
    }

    [Fact]
    public void CalculateSMA_ForDowntrendComparison_ShouldWork()
    {
        // Arrange - Test death cross setup
        var data = CreatePriceData(Enumerable.Range(1, 60).Select(i => 200m - i * 2m).ToArray());

        // Act
        var shortSMA = _calculator.CalculateSMA(data, 20);
        var longSMA = _calculator.CalculateSMA(data, 50);

        // Assert - In downtrend, short SMA should be below long SMA (death cross)
        shortSMA.Should().BeLessThan(longSMA);
    }

    // Helper methods

    private List<HistoricalPriceData> CreatePriceData(decimal[] prices)
    {
        var data = new List<HistoricalPriceData>();
        var baseDate = DateTime.Now.AddDays(-prices.Length);

        for (int i = 0; i < prices.Length; i++)
        {
            data.Add(new HistoricalPriceData
            {
                StockCode = "005930",
                Date = baseDate.AddDays(i),
                Open = prices[i] * 0.99m,
                High = prices[i] * 1.01m,
                Low = prices[i] * 0.98m,
                Close = prices[i],
                Volume = 1000000,
                AdjustedClose = prices[i]
            });
        }

        return data;
    }

    private List<HistoricalPriceData> CreatePriceDataWithOHLC((decimal Open, decimal High, decimal Low, decimal Close)[] ohlc)
    {
        var data = new List<HistoricalPriceData>();
        var baseDate = DateTime.Now.AddDays(-ohlc.Length);

        for (int i = 0; i < ohlc.Length; i++)
        {
            data.Add(new HistoricalPriceData
            {
                StockCode = "005930",
                Date = baseDate.AddDays(i),
                Open = ohlc[i].Open,
                High = ohlc[i].High,
                Low = ohlc[i].Low,
                Close = ohlc[i].Close,
                Volume = 1000000,
                AdjustedClose = ohlc[i].Close
            });
        }

        return data;
    }
}
