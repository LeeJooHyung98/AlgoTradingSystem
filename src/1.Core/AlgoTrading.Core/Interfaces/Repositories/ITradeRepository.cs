using AlgoTrading.Core.Entities.Trading;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : Trade 엔티티(체결된 거래)에 대한 데이터 접근 인터페이스
/// Details : Trading Context에서 완료된 거래(Trade)를 저장하고 조회합니다. Position이 청산될 때 Trade 레코드가 생성되며, 진입가, 청산가, 수익/손실, 수수료 등의 정보를 포함합니다. 날짜 범위, 종목, 전략, 포트폴리오별 거래 내역을 조회하여 성과 분석에 사용됩니다. 수익성 거래와 손실 거래를 분리 조회하여 승률과 손익비 계산을 지원합니다. Performance Calculation Service는 이 데이터를 기반으로 Sharpe Ratio, Max Drawdown 등의 지표를 계산합니다.
/// Applied technology patterns : Repository Pattern, Historical Data Pattern, Audit Trail Pattern
/// </summary>
public interface ITradeRepository : IRepository<Trade>
{
    /// <summary>
    /// 계좌번호로 거래 목록 조회
    /// </summary>
    Task<IEnumerable<Trade>> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 종목코드로 거래 목록 조회
    /// </summary>
    Task<IEnumerable<Trade>> GetByStockCodeAsync(string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 날짜 범위로 거래 목록 조회
    /// </summary>
    Task<IEnumerable<Trade>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 ID로 거래 목록 조회
    /// </summary>
    Task<IEnumerable<Trade>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 포트폴리오 ID로 거래 목록 조회
    /// </summary>
    Task<IEnumerable<Trade>> GetByPortfolioIdAsync(Guid portfolioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 수익성 거래 목록 조회
    /// </summary>
    Task<IEnumerable<Trade>> GetProfitableTradesAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 손실 거래 목록 조회
    /// </summary>
    Task<IEnumerable<Trade>> GetLosingTradesAsync(string accountNumber, CancellationToken cancellationToken = default);
}
