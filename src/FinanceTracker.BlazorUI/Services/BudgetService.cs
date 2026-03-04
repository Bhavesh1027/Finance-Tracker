using System.Net.Http;

namespace FinanceTracker.BlazorUI.Services;

public sealed class BudgetService : IBudgetService
{
    private readonly HttpClient _httpClient;

    public BudgetService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}

