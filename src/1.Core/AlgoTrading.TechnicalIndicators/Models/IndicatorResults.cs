namespace AlgoTrading.TechnicalIndicators.Models;

/// <summary>
/// Result of Bollinger Bands calculation
/// </summary>
public record BollingerBandsResult
{
    public decimal UpperBand { get; init; }
    public decimal MiddleBand { get; init; }
    public decimal LowerBand { get; init; }
    public decimal BandWidth { get; init; }
    public decimal PercentB { get; init; }

    public BollingerBandsResult(decimal upper, decimal middle, decimal lower, decimal currentPrice)
    {
        UpperBand = upper;
        MiddleBand = middle;
        LowerBand = lower;
        BandWidth = upper - lower;

        // %B: Position within bands (0 = lower band, 1 = upper band)
        PercentB = BandWidth > 0 ? (currentPrice - lower) / BandWidth : 0.5m;
    }
}

/// <summary>
/// Result of Stochastic Oscillator calculation
/// </summary>
public record StochasticResult
{
    public decimal K { get; init; } // %K line
    public decimal D { get; init; } // %D line (signal line)

    public StochasticResult(decimal k, decimal d)
    {
        K = k;
        D = d;
    }

    /// <summary>
    /// Check if oversold (typically below 20)
    /// </summary>
    public bool IsOversold(decimal threshold = 20) => K < threshold && D < threshold;

    /// <summary>
    /// Check if overbought (typically above 80)
    /// </summary>
    public bool IsOverbought(decimal threshold = 80) => K > threshold && D > threshold;

    /// <summary>
    /// Check for bullish crossover (K crosses above D)
    /// </summary>
    public bool IsBullishCrossover(StochasticResult previous) => K > D && previous.K <= previous.D;

    /// <summary>
    /// Check for bearish crossover (K crosses below D)
    /// </summary>
    public bool IsBearishCrossover(StochasticResult previous) => K < D && previous.K >= previous.D;
}

/// <summary>
/// Result of MACD (Moving Average Convergence Divergence) calculation
/// </summary>
public record MacdResult
{
    public decimal Macd { get; init; }
    public decimal Signal { get; init; }
    public decimal Histogram { get; init; }

    public MacdResult(decimal macd, decimal signal)
    {
        Macd = macd;
        Signal = signal;
        Histogram = macd - signal;
    }

    /// <summary>
    /// Check for bullish crossover (MACD crosses above Signal)
    /// </summary>
    public bool IsBullishCrossover(MacdResult previous) => Macd > Signal && previous.Macd <= previous.Signal;

    /// <summary>
    /// Check for bearish crossover (MACD crosses below Signal)
    /// </summary>
    public bool IsBearishCrossover(MacdResult previous) => Macd < Signal && previous.Macd >= previous.Signal;
}

/// <summary>
/// Result of Average True Range (ATR) calculation
/// </summary>
public record AtrResult
{
    public decimal Value { get; init; }

    public AtrResult(decimal value)
    {
        Value = value;
    }

    /// <summary>
    /// Calculate position size based on ATR and risk percentage
    /// </summary>
    public int CalculatePositionSize(decimal capital, decimal riskPercent, decimal stopLossMultiplier = 2)
    {
        if (Value <= 0) return 0;

        var riskAmount = capital * (riskPercent / 100);
        var stopLossDistance = Value * stopLossMultiplier;

        return (int)(riskAmount / stopLossDistance);
    }
}
