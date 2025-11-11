using AlgoTrading.Application.Services.MarketData;
using AlgoTrading.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Infrastructure.Services.MarketData;

/// <summary>
/// description(설명) : Database Market Data Provider (데이터베이스 시장 데이터 제공자)
/// Details(상세설명) : Provides historical and real-time market data from database
/// Applied technology patterns(적용기술패턴) : Provider Pattern, Repository Pattern
/// </summary>
public class DatabaseMarketDataProvider : IMarketDataProvider
{
    private readonly ILogger<DatabaseMarketDataProvider> _logger;
    private readonly IPriceDataRepository _priceDataRepository;

    public DatabaseMarketDataProvider(
        ILogger<DatabaseMarketDataProvider> logger,
        IPriceDataRepository priceDataRepository)
    {
        _logger = logger;
        _priceDataRepository = priceDataRepository;
    }

    /// <summary>
    /// Get historical price data for a stock
    /// </summary>
    public async Task<IEnumerable<HistoricalPriceData>> GetHistoricalPricesAsync(
        string stockCode,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching historical prices for {StockCode} from {StartDate} to {EndDate}",
                stockCode, startDate, endDate);

            var priceData = await _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                stockCode,
                startDate,
                endDate,
                cancellationToken);

            var historicalData = priceData
                .OrderBy(p => p.Timestamp)
                .Select(p => new HistoricalPriceData
                {
                    StockCode = stockCode,
                    Date = p.Timestamp,
                    Open = p.OpenPrice,
                    High = p.HighPrice,
                    Low = p.LowPrice,
                    Close = p.CurrentPrice,
                    Volume = (long)p.Volume,
                    AdjustedClose = p.CurrentPrice
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} price data points for {StockCode}",
                historicalData.Count, stockCode);

            return historicalData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical prices for {StockCode}", stockCode);
            throw;
        }
    }

    /// <summary>
    /// Get historical price data for multiple stocks
    /// </summary>
    public async Task<Dictionary<string, IEnumerable<HistoricalPriceData>>> GetHistoricalPricesAsync(
        IEnumerable<string> stockCodes,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stockCodesList = stockCodes.ToList();
            _logger.LogDebug("Fetching historical prices for {Count} stocks from {StartDate} to {EndDate}",
                stockCodesList.Count, startDate, endDate);

            var result = new Dictionary<string, IEnumerable<HistoricalPriceData>>();

            // Fetch data for each stock
            foreach (var stockCode in stockCodesList)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var data = await GetHistoricalPricesAsync(
                    stockCode,
                    startDate,
                    endDate,
                    cancellationToken);

                result[stockCode] = data;
            }

            _logger.LogInformation("Retrieved historical prices for {Count} stocks", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical prices for multiple stocks");
            throw;
        }
    }

    /// <summary>
    /// Get current real-time price for a stock
    /// </summary>
    public async Task<decimal?> GetCurrentPriceAsync(
        string stockCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching current price for {StockCode}", stockCode);

            // Get the most recent price data
            var recentData = await _priceDataRepository.GetByStockCodeAndDateRangeAsync(
                stockCode,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow,
                cancellationToken);

            var latestPrice = recentData
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefault();

            if (latestPrice == null)
            {
                _logger.LogWarning("No current price found for {StockCode}", stockCode);
                return null;
            }

            _logger.LogDebug("Current price for {StockCode}: {Price}", stockCode, latestPrice.CurrentPrice);

            return latestPrice.CurrentPrice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current price for {StockCode}", stockCode);
            throw;
        }
    }

    /// <summary>
    /// Get current real-time prices for multiple stocks
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetCurrentPricesAsync(
        IEnumerable<string> stockCodes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stockCodesList = stockCodes.ToList();
            _logger.LogDebug("Fetching current prices for {Count} stocks", stockCodesList.Count);

            var result = new Dictionary<string, decimal>();

            foreach (var stockCode in stockCodesList)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var price = await GetCurrentPriceAsync(stockCode, cancellationToken);

                if (price.HasValue)
                {
                    result[stockCode] = price.Value;
                }
            }

            _logger.LogInformation("Retrieved current prices for {Count} stocks", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current prices for multiple stocks");
            throw;
        }
    }
}
