using AlgoTrading.Core.Entities.MarketData;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : Stock 엔티티(거래 가능 종목 정보)에 대한 데이터 접근 인터페이스
/// Details : Market Data Context의 Stock 엔티티를 관리하며, 종목 코드, 이름, 시장(KOSPI/KOSDAQ), 섹터, 상장 상태 등의 메타데이터를 제공합니다. 종목 코드는 유니크 키로 사용되며, 시장과 섹터별 필터링을 지원하여 전략의 유니버스 선정에 활용됩니다. GetByCodesAsync는 대량 종목 조회를 위한 일괄 처리 메서드로, IN 절을 사용하여 쿼리 최적화를 수행합니다. Market Data Service는 Kiwoom OpenAPI에서 받은 실시간 데이터를 이 Stock 엔티티와 매핑하여 PriceData를 생성합니다.
/// Applied technology patterns : Repository Pattern, Reference Data Pattern, Batch Query Pattern
/// </summary>
public interface IStockRepository : IRepository<Stock>
{
    /// <summary>
    /// 종목 코드로 종목 조회
    /// </summary>
    Task<Stock?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// 종목 이름으로 종목 목록 조회
    /// </summary>
    Task<IEnumerable<Stock>> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 시장으로 종목 목록 조회
    /// </summary>
    Task<IEnumerable<Stock>> GetByMarketAsync(string market, CancellationToken cancellationToken = default);

    /// <summary>
    /// 섹터로 종목 목록 조회
    /// </summary>
    Task<IEnumerable<Stock>> GetBySectorAsync(string sector, CancellationToken cancellationToken = default);

    /// <summary>
    /// 활성화된 종목 목록 조회
    /// </summary>
    Task<IEnumerable<Stock>> GetActiveStocksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 종목 코드 목록으로 종목 목록 조회
    /// </summary>
    Task<IEnumerable<Stock>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default);
}
