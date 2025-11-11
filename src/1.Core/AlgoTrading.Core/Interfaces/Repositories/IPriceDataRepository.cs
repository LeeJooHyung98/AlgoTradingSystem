using AlgoTrading.Core.Entities.MarketData;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : PriceData 엔티티(시계열 가격 데이터)에 대한 데이터 접근 인터페이스
/// Details : Market Data Context에서 OHLC(시가/고가/저가/종가) 데이터와 거래량을 시계열로 저장하고 조회합니다. TimescaleDB의 Hypertable을 활용하여 대용량 시계열 데이터를 효율적으로 파티셔닝하고 인덱싱합니다. 종목별 최신 가격 조회, 날짜 범위 조회, 복수 종목 일괄 조회를 지원하여 기술적 지표 계산과 백테스팅에 필요한 데이터를 제공합니다. Strategy Context의 신호 생성 시 이 데이터를 기반으로 이동평균, RSI, MACD 등의 지표를 계산합니다.
/// Applied technology patterns : Repository Pattern, Time Series Data Pattern, Hypertable Pattern (TimescaleDB)
/// </summary>
public interface IPriceDataRepository : IRepository<PriceData, PriceDataId>
{
    /// <summary>
    /// 종목 코드로 최신 가격 데이터 조회
    /// </summary>
    Task<PriceData?> GetLatestByStockCodeAsync(string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 종목 코드와 날짜 범위로 가격 데이터 목록 조회
    /// </summary>
    Task<IEnumerable<PriceData>> GetByStockCodeAndDateRangeAsync(string stockCode, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 종목 코드와 특정 날짜로 가격 데이터 조회
    /// </summary>
    Task<PriceData?> GetByStockCodeAndDateAsync(string stockCode, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// 여러 종목 코드의 최신 가격 데이터 조회
    /// </summary>
    Task<IEnumerable<PriceData>> GetLatestByStockCodesAsync(IEnumerable<string> stockCodes, CancellationToken cancellationToken = default);
}
