using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Events;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Entities;

public sealed class Budget : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public Category Category { get; private set; }
    public Money LimitAmount { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }

    private Budget() : base()
    {
        LimitAmount = null!;
    }

    private Budget(Guid id, Guid userId, Category category, Money limitAmount, int month, int year) : base(id)
    {
        UserId = userId;
        Category = category;
        LimitAmount = limitAmount;
        Month = month;
        Year = year;
    }

    public static Budget Create(Guid userId, Category category, Money limitAmount, int month, int year)
    {
        if (userId == Guid.Empty)
            throw new Exceptions.DomainException("UserId cannot be empty.");

        if (limitAmount.Amount <= 0)
            throw new Exceptions.DomainException("Budget limit amount must be greater than zero.");

        if (month < 1 || month > 12)
            throw new Exceptions.DomainException("Month must be between 1 and 12.");

        if (year < 1900 || year > 2100)
            throw new Exceptions.DomainException("Year must be between 1900 and 2100.");

        return new Budget(Guid.NewGuid(), userId, category, limitAmount, month, year);
    }

    public void CheckBudgetExceeded(decimal spent)
    {
        if (spent > LimitAmount.Amount)
        {
            RaiseDomainEvent(new BudgetExceededDomainEvent(UserId, Category, spent, LimitAmount.Amount));
        }
    }

    public void UpdateLimit(Money limitAmount)
    {
        if (limitAmount.Amount <= 0)
            throw new Exceptions.DomainException("Budget limit amount must be greater than zero.");

        LimitAmount = limitAmount;
    }
}
