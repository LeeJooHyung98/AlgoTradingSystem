using AlgoTrading.Core.Entities.MarketData;
using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.Interfaces.External;

/// <summary>
/// description : 실시간 및 과거 시장 데이터를 제공하는 외부 서비스 인터페이스
/// Details : 실시간 가격, 과거 OHLC 데이터, 종목 정보, 기술적 지표 계산 등을 제공합니다.
///           데이터 구독/구독해제를 통해 실시간 스트리밍 데이터를 효율적으로 관리합니다.
/// Applied technology patterns : Adapter Pattern, Observer Pattern, Publish-Subscribe Pattern, Anti-Corruption Layer
/// </summary>
public interface IMarketDataService
{
    /// <summary>
    /// 실시간 가격 조회
    /// </summary>
    Task<PriceData> GetRealtimePriceAsync(string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 과거 가격 데이터 조회
    /// </summary>
    Task<IEnumerable<PriceData>> GetHistoricalPriceDataAsync(string stockCode, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// OHLC 캔들 데이터 조회
    /// </summary>
    Task<IEnumerable<Candle>> GetCandleDataAsync(string stockCode, TimeFrame timeFrame, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 종목 정보 조회
    /// </summary>
    Task<Stock> GetStockInfoAsync(string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 종목 목록 조회
    /// </summary>
    Task<IEnumerable<Stock>> GetStockListAsync(string market, CancellationToken cancellationToken = default);

    /// <summary>
    /// 실시간 데이터 구독
    /// </summary>
    Task SubscribeRealtimeDataAsync(string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 실시간 데이터 구독 해제
    /// </summary>
    Task UnsubscribeRealtimeDataAsync(string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 기술적 지표 계산
    /// </summary>
    Task<TechnicalIndicator> CalculateTechnicalIndicatorAsync(string stockCode, Enums.IndicatorType indicatorType, IDictionary<string, object> parameters, CancellationToken cancellationToken = default);
}
