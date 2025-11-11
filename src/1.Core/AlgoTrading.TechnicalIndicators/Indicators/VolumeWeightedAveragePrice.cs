using AlgoTrading.TechnicalIndicators.Models;

namespace AlgoTrading.TechnicalIndicators.Indicators;

/// <summary>
/// Volume Weighted Average Price (VWAP) indicator
/// Shows average price weighted by volume
/// </summary>
public class VolumeWeightedAveragePrice : IIndicator<Candle, decimal>
{
    public decimal Calculate(IEnumerable<Candle> data)
    {
        var dataList = data.ToList();

        if (!dataList.Any())
            throw new InvalidOperationException("No data provided");

        decimal totalPriceVolume = 0;
        long totalVolume = 0;

        foreach (var candle in dataList)
        {
            // Use typical price for VWAP calculation
            var typicalPrice = candle.TypicalPrice;
            totalPriceVolume += typicalPrice * candle.Volume;
            totalVolume += candle.Volume;
        }

        if (totalVolume == 0)
            return dataList.Last().Close; // Fallback to last close if no volume

        return totalPriceVolume / totalVolume;
    }

    /// <summary>
    /// Calculate intraday VWAP (resets at market open)
    /// </summary>
    public decimal CalculateIntraday(IEnumerable<Candle> data, DateTime tradingDay)
    {
        var intradayData = data.Where(c => c.Timestamp.Date == tradingDay.Date).ToList();
        return Calculate(intradayData);
    }
}
