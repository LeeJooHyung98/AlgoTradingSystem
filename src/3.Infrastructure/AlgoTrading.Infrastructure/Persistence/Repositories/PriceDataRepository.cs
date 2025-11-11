using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using StockCodeVO = AlgoTrading.Core.ValueObjects.StockCode;

namespace AlgoTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// description : PriceData repository implementation
/// Details : Provides data access for PriceData entity with TimescaleDB optimization
/// Applied technology patterns : Repository Pattern, Time Series Data Pattern
/// </summary>
public class PriceDataRepository : Repository<PriceData, PriceDataId>, IPriceDataRepository
{
    public PriceDataRepository(AlgoTradingDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 종목 코드로 최신 가격 데이터 조회
    /// </summary>
    public async Task<PriceData?> GetLatestByStockCodeAsync(
        string stockCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StockCode == new StockCodeVO(stockCode))
            .OrderByDescending(p => p.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 종목 코드와 날짜 범위로 가격 데이터 목록 조회
    /// </summary>
    public async Task<IEnumerable<PriceData>> GetByStockCodeAndDateRangeAsync(
        string stockCode,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StockCode == new StockCodeVO(stockCode)
                     && p.Timestamp >= startDate
                     && p.Timestamp <= endDate)
            .OrderBy(p => p.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 종목 코드와 특정 날짜로 가격 데이터 조회
    /// </summary>
    public async Task<PriceData?> GetByStockCodeAndDateAsync(
        string stockCode,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        // Get data for the entire day
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);

        return await _dbSet
            .Where(p => p.StockCode == new StockCodeVO(stockCode)
                     && p.Timestamp >= startOfDay
                     && p.Timestamp <= endOfDay)
            .OrderByDescending(p => p.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 여러 종목 코드의 최신 가격 데이터 조회
    /// </summary>
    public async Task<IEnumerable<PriceData>> GetLatestByStockCodesAsync(
        IEnumerable<string> stockCodes,
        CancellationToken cancellationToken = default)
    {
        var stockCodesList = stockCodes.ToList();

        // Convert string codes to StockCode value objects
        var stockCodeValueObjects = stockCodesList.Select(code => new StockCodeVO(code)).ToList();

        // Get the latest record for each stock code
        var result = new List<PriceData>();

        foreach (var stockCode in stockCodeValueObjects)
        {
            var latestPrice = await _dbSet
                .Where(p => p.StockCode == stockCode)
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestPrice != null)
            {
                result.Add(latestPrice);
            }
        }

        return result;
    }
}
