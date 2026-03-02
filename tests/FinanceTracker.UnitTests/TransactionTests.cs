using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FinanceTracker.UnitTests;

public sealed class TransactionTests
{
    [Fact]
    public void Should_NotAllow_NegativeAmount()
    {
        var userId = Guid.NewGuid();
        var amount = Money.Create(-10m, "USD");

        Action act = () => Transaction.Create(
            userId,
            amount,
            Category.Food,
            "Invalid",
            DateTime.UtcNow,
            TransactionType.Expense);

        act.Should().Throw<Domain.Exceptions.DomainException>()
            .WithMessage("Transaction amount must be greater than zero.");
    }

    [Fact]
    public void Should_SetCreatedAt_OnCreation()
    {
        var userId = Guid.NewGuid();
        var amount = Money.Create(10m, "USD");

        var transaction = Transaction.Create(
            userId,
            amount,
            Category.Food,
            "Valid",
            DateTime.UtcNow,
            TransactionType.Expense);

        transaction.DomainEvents.Should().ContainSingle(e => e is FinanceTracker.Domain.Events.TransactionCreatedDomainEvent);
    }
}

