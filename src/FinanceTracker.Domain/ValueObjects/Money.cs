using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new Domain.Exceptions.DomainException("Currency cannot be empty.");

        if (currency.Length != 3)
            throw new Domain.Exceptions.DomainException("Currency must be a 3-letter ISO code.");

        return new Money(amount, currency.Trim().ToUpperInvariant());
    }

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new Domain.Exceptions.DomainException(
                $"Cannot add money with different currencies: {left.Currency} and {right.Currency}.");

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
