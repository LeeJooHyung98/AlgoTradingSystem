using AlgoTrading.Application.DTOs.Trading;
using AlgoTrading.Application.Mappers;
using AlgoTrading.Application.Queries.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Application.Queries.Handlers;

/// <summary>
/// description(설명) : Get Positions Query Handler (포지션 조회 쿼리 핸들러)
/// Details(상세설명) : Retrieves all positions for an account with optional filters (계좌의 모든 포지션을 필터와 함께 조회)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, Handler Pattern, Repository Pattern (CQRS 패턴, 핸들러 패턴, Repository 패턴)
/// </summary>
public sealed class GetPositionsQueryHandler : IRequestHandler<GetPositionsQuery, ErrorOr<IEnumerable<PositionDto>>>
{
    private readonly IPositionRepository _positionRepository;
    private readonly ILogger<GetPositionsQueryHandler> _logger;

    public GetPositionsQueryHandler(
        IPositionRepository positionRepository,
        ILogger<GetPositionsQueryHandler> logger)
    {
        _positionRepository = positionRepository;
        _logger = logger;
    }

    public async Task<ErrorOr<IEnumerable<PositionDto>>> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Retrieving positions: AccountNumber={AccountNumber}, OnlyOpen={OnlyOpen}, StockCode={StockCode}, StrategyId={StrategyId}",
                request.AccountNumber, request.OnlyOpen, request.StockCode, request.StrategyId);

            // Get positions by open/closed status
            var positions = request.OnlyOpen
                ? await _positionRepository.GetOpenPositionsAsync(request.AccountNumber, cancellationToken)
                : await _positionRepository.GetByAccountNumberAsync(request.AccountNumber, cancellationToken);

            // Apply stock code filter if provided
            if (!string.IsNullOrEmpty(request.StockCode))
            {
                positions = positions.Where(p => p.StockCode.Value == request.StockCode);
            }

            // Apply strategy ID filter if provided
            if (request.StrategyId.HasValue)
            {
                positions = positions.Where(p => p.StrategyId == request.StrategyId.Value);
            }

            var positionDtos = positions
                .Select(p => p.ToDto())
                .OrderByDescending(p => p.OpenedAt)
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} positions for account {AccountNumber}",
                positionDtos.Count, request.AccountNumber);

            return positionDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve positions: AccountNumber={AccountNumber}", request.AccountNumber);
            return Error.Failure("Positions.RetrievalFailed", "An error occurred while retrieving positions");
        }
    }
}
