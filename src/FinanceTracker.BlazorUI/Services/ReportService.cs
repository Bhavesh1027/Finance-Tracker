using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Blazored.LocalStorage;
using FinanceTracker.BlazorUI.Models;

namespace FinanceTracker.BlazorUI.Services;

public sealed class ReportService : IReportService
{
    private const string TokenStorageKey = CustomAuthStateProvider.AuthTokenKey;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ReportService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<List<MonthlyTrendDto>> GetTrendsAsync(int months = 6)
    {
        await AddAuthorizationHeaderAsync();

        var response = await _httpClient.GetAsync($"/api/v1/reports/trends?months={months}");
        return await ReadContentAsync<List<MonthlyTrendDto>>(response);
    }

    public async Task<SpendingInsightDto> GetInsightsAsync()
    {
        await AddAuthorizationHeaderAsync();

        var response = await _httpClient.GetAsync("/api/v1/reports/insights");
        return await ReadContentAsync<SpendingInsightDto>(response);
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
}

