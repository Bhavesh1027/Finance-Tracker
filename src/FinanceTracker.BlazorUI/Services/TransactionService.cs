using System.Net.Http;

namespace FinanceTracker.BlazorUI.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly HttpClient _httpClient;

    public TransactionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}

