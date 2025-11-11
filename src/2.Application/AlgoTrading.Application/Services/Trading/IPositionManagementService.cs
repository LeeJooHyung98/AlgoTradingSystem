using AlgoTrading.Application.DTOs.Trading;
using ErrorOr;

namespace AlgoTrading.Application.Services.Trading;

/// <summary>
/// description(설명) : Position Management Service Interface (포지션 관리 서비스 인터페이스)
/// Details(상세설명) : Defines business operations for position management (포지션 관리를 위한 비즈니스 작업 정의)
/// Applied technology patterns(적용기술패턴) : Service Pattern, DDD Application Service (서비스 패턴, DDD 애플리케이션 서비스)
/// </summary>
public interface IPositionManagementService
{
    /// <summary>
    /// Update position prices with current market data (현재 시장 데이터로 포지션 가격 업데이트)
    /// </summary>
    Task<ErrorOr<bool>> UpdatePositionPricesAsync(
        string accountNumber,
        Dictionary<string, decimal> currentPrices,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get position summary for an account (계좌의 포지션 요약 조회)
    /// </summary>
    Task<ErrorOr<PositionSummaryDto>> GetPositionSummaryAsync(
        string accountNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate total portfolio value (전체 포트폴리오 가치 계산)
    /// </summary>
    Task<ErrorOr<decimal>> CalculatePortfolioValueAsync(
        string accountNumber,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Position Summary DTO (포지션 요약 DTO)
/// </summary>
public sealed record PositionSummaryDto
{
    public int TotalPositions { get; init; }
    public decimal TotalCostBasis { get; init; }
    public decimal TotalMarketValue { get; init; }
    public decimal TotalUnrealizedPL { get; init; }
    public decimal TotalUnrealizedPLPercent { get; init; }
    public int ProfitablePositions { get; init; }
    public int LosingPositions { get; init; }
    public decimal LargestGain { get; init; }
    public decimal LargestLoss { get; init; }
}
