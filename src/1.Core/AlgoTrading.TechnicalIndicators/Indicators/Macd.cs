using AlgoTrading.TechnicalIndicators.Models;

namespace AlgoTrading.TechnicalIndicators.Indicators;

/// <summary>
/// MACD (Moving Average Convergence Divergence) indicator
/// Trend-following momentum indicator showing relationship between two EMAs
/// </summary>
public class Macd : IIndicator<decimal, MacdResult>
{
    private readonly int _fastPeriod;
    private readonly int _slowPeriod;
    private readonly int _signalPeriod;

    /// <summary>
    /// Create MACD indicator
    /// </summary>
    /// <param name="fastPeriod">Fast EMA period (typically 12)</param>
    /// <param name="slowPeriod">Slow EMA period (typically 26)</param>
    /// <param name="signalPeriod">Signal line period (typically 9)</param>
    public Macd(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
    {
        if (fastPeriod <= 0)
            throw new ArgumentException("Fast period must be greater than 0", nameof(fastPeriod));

        if (slowPeriod <= fastPeriod)
            throw new ArgumentException("Slow period must be greater than fast period", nameof(slowPeriod));

        if (signalPeriod <= 0)
            throw new ArgumentException("Signal period must be greater than 0", nameof(signalPeriod));

        _fastPeriod = fastPeriod;
        _slowPeriod = slowPeriod;
        _signalPeriod = signalPeriod;
    }

    public MacdResult Calculate(IEnumerable<decimal> data)
    {
        var dataList = data.ToList();
        var minRequired = _slowPeriod + _signalPeriod;

        if (dataList.Count < minRequired)
            throw new InvalidOperationException($"Insufficient data. Required: {minRequired}, Available: {dataList.Count}");

        // Calculate EMAs
        var fastEma = new ExponentialMovingAverage(_fastPeriod);
        var slowEma = new ExponentialMovingAverage(_slowPeriod);

        // Calculate MACD line (Fast EMA - Slow EMA)
        var macdLine = new List<decimal>();

        for (int i = _slowPeriod - 1; i < dataList.Count; i++)
        {
            var subset = dataList.Take(i + 1);
            var fast = fastEma.Calculate(subset);
            var slow = slowEma.Calculate(subset);
            macdLine.Add(fast - slow);
        }

        // Calculate Signal line (EMA of MACD line)
        var signalEma = new ExponentialMovingAverage(_signalPeriod);
        var signal = signalEma.Calculate(macdLine);

        var macdValue = macdLine.Last();

        return new MacdResult(macdValue, signal);
    }

    /// <summary>
    /// Calculate MACD for full historical series
    /// </summary>
    public List<MacdResult> CalculateSeries(IEnumerable<decimal> data)
    {
        var dataList = data.ToList();
        var results = new List<MacdResult>();

        var fastEma = new ExponentialMovingAverage(_fastPeriod);
        var slowEma = new ExponentialMovingAverage(_slowPeriod);

        // Calculate MACD line
        var macdLine = new List<decimal>();

        for (int i = _slowPeriod - 1; i < dataList.Count; i++)
        {
            var subset = dataList.Take(i + 1);
            var fast = fastEma.Calculate(subset);
            var slow = slowEma.Calculate(subset);
            macdLine.Add(fast - slow);
        }

        // Calculate Signal line for each point
        for (int i = _signalPeriod - 1; i < macdLine.Count; i++)
        {
            var signalEma = new ExponentialMovingAverage(_signalPeriod);
            var signal = signalEma.Calculate(macdLine.Take(i + 1));

            results.Add(new MacdResult(macdLine[i], signal));
        }

        return results;
    }
}
