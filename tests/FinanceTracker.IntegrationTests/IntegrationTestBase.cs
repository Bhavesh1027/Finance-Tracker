using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;
using FinanceTracker.Infrastructure.Data;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceTracker.IntegrationTests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly FinanceTrackerWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected Guid TestUserId { get; } = Guid.NewGuid();

    protected IntegrationTestBase()
    {
        Factory = new FinanceTrackerWebApplicationFactory();
        Client = Factory.CreateClient();

        // For simplicity in tests, bypass real JWT and use a dummy bearer
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Users.Add(User.Create(TestUserId, "test@example.com", "Test User"));

        var amount = Money.Create(100m, "USD");
        var transaction = Transaction.Create(
            TestUserId,
            amount,
            Category.Food,
            "Initial",
            DateTime.UtcNow,
            TransactionType.Expense);

        context.Transactions.Add(transaction);

        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Users.RemoveRange(context.Users);
        context.Transactions.RemoveRange(context.Transactions);
        context.Budgets.RemoveRange(context.Budgets);

        await context.SaveChangesAsync();
    }

    protected static StringContent AsJson(object value)
        => new(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
}

