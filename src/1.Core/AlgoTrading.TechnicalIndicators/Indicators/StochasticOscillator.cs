using AlgoTrading.TechnicalIndicators.Models;

namespace AlgoTrading.TechnicalIndicators.Indicators;

/// <summary>
/// Stochastic Oscillator indicator
/// Compares closing price to price range over a period
/// </summary>
public class StochasticOscillator : IIndicator<Candle, StochasticResult>
{
    private readonly int _kPeriod;
    private readonly int _dPeriod;
    private readonly int _smoothK;

    /// <summary>
    /// Create Stochastic Oscillator
    /// </summary>
    /// <param name="kPeriod">Period for %K calculation (typically 14)</param>
    /// <param name="dPeriod">Period for %D smoothing (typically 3)</param>
    /// <param name="smoothK">Smoothing period for %K (typically 3)</param>
    public StochasticOscillator(int kPeriod = 14, int dPeriod = 3, int smoothK = 3)
    {
        if (kPeriod <= 0)
            throw new ArgumentException("K period must be greater than 0", nameof(kPeriod));

        if (dPeriod <= 0)
            throw new ArgumentException("D period must be greater than 0", nameof(dPeriod));

        if (smoothK <= 0)
            throw new ArgumentException("Smooth K must be greater than 0", nameof(smoothK));

        _kPeriod = kPeriod;
        _dPeriod = dPeriod;
        _smoothK = smoothK;
    }

    public StochasticResult Calculate(IEnumerable<Candle> data)
    {
        var dataList = data.ToList();
        var minRequired = _kPeriod + _smoothK + _dPeriod - 2;

        if (dataList.Count < minRequired)
            throw new InvalidOperationException($"Insufficient data. Required: {minRequired}, Available: {dataList.Count}");

        // Calculate raw %K values
        var rawKValues = CalculateRawK(dataList);

        // Smooth %K values
        var smoothKValues = SmoothValues(rawKValues, _smoothK);

        // Calculate %D (SMA of smoothed %K)
        var kValue = smoothKValues.Last();
        var dValue = smoothKValues.TakeLast(_dPeriod).Average();

        return new StochasticResult(kValue, dValue);
    }

    /// <summary>
    /// Calculate raw %K values
    /// %K = 100 * (Close - LowestLow) / (HighestHigh - LowestLow)
    /// </summary>
    private List<decimal> CalculateRawK(List<Candle> data)
    {
        var rawK = new List<decimal>();

        for (int i = _kPeriod - 1; i < data.Count; i++)
        {
            var period = data.Skip(i - _kPeriod + 1).Take(_kPeriod).ToList();

            var lowestLow = period.Min(c => c.Low);
            var highestHigh = period.Max(c => c.High);
            var currentClose = data[i].Close;

            var range = highestHigh - lowestLow;

            if (range == 0)
            {
                rawK.Add(50); // Neutral if no price movement
            }
            else
            {
                var k = 100 * (currentClose - lowestLow) / range;
                rawK.Add(k);
            }
        }

        return rawK;
    }

    /// <summary>
    /// Smooth values using SMA
    /// </summary>
    private List<decimal> SmoothValues(List<decimal> values, int period)
    {
        var smoothed = new List<decimal>();

        for (int i = period - 1; i < values.Count; i++)
        {
            var avg = values.Skip(i - period + 1).Take(period).Average();
            smoothed.Add(avg);
        }

        return smoothed;
    }
}
