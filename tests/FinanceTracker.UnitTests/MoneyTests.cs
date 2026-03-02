using FinanceTracker.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FinanceTracker.UnitTests;

public sealed class MoneyTests
{
    [Fact]
    public void Should_Add_TwoMoneyObjects_WithSameCurrency()
    {
        var first = Money.Create(50m, "USD");
        var second = Money.Create(25m, "USD");

        var result = first + second;

        result.Amount.Should().Be(75m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Should_ThrowException_WhenAddingDifferentCurrencies()
    {
        var usd = Money.Create(50m, "USD");
        var eur = Money.Create(25m, "EUR");

        Action act = () => { var _ = usd + eur; };

        act.Should().Throw<Domain.Exceptions.DomainException>()
            .WithMessage("Cannot add money with different currencies*");
    }

    [Theory]
    [InlineData(10, "usd", 10, "USD")]
    [InlineData(99.99, "EUR", 99.99, "EUR")]
    public void Should_BeEqual_WhenAmountAndCurrencyMatch(decimal amount, string currency, decimal otherAmount, string otherCurrency)
    {
        var first = Money.Create(amount, currency);
        var second = Money.Create(otherAmount, otherCurrency);

        first.Should().Be(second);
    }
}

