using FinanceTracker.BlazorUI.Models;

namespace FinanceTracker.BlazorUI.Services;

public interface IReportService
{
    Task<List<MonthlyTrendDto>> GetTrendsAsync(int months = 6);
    Task<SpendingInsightDto> GetInsightsAsync();
}

