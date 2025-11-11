namespace AlgoTrading.TechnicalIndicators.Indicators;

/// <summary>
/// Relative Strength Index (RSI) indicator
/// Momentum oscillator measuring speed and magnitude of price changes
/// </summary>
public class RelativeStrengthIndex : IIndicator<decimal, decimal>
{
    private readonly int _period;

    /// <summary>
    /// Create RSI indicator
    /// </summary>
    /// <param name="period">Period for RSI calculation (typically 14)</param>
    public RelativeStrengthIndex(int period = 14)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be greater than 0", nameof(period));

        _period = period;
    }

    public decimal Calculate(IEnumerable<decimal> data)
    {
        var dataList = data.ToList();

        if (dataList.Count < _period + 1)
            throw new InvalidOperationException($"Insufficient data. Required: {_period + 1}, Available: {dataList.Count}");

        // Calculate price changes
        var gains = new List<decimal>();
        var losses = new List<decimal>();

        for (int i = 1; i < dataList.Count; i++)
        {
            var change = dataList[i] - dataList[i - 1];

            if (change > 0)
            {
                gains.Add(change);
                losses.Add(0);
            }
            else
            {
                gains.Add(0);
                losses.Add(Math.Abs(change));
            }
        }

        // Calculate initial average gain and loss (SMA)
        var avgGain = gains.Take(_period).Average();
        var avgLoss = losses.Take(_period).Average();

        // Apply smoothing for remaining values (Wilder's smoothing)
        for (int i = _period; i < gains.Count; i++)
        {
            avgGain = ((avgGain * (_period - 1)) + gains[i]) / _period;
            avgLoss = ((avgLoss * (_period - 1)) + losses[i]) / _period;
        }

        // Calculate RSI
        if (avgLoss == 0)
            return 100; // All gains, RSI = 100

        var rs = avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));

        return rsi;
    }

    /// <summary>
    /// Calculate RSI series for all historical data
    /// </summary>
    public List<decimal> CalculateSeries(IEnumerable<decimal> data)
    {
        var dataList = data.ToList();
        var results = new List<decimal>();

        if (dataList.Count < _period + 1)
            return results;

        // Calculate price changes
        var gains = new List<decimal>();
        var losses = new List<decimal>();

        for (int i = 1; i < dataList.Count; i++)
        {
            var change = dataList[i] - dataList[i - 1];

            if (change > 0)
            {
                gains.Add(change);
                losses.Add(0);
            }
            else
            {
                gains.Add(0);
                losses.Add(Math.Abs(change));
            }
        }

        // Initial averages
        var avgGain = gains.Take(_period).Average();
        var avgLoss = losses.Take(_period).Average();

        // Calculate first RSI
        var rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));
        results.Add(rsi);

        // Calculate remaining RSI values
        for (int i = _period; i < gains.Count; i++)
        {
            avgGain = ((avgGain * (_period - 1)) + gains[i]) / _period;
            avgLoss = ((avgLoss * (_period - 1)) + losses[i]) / _period;

            rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
            rsi = 100 - (100 / (1 + rs));
            results.Add(rsi);
        }

        return results;
    }

    /// <summary>
    /// Check if RSI indicates oversold condition (typically below 30)
    /// </summary>
    public static bool IsOversold(decimal rsi, decimal threshold = 30) => rsi < threshold;

    /// <summary>
    /// Check if RSI indicates overbought condition (typically above 70)
    /// </summary>
    public static bool IsOverbought(decimal rsi, decimal threshold = 70) => rsi > threshold;
}
