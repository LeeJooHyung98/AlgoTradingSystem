using AlgoTrading.Application.Commands.Trading;
using FluentValidation;

namespace AlgoTrading.Application.Validators;

/// <summary>
/// description(설명) : Cancel Order Command Validator (주문 취소 명령 검증기)
/// Details(상세설명) : Validates CancelOrderCommand using FluentValidation rules (FluentValidation 규칙을 사용하여 CancelOrderCommand 검증)
/// Applied technology patterns(적용기술패턴) : Validator Pattern, Fluent API Pattern (검증기 패턴, Fluent API 패턴)
/// </summary>
public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        // Order ID validation
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        // Reason validation
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required")
            .MaximumLength(500).WithMessage("Cancellation reason cannot exceed 500 characters");

        // Account Number validation
        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required")
            .Length(8, 12).WithMessage("Account number must be between 8 and 12 characters");
    }
}
