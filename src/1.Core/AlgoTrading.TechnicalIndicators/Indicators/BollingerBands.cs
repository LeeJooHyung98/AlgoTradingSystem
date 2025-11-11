using AlgoTrading.TechnicalIndicators.Models;

namespace AlgoTrading.TechnicalIndicators.Indicators;

/// <summary>
/// Bollinger Bands indicator
/// Consists of a middle band (SMA) and upper/lower bands (standard deviations)
/// </summary>
public class BollingerBands : IIndicator<decimal, BollingerBandsResult>
{
    private readonly int _period;
    private readonly decimal _standardDeviations;

    /// <summary>
    /// Create Bollinger Bands indicator
    /// </summary>
    /// <param name="period">Period for moving average (typically 20)</param>
    /// <param name="standardDeviations">Number of standard deviations (typically 2)</param>
    public BollingerBands(int period = 20, decimal standardDeviations = 2)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be greater than 0", nameof(period));

        if (standardDeviations <= 0)
            throw new ArgumentException("Standard deviations must be greater than 0", nameof(standardDeviations));

        _period = period;
        _standardDeviations = standardDeviations;
    }

    public BollingerBandsResult Calculate(IEnumerable<decimal> data)
    {
        var dataList = data.TakeLast(_period).ToList();

        if (dataList.Count < _period)
            throw new InvalidOperationException($"Insufficient data. Required: {_period}, Available: {dataList.Count}");

        // Middle band: Simple Moving Average
        var middleBand = dataList.Average();
        var currentPrice = dataList.Last();

        // Calculate standard deviation
        var variance = dataList.Select(x => Math.Pow((double)(x - middleBand), 2)).Average();
        var stdDev = (decimal)Math.Sqrt(variance);

        // Upper and lower bands
        var upperBand = middleBand + (_standardDeviations * stdDev);
        var lowerBand = middleBand - (_standardDeviations * stdDev);

        return new BollingerBandsResult(upperBand, middleBand, lowerBand, currentPrice);
    }
}
