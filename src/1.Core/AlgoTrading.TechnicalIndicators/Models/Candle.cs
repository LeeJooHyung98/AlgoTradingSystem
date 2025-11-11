namespace AlgoTrading.TechnicalIndicators.Models;

/// <summary>
/// Represents OHLC (Open, High, Low, Close) candlestick data
/// </summary>
public record Candle
{
    public DateTime Timestamp { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public long Volume { get; init; }

    public Candle(DateTime timestamp, decimal open, decimal high, decimal low, decimal close, long volume = 0)
    {
        if (high < low)
            throw new ArgumentException("High price must be greater than or equal to low price");

        if (high < open || high < close)
            throw new ArgumentException("High price must be greater than or equal to open and close prices");

        if (low > open || low > close)
            throw new ArgumentException("Low price must be less than or equal to open and close prices");

        Timestamp = timestamp;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }

    /// <summary>
    /// Get typical price (High + Low + Close) / 3
    /// </summary>
    public decimal TypicalPrice => (High + Low + Close) / 3;

    /// <summary>
    /// Get weighted close price (High + Low + Close + Close) / 4
    /// </summary>
    public decimal WeightedClose => (High + Low + Close + Close) / 4;

    /// <summary>
    /// True Range: max(High - Low, |High - PrevClose|, |Low - PrevClose|)
    /// </summary>
    public decimal TrueRange(decimal previousClose)
    {
        var highLow = High - Low;
        var highClose = Math.Abs(High - previousClose);
        var lowClose = Math.Abs(Low - previousClose);

        return Math.Max(highLow, Math.Max(highClose, lowClose));
    }
}
