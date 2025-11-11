using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AlgoTrading.Application.Behaviors;

/// <summary>
/// description(설명) : Logging Behavior (로깅 동작)
/// Details(상세설명) : MediatR pipeline behavior that logs request and response information (요청 및 응답 정보를 로깅하는 MediatR 파이프라인 동작)
/// Applied technology patterns(적용기술패턴) : Pipeline Pattern, Decorator Pattern, Logging Pattern (파이프라인 패턴, 데코레이터 패턴, 로깅 패턴)
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "[START] {RequestName} | RequestId: {RequestGuid} | Request: {@Request}",
            requestName,
            requestGuid,
            request);

        TResponse? response = default;

        try
        {
            response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "[END] {RequestName} | RequestId: {RequestGuid} | Duration: {Duration}ms | Response: {@Response}",
                requestName,
                requestGuid,
                stopwatch.ElapsedMilliseconds,
                response);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "[ERROR] {RequestName} | RequestId: {RequestGuid} | Duration: {Duration}ms | Error: {ErrorMessage}",
                requestName,
                requestGuid,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}
