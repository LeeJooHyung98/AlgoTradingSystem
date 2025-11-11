using MediatR;
using Microsoft.Extensions.Logging;
using IUnitOfWork = AlgoTrading.Core.Interfaces.IUnitOfWork;

namespace AlgoTrading.Application.Behaviors;

/// <summary>
/// description(설명) : Transaction Behavior (트랜잭션 동작)
/// Details(상세설명) : MediatR pipeline behavior that wraps requests in a database transaction (데이터베이스 트랜잭션으로 요청을 감싸는 MediatR 파이프라인 동작)
/// Applied technology patterns(적용기술패턴) : Pipeline Pattern, Decorator Pattern, Unit of Work Pattern (파이프라인 패턴, 데코레이터 패턴, Unit of Work 패턴)
/// </summary>
/// <remarks>
/// Note: This behavior only applies to Commands that modify data.
/// Queries should not use transactions.
/// Transaction is automatically committed if successful, rolled back on exception.
/// </remarks>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Skip transaction for Query requests (read-only)
        if (requestName.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
        {
            return await next();
        }

        _logger.LogInformation(
            "[TRANSACTION] Starting transaction for {RequestName}",
            requestName);

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Execute the request handler
            var response = await next();

            // Commit transaction if successful
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "[TRANSACTION] Transaction committed successfully for {RequestName}",
                requestName);

            return response;
        }
        catch (Exception ex)
        {
            // Rollback transaction on error
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(
                ex,
                "[TRANSACTION] Transaction rolled back for {RequestName} | Error: {ErrorMessage}",
                requestName,
                ex.Message);

            throw;
        }
    }
}
