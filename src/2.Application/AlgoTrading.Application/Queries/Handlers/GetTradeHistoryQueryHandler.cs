using AlgoTrading.Application.DTOs.Trading;
using AlgoTrading.Application.Mappers;
using AlgoTrading.Application.Queries.Trading;
using AlgoTrading.Core.Entities.Trading;
using AlgoTrading.Core.Interfaces.Repositories;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AlgoTrading.Application.Queries.Handlers;

/// <summary>
/// description(설명) : Get Trade History Query Handler (거래 이력 조회 쿼리 핸들러)
/// Details(상세설명) : Retrieves trade history with optional filters and pagination (필터와 페이징을 포함한 거래 이력 조회)
/// Applied technology patterns(적용기술패턴) : CQRS Pattern, Handler Pattern, Repository Pattern, Pagination Pattern (CQRS 패턴, 핸들러 패턴, Repository 패턴, 페이징 패턴)
/// </summary>
public sealed class GetTradeHistoryQueryHandler : IRequestHandler<GetTradeHistoryQuery, ErrorOr<IEnumerable<TradeDto>>>
{
    private readonly ITradeRepository _tradeRepository;
    private readonly ILogger<GetTradeHistoryQueryHandler> _logger;

    public GetTradeHistoryQueryHandler(
        ITradeRepository tradeRepository,
        ILogger<GetTradeHistoryQueryHandler> logger)
    {
        _tradeRepository = tradeRepository;
        _logger = logger;
    }

    public async Task<ErrorOr<IEnumerable<TradeDto>>> Handle(GetTradeHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Retrieving trade history: AccountNumber={AccountNumber}, StartDate={StartDate}, EndDate={EndDate}, Page={Page}, PageSize={PageSize}",
                request.AccountNumber, request.StartDate, request.EndDate, request.PageNumber, request.PageSize);

            // Get trades by account number
            var trades = await _tradeRepository.GetByAccountNumberAsync(request.AccountNumber, cancellationToken);

            // Apply date range filter if provided
            if (request.StartDate.HasValue)
            {
                trades = trades.Where(t => t.ExitTime >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                trades = trades.Where(t => t.ExitTime <= request.EndDate.Value);
            }

            // Apply stock code filter if provided
            if (!string.IsNullOrEmpty(request.StockCode))
            {
                trades = trades.Where(t => t.StockCode.Value == request.StockCode);
            }

            // Apply strategy ID filter if provided
            if (request.StrategyId.HasValue)
            {
                trades = trades.Where(t => t.StrategyId == request.StrategyId.Value);
            }

            // Apply result filter if provided
            if (request.Result.HasValue)
            {
                var resultEnum = (TradeResult)request.Result.Value;
                trades = trades.Where(t => t.Result == resultEnum);
            }

            // Order by exit time descending (most recent first)
            var orderedTrades = trades.OrderByDescending(t => t.ExitTime);

            // Apply pagination
            var totalCount = orderedTrades.Count();
            var skip = (request.PageNumber - 1) * request.PageSize;
            var pagedTrades = orderedTrades
                .Skip(skip)
                .Take(request.PageSize);

            var tradeDtos = pagedTrades
                .Select(t => t.ToDto())
                .ToList();

            _logger.LogInformation(
                "Retrieved {Count} trades (Page {Page} of {TotalPages}, Total: {TotalCount}) for account {AccountNumber}",
                tradeDtos.Count,
                request.PageNumber,
                (int)Math.Ceiling((double)totalCount / request.PageSize),
                totalCount,
                request.AccountNumber);

            return tradeDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve trade history: AccountNumber={AccountNumber}", request.AccountNumber);
            return Error.Failure("Trades.RetrievalFailed", "An error occurred while retrieving trade history");
        }
    }
}
