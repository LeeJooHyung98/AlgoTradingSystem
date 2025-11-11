using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.TechnicalIndicators.Indicators;
using AlgoTrading.TechnicalIndicators.Models;
using Microsoft.Extensions.Logging;
using TechCandle = AlgoTrading.TechnicalIndicators.Models.Candle;

namespace AlgoTrading.Application.Services.Strategy;

/// <summary>
/// Service for calculating technical indicators from price data
/// Integrates TechnicalIndicators library with Application layer
/// </summary>
public class TechnicalIndicatorService : ITechnicalIndicatorService
{
    private readonly ILogger<TechnicalIndicatorService> _logger;

    public TechnicalIndicatorService(ILogger<TechnicalIndicatorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Convert PriceData entities to Candle models for indicator calculation
    /// </summary>
    public List<TechCandle> ConvertToCandles(IEnumerable<PriceData> priceData)
    {
        return priceData.Select(p => new TechCandle(
            timestamp: p.Timestamp,
            open: p.OpenPrice,
            high: p.HighPrice,
            low: p.LowPrice,
            close: p.CurrentPrice,
            volume: p.Volume
        )).ToList();
    }

    /// <summary>
    /// Calculate all available technical indicators for given price data
    /// </summary>
    public Dictionary<string, object> CalculateAllIndicators(List<PriceData> priceData)
    {
        var indicators = new Dictionary<string, object>();

        if (priceData.Count < 2)
        {
            _logger.LogWarning("Insufficient price data for indicator calculation");
            return indicators;
        }

        var closePrices = priceData.Select(p => p.CurrentPrice).ToList();
        var candles = ConvertToCandles(priceData);

        try
        {
            // Simple Moving Averages
            if (priceData.Count >= 20)
            {
                var sma20 = new SimpleMovingAverage(20);
                indicators["SMA20"] = sma20.Calculate(closePrices);
            }

            if (priceData.Count >= 50)
            {
                var sma50 = new SimpleMovingAverage(50);
                indicators["SMA50"] = sma50.Calculate(closePrices);
            }

            // Exponential Moving Averages
            if (priceData.Count >= 12)
            {
                var ema12 = new ExponentialMovingAverage(12);
                indicators["EMA12"] = ema12.Calculate(closePrices);
            }

            if (priceData.Count >= 26)
            {
                var ema26 = new ExponentialMovingAverage(26);
                indicators["EMA26"] = ema26.Calculate(closePrices);
            }

            // RSI (Relative Strength Index)
            if (priceData.Count >= 15)
            {
                var rsi = new RelativeStrengthIndex(14);
                indicators["RSI"] = rsi.Calculate(closePrices);
            }

            // MACD (Moving Average Convergence Divergence)
            if (priceData.Count >= 35)
            {
                var macd = new Macd(12, 26, 9);
                var macdResult = macd.Calculate(closePrices);
                indicators["MACD"] = macdResult.Macd;
                indicators["MACD_Signal"] = macdResult.Signal;
                indicators["MACD_Histogram"] = macdResult.Histogram;
            }

            // Bollinger Bands
            if (priceData.Count >= 20)
            {
                var bb = new BollingerBands(20, 2);
                var bbResult = bb.Calculate(closePrices);
                indicators["BB_Upper"] = bbResult.UpperBand;
                indicators["BB_Middle"] = bbResult.MiddleBand;
                indicators["BB_Lower"] = bbResult.LowerBand;
                indicators["BB_Width"] = bbResult.BandWidth;
                indicators["BB_PercentB"] = bbResult.PercentB;
            }

            // Stochastic Oscillator (requires OHLC data)
            if (candles.Count >= 20)
            {
                var stochastic = new StochasticOscillator(14, 3, 3);
                var stochResult = stochastic.Calculate(candles);
                indicators["Stochastic_K"] = stochResult.K;
                indicators["Stochastic_D"] = stochResult.D;
                indicators["Stochastic_Oversold"] = stochResult.IsOversold();
                indicators["Stochastic_Overbought"] = stochResult.IsOverbought();
            }

            // ATR (Average True Range)
            if (candles.Count >= 15)
            {
                var atr = new AverageTrueRange(14);
                var atrResult = atr.Calculate(candles);
                indicators["ATR"] = atrResult.Value;
            }

            // VWAP (Volume Weighted Average Price)
            if (candles.Count > 0)
            {
                var vwap = new VolumeWeightedAveragePrice();
                indicators["VWAP"] = vwap.Calculate(candles);
            }

            // Current price
            indicators["CurrentPrice"] = priceData.Last().CurrentPrice;

            _logger.LogDebug("Calculated {Count} technical indicators", indicators.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating technical indicators");
        }

        return indicators;
    }

    /// <summary>
    /// Calculate Bollinger Bands for price data
    /// </summary>
    public BollingerBandsResult? CalculateBollingerBands(List<PriceData> priceData, int period = 20, decimal standardDeviations = 2)
    {
        if (priceData.Count < period)
        {
            _logger.LogWarning("Insufficient data for Bollinger Bands calculation");
            return null;
        }

        try
        {
            var closePrices = priceData.Select(p => p.CurrentPrice);
            var bb = new BollingerBands(period, standardDeviations);
            return bb.Calculate(closePrices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Bollinger Bands");
            return null;
        }
    }

    /// <summary>
    /// Calculate Stochastic Oscillator for price data
    /// </summary>
    public StochasticResult? CalculateStochastic(List<PriceData> priceData, int kPeriod = 14, int dPeriod = 3, int smoothK = 3)
    {
        var minRequired = kPeriod + smoothK + dPeriod - 2;

        if (priceData.Count < minRequired)
        {
            _logger.LogWarning("Insufficient data for Stochastic calculation");
            return null;
        }

        try
        {
            var candles = ConvertToCandles(priceData);
            var stochastic = new StochasticOscillator(kPeriod, dPeriod, smoothK);
            return stochastic.Calculate(candles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Stochastic Oscillator");
            return null;
        }
    }

    /// <summary>
    /// Calculate Average True Range for price data
    /// </summary>
    public AtrResult? CalculateATR(List<PriceData> priceData, int period = 14)
    {
        if (priceData.Count < period + 1)
        {
            _logger.LogWarning("Insufficient data for ATR calculation");
            return null;
        }

        try
        {
            var candles = ConvertToCandles(priceData);
            var atr = new AverageTrueRange(period);
            return atr.Calculate(candles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ATR");
            return null;
        }
    }

    /// <summary>
    /// Calculate MACD for price data
    /// </summary>
    public MacdResult? CalculateMACD(List<PriceData> priceData, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
    {
        var minRequired = slowPeriod + signalPeriod;

        if (priceData.Count < minRequired)
        {
            _logger.LogWarning("Insufficient data for MACD calculation");
            return null;
        }

        try
        {
            var closePrices = priceData.Select(p => p.CurrentPrice);
            var macd = new Macd(fastPeriod, slowPeriod, signalPeriod);
            return macd.Calculate(closePrices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating MACD");
            return null;
        }
    }

    /// <summary>
    /// Calculate RSI for price data
    /// </summary>
    public decimal? CalculateRSI(List<PriceData> priceData, int period = 14)
    {
        if (priceData.Count < period + 1)
        {
            _logger.LogWarning("Insufficient data for RSI calculation");
            return null;
        }

        try
        {
            var closePrices = priceData.Select(p => p.CurrentPrice);
            var rsi = new RelativeStrengthIndex(period);
            return rsi.Calculate(closePrices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating RSI");
            return null;
        }
    }
}

/// <summary>
/// Interface for Technical Indicator Service
/// </summary>
public interface ITechnicalIndicatorService
{
    List<TechCandle> ConvertToCandles(IEnumerable<PriceData> priceData);
    Dictionary<string, object> CalculateAllIndicators(List<PriceData> priceData);
    BollingerBandsResult? CalculateBollingerBands(List<PriceData> priceData, int period = 20, decimal standardDeviations = 2);
    StochasticResult? CalculateStochastic(List<PriceData> priceData, int kPeriod = 14, int dPeriod = 3, int smoothK = 3);
    AtrResult? CalculateATR(List<PriceData> priceData, int period = 14);
    MacdResult? CalculateMACD(List<PriceData> priceData, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9);
    decimal? CalculateRSI(List<PriceData> priceData, int period = 14);
}
