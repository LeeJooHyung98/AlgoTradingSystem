namespace AlgoTrading.Application.Services.MarketData;

/// <summary>
/// Interface for Market Data Provider (시장 데이터 제공자 인터페이스)
/// Provides historical and real-time market data for backtesting and live trading
/// </summary>
public interface IMarketDataProvider
{
    /// <summary>
    /// Get historical price data for a stock
    /// </summary>
    Task<IEnumerable<HistoricalPriceData>> GetHistoricalPricesAsync(
        string stockCode,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical price data for multiple stocks
    /// </summary>
    Task<Dictionary<string, IEnumerable<HistoricalPriceData>>> GetHistoricalPricesAsync(
        IEnumerable<string> stockCodes,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current real-time price for a stock
    /// </summary>
    Task<decimal?> GetCurrentPriceAsync(
        string stockCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current real-time prices for multiple stocks
    /// </summary>
    Task<Dictionary<string, decimal>> GetCurrentPricesAsync(
        IEnumerable<string> stockCodes,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Historical price data point
/// </summary>
public class HistoricalPriceData
{
    public string StockCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public decimal AdjustedClose { get; set; }

    public HistoricalPriceData()
    {
    }

    public HistoricalPriceData(
        string stockCode,
        DateTime date,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume,
        decimal? adjustedClose = null)
    {
        StockCode = stockCode;
        Date = date;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
        AdjustedClose = adjustedClose ?? close;
    }
}
