using FinanceTracker.Domain.Enums;
using FluentValidation;

namespace FinanceTracker.Application.Commands.Transactions;

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description cannot be empty.");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Date cannot be in the future.");

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithMessage("Category must be a valid enum value.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId cannot be empty.");

        RuleFor(x => x.TransactionType)
            .IsInEnum()
            .WithMessage("TransactionType must be a valid enum value.");
    }
}
