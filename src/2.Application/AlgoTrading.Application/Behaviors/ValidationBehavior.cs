using ErrorOr;
using FluentValidation;
using MediatR;

namespace AlgoTrading.Application.Behaviors;

/// <summary>
/// description(설명) : Validation Behavior (유효성 검증 동작)
/// Details(상세설명) : MediatR pipeline behavior that automatically validates requests using FluentValidation (FluentValidation을 사용하여 요청을 자동으로 검증하는 MediatR 파이프라인 동작)
/// Applied technology patterns(적용기술패턴) : Pipeline Pattern, Decorator Pattern, Validation Pattern (파이프라인 패턴, 데코레이터 패턴, 검증 패턴)
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If no validators, skip validation
        if (!_validators.Any())
        {
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        // If no failures, continue pipeline
        if (!failures.Any())
        {
            return await next();
        }

        // Convert validation failures to ErrorOr errors
        var errors = failures
            .Select(failure => Error.Validation(
                code: $"{typeof(TRequest).Name}.{failure.PropertyName}",
                description: failure.ErrorMessage))
            .ToList();

        // Return validation errors
        // Note: This uses reflection to create ErrorOr<TResponse> with errors
        return (TResponse)(dynamic)errors;
    }
}
