using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AlgoTrading.Application.Behaviors;

/// <summary>
/// description(설명) : Performance Behavior (성능 측정 동작)
/// Details(상세설명) : MediatR pipeline behavior that measures and logs slow requests (느린 요청을 측정하고 로깅하는 MediatR 파이프라인 동작)
/// Applied technology patterns(적용기술패턴) : Pipeline Pattern, Decorator Pattern, Performance Monitoring Pattern (파이프라인 패턴, 데코레이터 패턴, 성능 모니터링 패턴)
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly Stopwatch _stopwatch;

    // Threshold for slow requests in milliseconds
    private const int SlowRequestThresholdMs = 500;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _stopwatch = new Stopwatch();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _stopwatch.Start();

        var response = await next();

        _stopwatch.Stop();

        var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

        // Log warning if request took longer than threshold
        if (elapsedMilliseconds > SlowRequestThresholdMs)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogWarning(
                "[PERFORMANCE] Slow Request Detected: {RequestName} | Duration: {Duration}ms | Threshold: {Threshold}ms | Request: {@Request}",
                requestName,
                elapsedMilliseconds,
                SlowRequestThresholdMs,
                request);
        }

        return response;
    }
}
