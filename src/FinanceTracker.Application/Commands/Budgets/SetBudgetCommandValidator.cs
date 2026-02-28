using FluentValidation;

namespace FinanceTracker.Application.Commands.Budgets;

public sealed class SetBudgetCommandValidator : AbstractValidator<SetBudgetCommand>
{
    public SetBudgetCommandValidator()
    {
        RuleFor(x => x.LimitAmount)
            .GreaterThan(0)
            .WithMessage("Limit amount must be greater than zero.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId cannot be empty.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithMessage("Month must be between 1 and 12.");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, 2100)
            .WithMessage("Year must be between 1900 and 2100.");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Category must be a valid enum value.");
    }
}
