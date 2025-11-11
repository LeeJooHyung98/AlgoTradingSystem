using AlgoTrading.Application.Commands.Trading;
using FluentValidation;

namespace AlgoTrading.Application.Validators;

/// <summary>
/// description(설명) : Create Order Command Validator (주문 생성 명령 검증기)
/// Details(상세설명) : Validates CreateOrderCommand using FluentValidation rules (FluentValidation 규칙을 사용하여 CreateOrderCommand 검증)
/// Applied technology patterns(적용기술패턴) : Validator Pattern, Fluent API Pattern (검증기 패턴, Fluent API 패턴)
/// </summary>
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        // Stock Code validation
        RuleFor(x => x.StockCode)
            .NotEmpty().WithMessage("Stock code is required")
            .Length(6).WithMessage("Stock code must be 6 characters")
            .Matches("^[A-Z0-9]{6}$").WithMessage("Stock code must contain only uppercase letters and numbers");

        // Order Side validation
        RuleFor(x => x.Side)
            .InclusiveBetween(1, 2)
            .WithMessage("Side must be either Buy (1) or Sell (2)");

        // Order Type validation
        RuleFor(x => x.Type)
            .InclusiveBetween(1, 4)
            .WithMessage("Type must be Market (1), Limit (2), Stop (3), or StopLimit (4)");

        // Quantity validation
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(1_000_000).WithMessage("Quantity cannot exceed 1,000,000 shares");

        // Limit Price validation (required for Limit orders)
        When(x => x.Type == 2, () =>
        {
            RuleFor(x => x.LimitPrice)
                .NotNull().WithMessage("Limit price is required for limit orders")
                .GreaterThan(0).WithMessage("Limit price must be greater than 0")
                .LessThanOrEqualTo(1_000_000_000m).WithMessage("Limit price is too high");
        });

        // Stop Price validation (required for Stop orders)
        When(x => x.Type == 3, () =>
        {
            RuleFor(x => x.StopPrice)
                .NotNull().WithMessage("Stop price is required for stop orders")
                .GreaterThan(0).WithMessage("Stop price must be greater than 0")
                .LessThanOrEqualTo(1_000_000_000m).WithMessage("Stop price is too high");
        });

        // Account Number validation
        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required")
            .Length(8, 12).WithMessage("Account number must be between 8 and 12 characters");

        // Order Source validation
        RuleFor(x => x.Source)
            .InclusiveBetween(1, 3)
            .WithMessage("Source must be Manual (1), Strategy (2), or API (3)");

        // Strategy ID validation (required when Source is Strategy)
        When(x => x.Source == 2, () =>
        {
            RuleFor(x => x.StrategyId)
                .NotNull().WithMessage("Strategy ID is required when order source is Strategy");
        });

        // Time In Force validation
        RuleFor(x => x.TimeInForce)
            .InclusiveBetween(1, 4)
            .WithMessage("TimeInForce must be Day (1), GTC (2), IOC (3), or FOK (4)");
    }
}
