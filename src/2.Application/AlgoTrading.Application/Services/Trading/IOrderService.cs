using AlgoTrading.Application.DTOs.Trading;
using ErrorOr;

namespace AlgoTrading.Application.Services.Trading;

/// <summary>
/// description(설명) : Order Service Interface (주문 서비스 인터페이스)
/// Details(상세설명) : Defines business operations for order management (주문 관리를 위한 비즈니스 작업 정의)
/// Applied technology patterns(적용기술패턴) : Service Pattern, DDD Application Service (서비스 패턴, DDD 애플리케이션 서비스)
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Submit order to broker for execution (증권사로 주문 제출하여 실행)
    /// </summary>
    Task<ErrorOr<bool>> SubmitOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process order execution from broker (증권사로부터 주문 체결 처리)
    /// </summary>
    Task<ErrorOr<bool>> ProcessOrderExecutionAsync(
        Guid orderId,
        int filledQuantity,
        decimal fillPrice,
        DateTime fillTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get order statistics for an account (계좌의 주문 통계 조회)
    /// </summary>
    Task<ErrorOr<OrderStatisticsDto>> GetOrderStatisticsAsync(
        string accountNumber,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Order Statistics DTO (주문 통계 DTO)
/// </summary>
public sealed record OrderStatisticsDto
{
    public int TotalOrders { get; init; }
    public int PendingOrders { get; init; }
    public int AcceptedOrders { get; init; }
    public int FilledOrders { get; init; }
    public int CancelledOrders { get; init; }
    public int RejectedOrders { get; init; }
    public decimal TotalOrderValue { get; init; }
    public decimal AverageFillRate { get; init; }
}
