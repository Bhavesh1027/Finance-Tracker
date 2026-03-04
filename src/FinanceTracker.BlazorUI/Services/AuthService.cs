using System.Net.Http;

namespace FinanceTracker.BlazorUI.Services;

public sealed class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}

