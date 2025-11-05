using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Enums;

namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : 주문(Order) 엔티티에 대한 데이터 액세스를 담당하는 리포지토리 인터페이스
/// Details : 주문 생성, 조회, 수정, 삭제 기능과 함께 계좌별, 종목별, 상태별 조회 등
///           주문 도메인에 특화된 쿼리 메서드를 제공합니다. CQRS 패턴의 Command 측면을 지원합니다.
/// Applied technology patterns : Repository Pattern, CQRS Pattern, Specification Pattern, Domain-Driven Design
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// 계좌번호로 주문 목록 조회
    /// </summary>
    Task<IEnumerable<Order>> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// 종목코드로 주문 목록 조회
    /// </summary>
    Task<IEnumerable<Order>> GetByStockCodeAsync(string stockCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// 주문 상태로 주문 목록 조회
    /// </summary>
    Task<IEnumerable<Order>> GetByStatusAsync(Enums.OrderStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 전략 ID로 주문 목록 조회
    /// </summary>
    Task<IEnumerable<Order>> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 날짜 범위로 주문 목록 조회
    /// </summary>
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// 미체결 주문 목록 조회
    /// </summary>
    Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 오늘의 주문 목록 조회
    /// </summary>
    Task<IEnumerable<Order>> GetTodaysOrdersAsync(CancellationToken cancellationToken = default);
}
