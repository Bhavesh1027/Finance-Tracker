using System.Net;
using System.Net.Http.Json;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Events;
using FinanceTracker.Domain.Enums;
using FinanceTracker.IntegrationTests;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;

namespace FinanceTracker.IntegrationTests.Steps;

[Binding]
public sealed class BudgetAlertSteps
{
    private readonly FinanceTrackerWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestHarness _harness;

    private Guid _userId;
    private int _month;
    private int _year;
    private HttpResponseMessage? _lastResponse;

    public BudgetAlertSteps()
    {
        _factory = new FinanceTrackerWebApplicationFactory();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        var scope = _factory.Services.CreateScope();
        _harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
    }

    [Given(@"a user has set a Food budget of (.*) INR for February (.*)")]
    public async Task GivenAUserHasSetAFoodBudgetOfInrForFebruary(int amount, int year)
    {
        _userId = Guid.NewGuid();
        _month = 2;
        _year = year;

        var command = new
        {
            UserId = _userId,
            Category = Category.Food,
            LimitAmount = (decimal)amount,
            Month = _month,
            Year = _year
        };

        var response = await _client.PostAsJsonAsync("/api/v1/budgets", command);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Given(@"the user has already spent (.*) INR on Food")]
    public async Task GivenTheUserHasAlreadySpentInrOnFood(int amount)
    {
        var command = new
        {
            UserId = _userId,
            Amount = (decimal)amount,
            Currency = "INR",
            Category = Category.Food,
            Description = "Existing spend",
            Date = new DateTime(_year, _month, 1),
            TransactionType = Domain.Enums.TransactionType.Expense
        };

        var response = await _client.PostAsJsonAsync("/api/v1/transactions", command);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [When(@"the user adds a Food transaction of (.*) INR")]
    public async Task WhenTheUserAddsAFoodTransactionOfInr(int amount)
    {
        var command = new
        {
            UserId = _userId,
            Amount = (decimal)amount,
            Currency = "INR",
            Category = Category.Food,
            Description = "New spend",
            Date = new DateTime(_year, _month, 2),
            TransactionType = Domain.Enums.TransactionType.Expense
        };

        _lastResponse = await _client.PostAsJsonAsync("/api/v1/transactions", command);
    }

    [Then(@"a budget exceeded alert should be published")]
    public async Task ThenABudgetExceededAlertShouldBePublished()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.Created);

        var published = await _harness.Published.Any<BudgetExceededIntegrationEvent>();
        published.Should().BeTrue();
    }

    [Then(@"no budget alert should be published")]
    public async Task ThenNoBudgetAlertShouldBePublished()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.Created);

        var published = await _harness.Published.Any<BudgetExceededIntegrationEvent>();
        published.Should().BeFalse();
    }

    [Then(@"the budget status should show (.*)% usage")]
    public async Task ThenTheBudgetStatusShouldShowUsage(int expectedPercentage)
    {
        var response = await _client.GetAsync($"/api/v1/budgets/status?month={_month}&year={_year}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var statuses = await response.Content.ReadFromJsonAsync<List<BudgetStatusDto>>();
        statuses.Should().NotBeNull();
        var status = statuses!.First(s => s.Category == Category.Food);

        status.PercentageUsed.Should().Be(expectedPercentage);
    }
}

