using System.Text.RegularExpressions;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new Domain.Exceptions.DomainException("Email cannot be empty.");

        var trimmed = email.Trim();

        if (trimmed.Length > 256)
            throw new Domain.Exceptions.DomainException("Email is too long.");

        if (!EmailRegex.IsMatch(trimmed))
            throw new Domain.Exceptions.DomainException($"Invalid email format: {email}.");

        return new Email(trimmed.ToLowerInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
