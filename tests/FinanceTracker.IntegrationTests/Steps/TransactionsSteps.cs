using System.Net;
using System.Net.Http.Json;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Enums;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace FinanceTracker.IntegrationTests.Steps;

[Binding]
public sealed class TransactionsSteps
{
    private readonly FinanceTrackerWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private Guid _userId;
    private HttpResponseMessage? _lastResponse;
    private int _month;
    private int _year;

    public TransactionsSteps()
    {
        _factory = new FinanceTrackerWebApplicationFactory();
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
    }

    [Given(@"a user exists in the system")]
    public void GivenAUserExistsInTheSystem()
    {
        _userId = Guid.NewGuid();
        _month = 2;
        _year = 2026;
    }

    [When(@"the user creates an Expense transaction of (.*) INR in February 2026 for Food")]
    public async Task WhenTheUserCreatesAnExpenseTransactionOfInrInFebruaryForFood(int amount)
    {
        var command = new
        {
            UserId = _userId,
            Amount = (decimal)amount,
            Currency = "INR",
            Category = Category.Food,
            Description = "BDD transaction",
            Date = new DateTime(2026, 2, 10),
            TransactionType = Domain.Enums.TransactionType.Expense
        };

        _lastResponse = await _client.PostAsJsonAsync("/api/v1/transactions", command);
    }

    [Then(@"the transaction should be created successfully")]
    public async Task ThenTheTransactionShouldBeCreatedSuccessfully()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await _lastResponse.Content.ReadFromJsonAsync<TransactionDto>();
        dto.Should().NotBeNull();
        dto!.Category.Should().Be(Category.Food);
    }

    [Then(@"the monthly transactions should include that transaction")]
    public async Task ThenTheMonthlyTransactionsShouldIncludeThatTransaction()
    {
        var response = await _client.GetAsync($"/api/v1/transactions?month={_month}&year={_year}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await response.Content.ReadFromJsonAsync<List<TransactionDto>>();
        items.Should().NotBeNull();
        items!.Any(t => t.Category == Category.Food && t.Amount > 0).Should().BeTrue();
    }

    [When(@"the user creates an Expense transaction of -500 INR in February 2026 for Food")]
    public async Task WhenTheUserCreatesAnExpenseTransactionOfMinusInrInFebruaryForFood()
    {
        var command = new
        {
            UserId = _userId,
            Amount = -500m,
            Currency = "INR",
            Category = Category.Food,
            Description = "Invalid BDD transaction",
            Date = new DateTime(2026, 2, 11),
            TransactionType = Domain.Enums.TransactionType.Expense
        };

        _lastResponse = await _client.PostAsJsonAsync("/api/v1/transactions", command);
    }

    [Then(@"the transaction request should be rejected as bad request")]
    public void ThenTheTransactionRequestShouldBeRejectedAsBadRequest()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

