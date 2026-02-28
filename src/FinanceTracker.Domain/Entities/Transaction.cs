using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Events;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Entities;

public sealed class Transaction : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Money Amount { get; private set; }
    public Category Category { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public TransactionType Type { get; private set; }

    private Transaction() : base()
    {
        Amount = null!;
        Description = null!;
    }

    private Transaction(Guid id, Guid userId, Money amount, Category category, string description, DateTime date, TransactionType type) : base(id)
    {
        UserId = userId;
        Amount = amount;
        Category = category;
        Description = description;
        Date = date;
        Type = type;
    }

    public static Transaction Create(Guid userId, Money amount, Category category, string description, DateTime date, TransactionType type)
    {
        if (userId == Guid.Empty)
            throw new Exceptions.DomainException("UserId cannot be empty.");

        if (amount.Amount <= 0)
            throw new Exceptions.DomainException("Transaction amount must be greater than zero.");

        if (description.Length > 500)
            throw new Exceptions.DomainException("Description cannot exceed 500 characters.");

        var transaction = new Transaction(Guid.NewGuid(), userId, amount, category, description ?? string.Empty, date, type);
        transaction.RaiseDomainEvent(new TransactionCreatedDomainEvent(transaction.Id, userId));
        return transaction;
    }
}
