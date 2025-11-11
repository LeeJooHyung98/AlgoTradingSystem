using AlgoTrading.Application.DTOs.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using Microsoft.Extensions.Logging;
using IUnitOfWork = AlgoTrading.Core.Interfaces.IUnitOfWork;

namespace AlgoTrading.Application.Services.Trading;

/// <summary>
/// description(설명) : Position Management Service Implementation (포지션 관리 서비스 구현)
/// Details(상세설명) : Implements business logic for position management (포지션 관리 비즈니스 로직 구현)
/// Applied technology patterns(적용기술패턴) : Service Pattern, DDD Application Service (서비스 패턴, DDD 애플리케이션 서비스)
/// </summary>
public sealed class PositionManagementService : IPositionManagementService
{
    private readonly IPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PositionManagementService> _logger;

    public PositionManagementService(
        IPositionRepository positionRepository,
        IUnitOfWork unitOfWork,
        ILogger<PositionManagementService> logger)
    {
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ErrorOr<bool>> UpdatePositionPricesAsync(
        string accountNumber,
        Dictionary<string, decimal> currentPrices,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var positions = await _positionRepository.GetOpenPositionsAsync(accountNumber, cancellationToken);

            foreach (var position in positions)
            {
                if (currentPrices.TryGetValue(position.StockCode.Value, out var currentPrice))
                {
                    position.UpdatePrice(currentPrice);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated {Count} position prices for account {AccountNumber}",
                positions.Count(), accountNumber);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update position prices: AccountNumber={AccountNumber}", accountNumber);
            return Error.Failure("Position.PriceUpdateFailed", "An error occurred while updating position prices");
        }
    }

    public async Task<ErrorOr<PositionSummaryDto>> GetPositionSummaryAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var positions = await _positionRepository.GetOpenPositionsAsync(accountNumber, cancellationToken);
            var positionsList = positions.ToList();

            if (!positionsList.Any())
            {
                return new PositionSummaryDto();
            }

            var summary = new PositionSummaryDto
            {
                TotalPositions = positionsList.Count,
                TotalCostBasis = positionsList.Sum(p => p.CostBasis.Amount),
                TotalMarketValue = positionsList.Sum(p => p.MarketValue.Amount),
                TotalUnrealizedPL = positionsList.Sum(p => p.UnrealizedPL.Amount),
                TotalUnrealizedPLPercent = 0, // Will calculate below
                ProfitablePositions = positionsList.Count(p => p.IsProfitable()),
                LosingPositions = positionsList.Count(p => p.IsInLoss()),
                LargestGain = positionsList.Where(p => p.IsProfitable()).DefaultIfEmpty().Max(p => p?.UnrealizedPL.Amount ?? 0),
                LargestLoss = positionsList.Where(p => p.IsInLoss()).DefaultIfEmpty().Min(p => p?.UnrealizedPL.Amount ?? 0)
            };

            // Calculate total unrealized P/L percentage
            if (summary.TotalCostBasis != 0)
            {
                summary = summary with
                {
                    TotalUnrealizedPLPercent = (summary.TotalUnrealizedPL / summary.TotalCostBasis) * 100
                };
            }

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get position summary: AccountNumber={AccountNumber}", accountNumber);
            return Error.Failure("Position.SummaryFailed", "An error occurred while retrieving position summary");
        }
    }

    public async Task<ErrorOr<decimal>> CalculatePortfolioValueAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var positions = await _positionRepository.GetOpenPositionsAsync(accountNumber, cancellationToken);
            var totalValue = positions.Sum(p => p.MarketValue.Amount);

            _logger.LogInformation(
                "Calculated portfolio value: AccountNumber={AccountNumber}, Value={Value}",
                accountNumber, totalValue);

            return totalValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate portfolio value: AccountNumber={AccountNumber}", accountNumber);
            return Error.Failure("Portfolio.CalculationFailed", "An error occurred while calculating portfolio value");
        }
    }
}
