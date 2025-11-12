using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.TechnicalIndicators.Indicators;
using AlgoTrading.TechnicalIndicators.Models;
using Microsoft.Extensions.Logging;
using Candle = AlgoTrading.TechnicalIndicators.Models.Candle;

namespace AlgoTrading.Application.Services.Strategy.Strategies;

/// <summary>
/// Moving Average Crossover Strategy
/// Buy when short MA crosses above long MA
/// Sell when short MA crosses below long MA
/// </summary>
public class MovingAverageCrossStrategy : ITradingStrategy
{
    private readonly ILogger<MovingAverageCrossStrategy> _logger;

    public string Name => "Moving Average Cross";
    public string Description => "Golden Cross / Death Cross strategy using two moving averages";
    public StrategyType StrategyType => StrategyType.Trend;

    public MovingAverageCrossStrategy(ILogger<MovingAverageCrossStrategy> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SignalResult> GenerateEntrySignalAsync(
        string stockCode,
        List<PriceData> priceData,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Placeholder for async operations

        if (priceData == null || priceData.Count < 2)
        {
            return new SignalResult { HasSignal = false, Reason = "Insufficient price data" };
        }

        // Get parameters
        var strategyParams = parameters ?? GetDefaultParameters();
        var shortPeriod = Convert.ToInt32(strategyParams["ShortPeriod"]);
        var longPeriod = Convert.ToInt32(strategyParams["LongPeriod"]);
        var signalStrengthThreshold = Convert.ToDecimal(strategyParams.GetValueOrDefault("SignalStrengthThreshold", 50m));

        // Need enough data for long MA
        if (priceData.Count < longPeriod + 1)
        {
            return new SignalResult
            {
                HasSignal = false,
                Reason = $"Need at least {longPeriod + 1} data points, got {priceData.Count}"
            };
        }

        // Convert to Candle format
        var candles = priceData
            .OrderBy(p => p.Timestamp)
            .Select(p => new Candle(
                p.Timestamp,
                p.OpenPrice,
                p.HighPrice,
                p.LowPrice,
                p.CurrentPrice,
                p.Volume))
            .ToList();

        // Calculate moving averages
        var closePrices = candles.Select(c => c.Close).ToList();
        var shortMAIndicator = new SimpleMovingAverage(shortPeriod);
        var longMAIndicator = new SimpleMovingAverage(longPeriod);

        var shortMA = new List<decimal>();
        var longMA = new List<decimal>();

        for (int i = shortPeriod - 1; i < closePrices.Count; i++)
        {
            shortMA.Add(shortMAIndicator.Calculate(closePrices.Take(i + 1)));
        }

        for (int i = longPeriod - 1; i < closePrices.Count; i++)
        {
            longMA.Add(longMAIndicator.Calculate(closePrices.Take(i + 1)));
        }

        // Check if we have enough calculated values
        if (shortMA.Count < 2 || longMA.Count < 2)
        {
            return new SignalResult { HasSignal = false, Reason = "Insufficient MA calculations" };
        }

        // Get current and previous values
        var currentShortMA = shortMA[^1];
        var prevShortMA = shortMA[^2];
        var currentLongMA = longMA[^1];
        var prevLongMA = longMA[^2];

        _logger.LogDebug("MA Cross Check - Stock: {Stock}, Short: {Short:F2} (prev: {PrevShort:F2}), Long: {Long:F2} (prev: {PrevLong:F2})",
            stockCode, currentShortMA, prevShortMA, currentLongMA, prevLongMA);

        // Golden Cross: Short MA crosses above Long MA
        var wasBelow = prevShortMA <= prevLongMA;
        var isAbove = currentShortMA > currentLongMA;
        var hasCrossover = wasBelow && isAbove;

        if (!hasCrossover)
        {
            return new SignalResult
            {
                HasSignal = false,
                Reason = $"No golden cross detected. Short MA: {currentShortMA:F2}, Long MA: {currentLongMA:F2}"
            };
        }

        // Calculate signal strength based on:
        // 1. Distance between MAs (larger distance = stronger signal)
        // 2. Recent price momentum
        var maDistance = ((currentShortMA - currentLongMA) / currentLongMA) * 100;
        var recentPrices = candles.TakeLast(5).ToList();
        var priceChange = ((recentPrices[^1].Close - recentPrices[0].Close) / recentPrices[0].Close) * 100;

        var signalStrength = Math.Min(100, 50 + (maDistance * 10) + (priceChange * 5));
        signalStrength = Math.Max(0, signalStrength);

        // Only generate signal if strength exceeds threshold
        if (signalStrength < signalStrengthThreshold)
        {
            return new SignalResult
            {
                HasSignal = false,
                Reason = $"Signal strength {signalStrength:F2}% below threshold {signalStrengthThreshold}%"
            };
        }

        var currentPrice = candles[^1].Close;
        var atr = CalculateATR(candles.TakeLast(14).ToList());

        return new SignalResult
        {
            HasSignal = true,
            SignalStrength = signalStrength,
            Reason = $"Golden Cross detected! Short MA ({currentShortMA:F2}) crossed above Long MA ({currentLongMA:F2})",
            SuggestedPrice = currentPrice,
            StopLoss = currentPrice - (atr * 2), // 2 ATR stop loss
            TakeProfit = currentPrice + (atr * 3), // 3 ATR take profit (1:1.5 risk/reward)
            Metadata = new Dictionary<string, object>
            {
                ["ShortMA"] = currentShortMA,
                ["LongMA"] = currentLongMA,
                ["MADistance"] = maDistance,
                ["PriceChange"] = priceChange,
                ["ATR"] = atr
            }
        };
    }

    public async Task<SignalResult> GenerateExitSignalAsync(
        string stockCode,
        List<PriceData> priceData,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (priceData == null || priceData.Count < 2)
        {
            return new SignalResult { HasSignal = false, Reason = "Insufficient price data" };
        }

        var strategyParams = parameters ?? GetDefaultParameters();
        var shortPeriod = Convert.ToInt32(strategyParams["ShortPeriod"]);
        var longPeriod = Convert.ToInt32(strategyParams["LongPeriod"]);

        if (priceData.Count < longPeriod + 1)
        {
            return new SignalResult { HasSignal = false, Reason = "Insufficient data for exit signal" };
        }

        var candles = priceData
            .OrderBy(p => p.Timestamp)
            .Select(p => new Candle(
                p.Timestamp,
                p.OpenPrice,
                p.HighPrice,
                p.LowPrice,
                p.CurrentPrice,
                p.Volume))
            .ToList();

        var closePrices = candles.Select(c => c.Close).ToList();
        var shortMAIndicator = new SimpleMovingAverage(shortPeriod);
        var longMAIndicator = new SimpleMovingAverage(longPeriod);

        var shortMA = new List<decimal>();
        var longMA = new List<decimal>();

        for (int i = shortPeriod - 1; i < closePrices.Count; i++)
        {
            shortMA.Add(shortMAIndicator.Calculate(closePrices.Take(i + 1)));
        }

        for (int i = longPeriod - 1; i < closePrices.Count; i++)
        {
            longMA.Add(longMAIndicator.Calculate(closePrices.Take(i + 1)));
        }

        if (shortMA.Count < 2 || longMA.Count < 2)
        {
            return new SignalResult { HasSignal = false, Reason = "Insufficient MA calculations" };
        }

        var currentShortMA = shortMA[^1];
        var prevShortMA = shortMA[^2];
        var currentLongMA = longMA[^1];
        var prevLongMA = longMA[^2];

        // Death Cross: Short MA crosses below Long MA
        var wasAbove = prevShortMA >= prevLongMA;
        var isBelow = currentShortMA < currentLongMA;
        var hasCrossunder = wasAbove && isBelow;

        if (!hasCrossunder)
        {
            return new SignalResult
            {
                HasSignal = false,
                Reason = $"No death cross detected. Short MA: {currentShortMA:F2}, Long MA: {currentLongMA:F2}"
            };
        }

        var maDistance = ((currentLongMA - currentShortMA) / currentLongMA) * 100;
        var signalStrength = Math.Min(100, 50 + (maDistance * 10));

        return new SignalResult
        {
            HasSignal = true,
            SignalStrength = signalStrength,
            Reason = $"Death Cross detected! Short MA ({currentShortMA:F2}) crossed below Long MA ({currentLongMA:F2})",
            SuggestedPrice = candles[^1].Close,
            Metadata = new Dictionary<string, object>
            {
                ["ShortMA"] = currentShortMA,
                ["LongMA"] = currentLongMA,
                ["MADistance"] = maDistance
            }
        };
    }

    public bool ValidateParameters(Dictionary<string, object> parameters)
    {
        if (!parameters.ContainsKey("ShortPeriod") || !parameters.ContainsKey("LongPeriod"))
            return false;

        var shortPeriod = Convert.ToInt32(parameters["ShortPeriod"]);
        var longPeriod = Convert.ToInt32(parameters["LongPeriod"]);

        // Short period must be less than long period
        if (shortPeriod >= longPeriod)
            return false;

        // Periods must be positive
        if (shortPeriod <= 0 || longPeriod <= 0)
            return false;

        // Reasonable ranges
        if (shortPeriod < 5 || shortPeriod > 50)
            return false;

        if (longPeriod < 10 || longPeriod > 200)
            return false;

        return true;
    }

    public Dictionary<string, object> GetDefaultParameters()
    {
        return new Dictionary<string, object>
        {
            ["ShortPeriod"] = 20,  // 20-day MA
            ["LongPeriod"] = 50,   // 50-day MA
            ["SignalStrengthThreshold"] = 50m  // Minimum signal strength to generate entry
        };
    }

    /// <summary>
    /// Calculate ATR for stop loss/take profit
    /// </summary>
    private decimal CalculateATR(List<Candle> candles, int period = 14)
    {
        if (candles.Count < 2)
            return 0;

        var trueRanges = new List<decimal>();

        for (int i = 1; i < candles.Count; i++)
        {
            var high = candles[i].High;
            var low = candles[i].Low;
            var prevClose = candles[i - 1].Close;

            var tr1 = high - low;
            var tr2 = Math.Abs(high - prevClose);
            var tr3 = Math.Abs(low - prevClose);

            var tr = Math.Max(tr1, Math.Max(tr2, tr3));
            trueRanges.Add(tr);
        }

        return trueRanges.TakeLast(Math.Min(period, trueRanges.Count)).Average();
    }
}
