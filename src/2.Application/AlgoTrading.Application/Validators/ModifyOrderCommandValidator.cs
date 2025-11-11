using AlgoTrading.Application.Commands.Trading;
using FluentValidation;

namespace AlgoTrading.Application.Validators;

/// <summary>
/// description(설명) : Modify Order Command Validator (주문 정정 명령 검증기)
/// Details(상세설명) : Validates ModifyOrderCommand using FluentValidation rules (FluentValidation 규칙을 사용하여 ModifyOrderCommand 검증)
/// Applied technology patterns(적용기술패턴) : Validator Pattern, Fluent API Pattern (검증기 패턴, Fluent API 패턴)
/// </summary>
public sealed class ModifyOrderCommandValidator : AbstractValidator<ModifyOrderCommand>
{
    public ModifyOrderCommandValidator()
    {
        // Order ID validation
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        // At least one modification must be provided
        RuleFor(x => x)
            .Must(cmd => cmd.NewQuantity.HasValue || cmd.NewLimitPrice.HasValue || cmd.NewStopPrice.HasValue)
            .WithMessage("At least one modification (quantity, limit price, or stop price) must be provided");

        // New Quantity validation (if provided)
        When(x => x.NewQuantity.HasValue, () =>
        {
            RuleFor(x => x.NewQuantity!.Value)
                .GreaterThan(0).WithMessage("New quantity must be greater than 0")
                .LessThanOrEqualTo(1_000_000).WithMessage("New quantity cannot exceed 1,000,000 shares");
        });

        // New Limit Price validation (if provided)
        When(x => x.NewLimitPrice.HasValue, () =>
        {
            RuleFor(x => x.NewLimitPrice!.Value)
                .GreaterThan(0).WithMessage("New limit price must be greater than 0")
                .LessThanOrEqualTo(1_000_000_000m).WithMessage("New limit price is too high");
        });

        // New Stop Price validation (if provided)
        When(x => x.NewStopPrice.HasValue, () =>
        {
            RuleFor(x => x.NewStopPrice!.Value)
                .GreaterThan(0).WithMessage("New stop price must be greater than 0")
                .LessThanOrEqualTo(1_000_000_000m).WithMessage("New stop price is too high");
        });

        // Account Number validation
        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required")
            .Length(8, 12).WithMessage("Account number must be between 8 and 12 characters");
    }
}
