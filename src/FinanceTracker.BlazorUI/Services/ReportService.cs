using System.Net.Http;

namespace FinanceTracker.BlazorUI.Services;

public sealed class ReportService : IReportService
{
    private readonly HttpClient _httpClient;

    public ReportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}

