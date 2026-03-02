using System.Net;
using System.Net.Http.Json;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Enums;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceTracker.IntegrationTests;

public sealed class BudgetApiTests : IntegrationTestBase
{
    [Fact]
    public async Task SetBudget_ReturnsOk()
    {
        var command = new
        {
            UserId = TestUserId,
            Category = Category.Food,
            LimitAmount = 100m,
            Month = DateTime.UtcNow.Month,
            Year = DateTime.UtcNow.Year
        };

        var response = await Client.PostAsync("/api/v1/budgets", AsJson(command));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var budget = await response.Content.ReadFromJsonAsync<BudgetDto>();
        budget.Should().NotBeNull();
        budget!.LimitAmount.Should().Be(100m);
    }

    [Fact]
    public async Task GetBudgetStatus_ShowsCorrectSpentAmount_AfterTransactions()
    {
        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;

        var setBudget = new
        {
            UserId = TestUserId,
            Category = Category.Food,
            LimitAmount = 100m,
            Month = month,
            Year = year
        };

        await Client.PostAsync("/api/v1/budgets", AsJson(setBudget));

        var response = await Client.GetAsync($"/api/v1/budgets/status?month={month}&year={year}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statuses = await response.Content.ReadFromJsonAsync<List<BudgetStatusDto>>();
        statuses.Should().NotBeNull();
        statuses!.Should().NotBeEmpty();
        statuses!.First().SpentAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task BudgetStatus_ShowsExceeded_WhenSpentOverLimit()
    {
        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;

        var setBudget = new
        {
            UserId = TestUserId,
            Category = Category.Food,
            LimitAmount = 50m,
            Month = month,
            Year = year
        };

        await Client.PostAsync("/api/v1/budgets", AsJson(setBudget));

        var createTransaction = new
        {
            UserId = TestUserId,
            Amount = 200m,
            Currency = "USD",
            Category = Category.Food,
            Description = "Big dinner",
            Date = DateTime.UtcNow,
            TransactionType = TransactionType.Expense
        };

        await Client.PostAsync("/api/v1/transactions", AsJson(createTransaction));

        var response = await Client.GetAsync($"/api/v1/budgets/status?month={month}&year={year}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statuses = await response.Content.ReadFromJsonAsync<List<BudgetStatusDto>>();
        statuses.Should().NotBeNull();
        statuses!.First().IsExceeded.Should().BeTrue();
    }

    [Fact]
    public async Task BudgetExceededIntegrationEvent_IsPublished()
    {
        using var scope = Factory.Services.CreateScope();
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();

        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;

        var setBudget = new
        {
            UserId = TestUserId,
            Category = Category.Food,
            LimitAmount = 50m,
            Month = month,
            Year = year
        };

        await Client.PostAsync("/api/v1/budgets", AsJson(setBudget));

        var createTransaction = new
        {
            UserId = TestUserId,
            Amount = 200m,
            Currency = "USD",
            Category = Category.Food,
            Description = "Big dinner",
            Date = DateTime.UtcNow,
            TransactionType = TransactionType.Expense
        };

        await Client.PostAsync("/api/v1/transactions", AsJson(createTransaction));

        (await harness.Published.Any<FinanceTracker.Application.Events.BudgetExceededIntegrationEvent>())
            .Should().BeTrue();
    }
}

