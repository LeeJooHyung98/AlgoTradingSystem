using AlgoTrading.Application.Services.MarketData;

namespace AlgoTrading.Application.Services.Strategy.Evaluation;

/// <summary>
/// description(설명) : Technical Indicator Calculator (기술적 지표 계산기)
/// Details(상세설명) : Calculates technical indicators from price data for rule evaluation
/// Applied technology patterns(적용기술패턴) : Calculator Pattern, Technical Analysis
/// </summary>
public class TechnicalIndicatorCalculator
{
    /// <summary>
    /// Calculate Simple Moving Average (SMA)
    /// </summary>
    public decimal CalculateSMA(List<HistoricalPriceData> data, int period)
    {
        if (data.Count < period)
            return 0;

        var prices = data.TakeLast(period).Select(d => d.Close).ToList();
        return prices.Average();
    }

    /// <summary>
    /// Calculate Exponential Moving Average (EMA)
    /// </summary>
    public decimal CalculateEMA(List<HistoricalPriceData> data, int period)
    {
        if (data.Count < period)
            return 0;

        var multiplier = 2.0m / (period + 1);
        var prices = data.Select(d => d.Close).ToList();

        // Start with SMA for first EMA value
        var ema = prices.Take(period).Average();

        // Calculate EMA for remaining values
        for (int i = period; i < prices.Count; i++)
        {
            ema = (prices[i] - ema) * multiplier + ema;
        }

        return ema;
    }

