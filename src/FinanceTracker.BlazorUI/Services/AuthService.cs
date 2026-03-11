using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using FinanceTracker.BlazorUI.Models;

namespace FinanceTracker.BlazorUI.Services;

public sealed class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly CustomAuthStateProvider _authStateProvider;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        CustomAuthStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", request, JsonOptions);
        var auth = await HandleAuthResponseAsync(response);

        await PersistTokensAsync(auth);
        _authStateProvider.NotifyAuthStateChanged();

        return auth;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/register", request, JsonOptions);
        var auth = await HandleAuthResponseAsync(response);

        await PersistTokensAsync(auth);
        _authStateProvider.NotifyAuthStateChanged();

        return auth;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(CustomAuthStateProvider.AuthTokenKey);
        await _localStorage.RemoveItemAsync(CustomAuthStateProvider.RefreshTokenKey);
        _authStateProvider.NotifyAuthStateChanged();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var principal = await GetPrincipalFromStoredTokenAsync();
        return principal.Identity is { IsAuthenticated: true };
    }

    public async Task<string?> GetUserNameAsync()
    {
        var principal = await GetPrincipalFromStoredTokenAsync();
        return principal.Claims.FirstOrDefault(c =>
                   c.Type == ClaimTypes.Name ||
                   c.Type == "name" ||
                   c.Type == "preferred_username" ||
                   c.Type == "username")
               ?.Value;
    }

    public async Task<Guid?> GetUserIdAsync()
    {
        var principal = await GetPrincipalFromStoredTokenAsync();

        var id = principal.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type == "sub" ||
                c.Type == "userId")
            ?.Value;

        if (Guid.TryParse(id, out var guid))
        {
            return guid;
        }

        return null;
    }

    private async Task<AuthResponse> HandleAuthResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(response.StatusCode, "Authentication failed.", content);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ApiException(HttpStatusCode.InternalServerError, "Empty authentication response.", content);
        }

        var apiResponse = JsonSerializer.Deserialize<ApiResponse<AuthResponse>>(content, JsonOptions);
        if (apiResponse is { Success: true } && apiResponse.Data is not null)
        {
            return apiResponse.Data;
        }

        if (apiResponse is { Success: false })
        {
            throw new ApiException(
                HttpStatusCode.BadRequest,
                apiResponse.Message ?? "Authentication failed.",
                content,
                apiResponse.Errors);
        }

        var auth = JsonSerializer.Deserialize<AuthResponse>(content, JsonOptions);
        if (auth is null)
        {
            throw new ApiException(HttpStatusCode.InternalServerError, "Unable to deserialize authentication response.", content);
        }

        return auth;
    }

    private async Task PersistTokensAsync(AuthResponse auth)
    {
        if (!string.IsNullOrWhiteSpace(auth.AccessToken))
        {
            await _localStorage.SetItemAsync(CustomAuthStateProvider.AuthTokenKey, auth.AccessToken);
        }

        if (!string.IsNullOrWhiteSpace(auth.RefreshToken))
        {
            await _localStorage.SetItemAsync(CustomAuthStateProvider.RefreshTokenKey, auth.RefreshToken!);
        }
    }

    private async Task<ClaimsPrincipal> GetPrincipalFromStoredTokenAsync()
    {
        var token = await _localStorage.GetItemAsync<string?>(CustomAuthStateProvider.AuthTokenKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var principal = CustomAuthStateProvider.CreatePrincipalFromToken(token);

        var expClaim = principal.Claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim is null || !long.TryParse(expClaim.Value, out var expSeconds))
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var expiry = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
        if (expiry <= DateTime.UtcNow)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        return principal;
    }
}

