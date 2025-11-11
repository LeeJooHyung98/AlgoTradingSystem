using AlgoTrading.Application.Commands.Trading;
using FluentValidation;

namespace AlgoTrading.Application.Validators;

/// <summary>
/// description(설명) : Close Position Command Validator (포지션 청산 명령 검증기)
/// Details(상세설명) : Validates ClosePositionCommand using FluentValidation rules (FluentValidation 규칙을 사용하여 ClosePositionCommand 검증)
/// Applied technology patterns(적용기술패턴) : Validator Pattern, Fluent API Pattern (검증기 패턴, Fluent API 패턴)
/// </summary>
public sealed class ClosePositionCommandValidator : AbstractValidator<ClosePositionCommand>
{
    public ClosePositionCommandValidator()
    {
        // Position ID validation
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("Position ID is required");

        // Quantity validation (if provided)
        When(x => x.Quantity.HasValue, () =>
        {
            RuleFor(x => x.Quantity!.Value)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(1_000_000).WithMessage("Quantity cannot exceed 1,000,000 shares");
        });

        // Order Type validation
        RuleFor(x => x.OrderType)
            .InclusiveBetween(1, 2)
            .WithMessage("Order type must be Market (1) or Limit (2)");

        // Limit Price validation (required for Limit order type)
        When(x => x.OrderType == 2, () =>
        {
            RuleFor(x => x.LimitPrice)
                .NotNull().WithMessage("Limit price is required for limit orders")
                .GreaterThan(0).WithMessage("Limit price must be greater than 0")
                .LessThanOrEqualTo(1_000_000_000m).WithMessage("Limit price is too high");
        });

        // Account Number validation
        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required")
            .Length(8, 12).WithMessage("Account number must be between 8 and 12 characters");

        // Reason validation (if provided)
        When(x => !string.IsNullOrEmpty(x.Reason), () =>
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
        });
    }
}
