using AlgoTrading.TechnicalIndicators.Models;

namespace AlgoTrading.TechnicalIndicators.Indicators;

/// <summary>
/// Average True Range (ATR) indicator
/// Measures market volatility by decomposing the entire range of an asset price
/// </summary>
public class AverageTrueRange : IIndicator<Candle, AtrResult>
{
    private readonly int _period;

    /// <summary>
    /// Create ATR indicator
    /// </summary>
    /// <param name="period">Period for ATR calculation (typically 14)</param>
    public AverageTrueRange(int period = 14)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be greater than 0", nameof(period));

        _period = period;
    }

    public AtrResult Calculate(IEnumerable<Candle> data)
    {
        var dataList = data.ToList();

        if (dataList.Count < _period + 1)
            throw new InvalidOperationException($"Insufficient data. Required: {_period + 1}, Available: {dataList.Count}");

        // Calculate True Range for each candle
        var trueRanges = new List<decimal>();

        for (int i = 1; i < dataList.Count; i++)
        {
            var tr = dataList[i].TrueRange(dataList[i - 1].Close);
            trueRanges.Add(tr);
        }

        // First ATR is simple average of True Ranges
        var atr = trueRanges.Take(_period).Average();

        // Subsequent ATR values use smoothing: ATR = ((Prior ATR * (n-1)) + Current TR) / n
        for (int i = _period; i < trueRanges.Count; i++)
        {
            atr = ((atr * (_period - 1)) + trueRanges[i]) / _period;
        }

        return new AtrResult(atr);
    }

    /// <summary>
    /// Calculate ATR series for all historical data
    /// </summary>
    public List<AtrResult> CalculateSeries(IEnumerable<Candle> data)
    {
        var dataList = data.ToList();
        var results = new List<AtrResult>();

        if (dataList.Count < _period + 1)
            return results;

        // Calculate True Range for each candle
        var trueRanges = new List<decimal>();

        for (int i = 1; i < dataList.Count; i++)
        {
            var tr = dataList[i].TrueRange(dataList[i - 1].Close);
            trueRanges.Add(tr);
        }

        // First ATR
        var atr = trueRanges.Take(_period).Average();
        results.Add(new AtrResult(atr));

        // Calculate remaining ATR values
        for (int i = _period; i < trueRanges.Count; i++)
        {
            atr = ((atr * (_period - 1)) + trueRanges[i]) / _period;
            results.Add(new AtrResult(atr));
        }

        return results;
    }
}
