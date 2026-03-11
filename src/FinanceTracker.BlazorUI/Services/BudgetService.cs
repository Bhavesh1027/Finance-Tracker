using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using FinanceTracker.BlazorUI.Models;

namespace FinanceTracker.BlazorUI.Services;

public sealed class BudgetService : IBudgetService
{
    private const string TokenStorageKey = CustomAuthStateProvider.AuthTokenKey;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public BudgetService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<List<BudgetStatusDto>> GetBudgetStatusAsync(int month, int year)
    {
        await AddAuthorizationHeaderAsync();

        var response = await _httpClient.GetAsync($"/api/v1/budgets/status?month={month}&year={year}");
        return await ReadContentAsync<List<BudgetStatusDto>>(response);
    }

    public async Task<BudgetDto> SetBudgetAsync(SetBudgetRequest request)
    {
        await AddAuthorizationHeaderAsync();

        var userId = await GetUserIdFromTokenAsync();

        var payload = new
        {
            UserId = userId,
            request.Category,
            request.LimitAmount,
            request.Month,
            request.Year
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/v1/budgets", content);
        return await ReadContentAsync<BudgetDto>(response);
    }

    private async Task AddAuthorizationHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync<string?>(TokenStorageKey);

        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static async Task<T> ReadContentAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw CreateApiException(response.StatusCode, content);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ApiException(HttpStatusCode.InternalServerError, "The server returned an empty response.", content);
        }

        try
        {
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
            if (apiResponse is { Success: true } && apiResponse.Data is not null)
            {
                return apiResponse.Data;
            }

            if (apiResponse is { Success: false })
            {
                throw new ApiException(
                    HttpStatusCode.BadRequest,
                    apiResponse.Message ?? "The server reported a failure.",
                    content,
                    apiResponse.Errors);
            }
        }
        catch (JsonException)
        {
        }

        var result = JsonSerializer.Deserialize<T>(content, JsonOptions);

        if (result is null)
        {
            throw new ApiException(HttpStatusCode.InternalServerError, "Unable to deserialize server response.", content);
        }

        return result;
    }

    private static ApiException CreateApiException(HttpStatusCode statusCode, string content)
    {
        try
        {
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, JsonOptions);
            if (apiResponse is not null)
            {
                var message = apiResponse.Message ??
                              $"The server returned an unsuccessful status code: {(int)statusCode}.";

                return new ApiException(statusCode, message, content, apiResponse.Errors);
            }
        }
        catch (JsonException)
        {
        }

        var fallbackMessage = string.IsNullOrWhiteSpace(content)
            ? $"The server returned an unsuccessful status code: {(int)statusCode}."
            : content;

        return new ApiException(statusCode, fallbackMessage, content);
    }

    private async Task<Guid> GetUserIdFromTokenAsync()
    {
        var token = await _localStorage.GetItemAsync<string?>(TokenStorageKey);

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ApiException(HttpStatusCode.Unauthorized, "User is not authenticated. Missing token.");
        }

        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            throw new ApiException(HttpStatusCode.Unauthorized, "Invalid JWT token format.");
        }

        var payload = parts[1];
        var buffer = PadBase64(payload);
        var bytes = Convert.FromBase64String(buffer);
        using var document = JsonDocument.Parse(bytes);

        if (document.RootElement.TryGetProperty("sub", out var subElement) &&
            Guid.TryParse(subElement.GetString(), out var subId))
        {
            return subId;
        }

        if (document.RootElement.TryGetProperty("userId", out var userIdElement) &&
            Guid.TryParse(userIdElement.GetString(), out var userId))
        {
            return userId;
        }

        throw new ApiException(HttpStatusCode.Unauthorized, "User identifier not found in token.");
    }

    private static string PadBase64(string base64)
    {
        var padded = base64.Replace('-', '+').Replace('_', '/');
        return padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
    }
}

