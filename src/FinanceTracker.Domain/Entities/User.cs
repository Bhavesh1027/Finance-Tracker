using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.ValueObjects;

namespace FinanceTracker.Domain.Entities;

public sealed class User : Entity<Guid>
{
    public Email Email { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() : base() 
    {
        Email = null!;
        Name = null!;
    }

    private User(Guid id, Email email, string name, DateTime createdAt) : base(id)
    {
        Email = email;
        Name = name;
        CreatedAt = createdAt;
    }

    public static User Create(Email email, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainException("User name cannot be empty.");

        if (name.Length > 200)
            throw new Exceptions.DomainException("User name cannot exceed 200 characters.");

        return new User(Guid.NewGuid(), email, name.Trim(), DateTime.UtcNow);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainException("User name cannot be empty.");

        if (name.Length > 200)
            throw new Exceptions.DomainException("User name cannot exceed 200 characters.");

        Name = name.Trim();
    }

    public void UpdateEmail(Email email)
    {
        Email = email ?? throw new Exceptions.DomainException("Email cannot be null.");
    }
}
