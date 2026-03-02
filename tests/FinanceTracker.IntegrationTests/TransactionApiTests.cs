using System.Net;
using System.Net.Http.Json;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Queries.Transactions;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.ValueObjects;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinanceTracker.IntegrationTests;

public sealed class TransactionApiTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateTransaction_ReturnsCreatedResult_WithValidData()
    {
        var command = new
        {
            UserId = TestUserId,
            Amount = 50m,
            Currency = "USD",
            Category = Category.Food,
            Description = "Test",
            Date = DateTime.UtcNow,
            TransactionType = TransactionType.Expense
        };

        var response = await Client.PostAsync("/api/v1/transactions", AsJson(command));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = await response.Content.ReadFromJsonAsync<TransactionDto>();
        dto.Should().NotBeNull();
        dto!.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task CreateTransaction_Returns400_WhenAmountIsNegative()
    {
        var command = new
        {
            UserId = TestUserId,
            Amount = -10m,
            Currency = "USD",
            Category = Category.Food,
            Description = "Invalid",
            Date = DateTime.UtcNow,
            TransactionType = TransactionType.Expense
        };

        var response = await Client.PostAsync("/api/v1/transactions", AsJson(command));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransactions_ReturnsMonthlyTransactions()
    {
        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;

        var response = await Client.GetAsync($"/api/v1/transactions?month={month}&year={year}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        items.Should().NotBeNull();
        items!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSummary_ReturnsCorrectNetBalance()
    {
        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;

        var response = await Client.GetAsync($"/api/v1/transactions/summary?month={month}&year={year}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<MonthlySummaryDto>();
        summary.Should().NotBeNull();
        summary!.NetBalance.Should().BeLessOrEqualTo(0);
    }
}