    /// <summary>
    /// Calculate Relative Strength Index (RSI)
    /// </summary>
    public decimal CalculateRSI(List<HistoricalPriceData> data, int period)
    {
        if (data.Count < period + 1)
            return 50; // Neutral

        var prices = data.Select(d => d.Close).ToList();
        var gains = new List<decimal>();
        var losses = new List<decimal>();

        // Calculate price changes
        for (int i = 1; i < prices.Count; i++)
        {
            var change = prices[i] - prices[i - 1];
            gains.Add(change > 0 ? change : 0);
            losses.Add(change < 0 ? -change : 0);
        }

        // Calculate average gain and loss
        var avgGain = gains.TakeLast(period).Average();
        var avgLoss = losses.TakeLast(period).Average();

        if (avgLoss == 0)
            return 100;

        var rs = avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));

        return rsi;
    }

    /// <summary>
    /// Calculate MACD (Moving Average Convergence Divergence)
    /// Returns (MACD, Signal, Histogram)
    /// </summary>
    public (decimal macd, decimal signal, decimal histogram) CalculateMACD(
        List<HistoricalPriceData> data,
        int fastPeriod = 12,
        int slowPeriod = 26,
        int signalPeriod = 9)
    {
        if (data.Count < slowPeriod + signalPeriod)
            return (0, 0, 0);

        var fastEMA = CalculateEMA(data, fastPeriod);
        var slowEMA = CalculateEMA(data, slowPeriod);
        var macd = fastEMA - slowEMA;

        // Calculate signal line (EMA of MACD)
        // For simplicity, we'll approximate with recent MACD values
        var signal = macd * 0.8m; // Simplified - in production, track MACD history
        var histogram = macd - signal;

        return (macd, signal, histogram);
    }

    /// <summary>
    /// Calculate Bollinger Bands
    /// Returns (Upper Band, Middle Band, Lower Band)
    /// </summary>
    public (decimal upper, decimal middle, decimal lower) CalculateBollingerBands(
        List<HistoricalPriceData> data,
        int period = 20,
        decimal stdDevMultiplier = 2)
    {
        if (data.Count < period)
            return (0, 0, 0);

        var prices = data.TakeLast(period).Select(d => d.Close).ToList();
        var sma = prices.Average();

        // Calculate standard deviation
        var variance = prices.Sum(p => (p - sma) * (p - sma)) / period;
        var stdDev = (decimal)Math.Sqrt((double)variance);

        var upper = sma + (stdDev * stdDevMultiplier);
        var lower = sma - (stdDev * stdDevMultiplier);

        return (upper, sma, lower);
    }

    /// <summary>
    /// Calculate Average Volume
    /// </summary>
    public decimal CalculateAverageVolume(List<HistoricalPriceData> data, int period)
    {
        if (data.Count < period)
            return 0;

        var volumes = data.TakeLast(period).Select(d => (decimal)d.Volume).ToList();
        return volumes.Average();
    }

    /// <summary>
    /// Calculate Stochastic Oscillator
    /// Returns (%K, %D)
    /// </summary>
    public (decimal k, decimal d) CalculateStochastic(
        List<HistoricalPriceData> data,
        int kPeriod = 14,
        int dPeriod = 3)
    {
        if (data.Count < kPeriod)
            return (50, 50);

        var recentData = data.TakeLast(kPeriod).ToList();
        var currentClose = recentData.Last().Close;
        var highest = recentData.Max(d => d.High);
        var lowest = recentData.Min(d => d.Low);

        var k = highest - lowest != 0
            ? ((currentClose - lowest) / (highest - lowest)) * 100
            : 50;

        // %D is SMA of %K (simplified - should track %K history)
        var d = k; // Simplified

        return (k, d);
    }

    /// <summary>
    /// Calculate Average True Range (ATR)
    /// </summary>
    public decimal CalculateATR(List<HistoricalPriceData> data, int period = 14)
    {
        if (data.Count < period + 1)
            return 0;

        var trueRanges = new List<decimal>();

        for (int i = 1; i < data.Count; i++)
        {
            var high = data[i].High;
            var low = data[i].Low;
            var prevClose = data[i - 1].Close;

            var tr1 = high - low;
            var tr2 = Math.Abs(high - prevClose);
            var tr3 = Math.Abs(low - prevClose);

            var trueRange = Math.Max(tr1, Math.Max(tr2, tr3));
            trueRanges.Add(trueRange);
        }

        return trueRanges.TakeLast(period).Average();
    }

    /// <summary>
    /// Calculate Rate of Change (ROC)
    /// </summary>
    public decimal CalculateROC(List<HistoricalPriceData> data, int period = 12)
    {
        if (data.Count < period + 1)
            return 0;

        var currentPrice = data.Last().Close;
        var pastPrice = data[^(period + 1)].Close;

        if (pastPrice == 0)
            return 0;

        return ((currentPrice - pastPrice) / pastPrice) * 100;
    }

    /// <summary>
    /// Calculate Commodity Channel Index (CCI)
    /// </summary>
    public decimal CalculateCCI(List<HistoricalPriceData> data, int period = 20)
    {
        if (data.Count < period)
            return 0;

        var recentData = data.TakeLast(period).ToList();

        // Calculate typical price
        var typicalPrices = recentData
            .Select(d => (d.High + d.Low + d.Close) / 3)
            .ToList();

        var sma = typicalPrices.Average();
        var meanDeviation = typicalPrices
            .Sum(tp => Math.Abs(tp - sma)) / period;

        if (meanDeviation == 0)
            return 0;

        var currentTypicalPrice = typicalPrices.Last();
        var cci = (currentTypicalPrice - sma) / (0.015m * meanDeviation);

        return cci;
    }

    /// <summary>
    /// Calculate Money Flow Index (MFI)
    /// </summary>
    public decimal CalculateMFI(List<HistoricalPriceData> data, int period = 14)
    {
        if (data.Count < period + 1)
            return 50;

        var recentData = data.TakeLast(period + 1).ToList();
        var positiveFlow = 0m;
        var negativeFlow = 0m;

        for (int i = 1; i < recentData.Count; i++)
        {
            var typicalPrice = (recentData[i].High + recentData[i].Low + recentData[i].Close) / 3;
            var prevTypicalPrice = (recentData[i - 1].High + recentData[i - 1].Low + recentData[i - 1].Close) / 3;
            var moneyFlow = typicalPrice * recentData[i].Volume;

            if (typicalPrice > prevTypicalPrice)
                positiveFlow += moneyFlow;
            else if (typicalPrice < prevTypicalPrice)
                negativeFlow += moneyFlow;
        }

        if (negativeFlow == 0)
            return 100;

        var moneyRatio = positiveFlow / negativeFlow;
        var mfi = 100 - (100 / (1 + moneyRatio));

        return mfi;
    }

    /// <summary>
    /// Check if golden cross occurred (short MA crosses above long MA)
    /// </summary>
    public bool IsGoldenCross(List<HistoricalPriceData> data, int shortPeriod = 50, int longPeriod = 200)
    {
        if (data.Count < longPeriod + 1)
            return false;

        var currentShortMA = CalculateSMA(data, shortPeriod);
        var currentLongMA = CalculateSMA(data, longPeriod);

        var prevShortMA = CalculateSMA(data.Take(data.Count - 1).ToList(), shortPeriod);
        var prevLongMA = CalculateSMA(data.Take(data.Count - 1).ToList(), longPeriod);

        return prevShortMA <= prevLongMA && currentShortMA > currentLongMA;
    }

    /// <summary>
    /// Check if death cross occurred (short MA crosses below long MA)
    /// </summary>
    public bool IsDeathCross(List<HistoricalPriceData> data, int shortPeriod = 50, int longPeriod = 200)
    {
        if (data.Count < longPeriod + 1)
            return false;

        var currentShortMA = CalculateSMA(data, shortPeriod);
        var currentLongMA = CalculateSMA(data, longPeriod);

        var prevShortMA = CalculateSMA(data.Take(data.Count - 1).ToList(), shortPeriod);
        var prevLongMA = CalculateSMA(data.Take(data.Count - 1).ToList(), longPeriod);

        return prevShortMA >= prevLongMA && currentShortMA < currentLongMA;
    }

    /// <summary>
    /// Calculate Ichimoku Cloud components
    /// Returns (Tenkan, Kijun, SenkouA, SenkouB, Chikou)
    /// </summary>
    public (decimal tenkan, decimal kijun, decimal senkouA, decimal senkouB, decimal chikou) CalculateIchimoku(
        List<HistoricalPriceData> data)
    {
        if (data.Count < 52)
            return (0, 0, 0, 0, 0);

        // Tenkan-sen (Conversion Line): (9-period high + 9-period low) / 2
        var tenkanHigh = data.TakeLast(9).Max(d => d.High);
        var tenkanLow = data.TakeLast(9).Min(d => d.Low);
        var tenkan = (tenkanHigh + tenkanLow) / 2;

        // Kijun-sen (Base Line): (26-period high + 26-period low) / 2
        var kijunHigh = data.TakeLast(26).Max(d => d.High);
        var kijunLow = data.TakeLast(26).Min(d => d.Low);
        var kijun = (kijunHigh + kijunLow) / 2;

        // Senkou Span A: (Tenkan + Kijun) / 2
        var senkouA = (tenkan + kijun) / 2;

        // Senkou Span B: (52-period high + 52-period low) / 2
        var senkouBHigh = data.TakeLast(52).Max(d => d.High);
        var senkouBLow = data.TakeLast(52).Min(d => d.Low);
        var senkouB = (senkouBHigh + senkouBLow) / 2;

        // Chikou Span: Current close price
        var chikou = data.Last().Close;

        return (tenkan, kijun, senkouA, senkouB, chikou);
    }
}
