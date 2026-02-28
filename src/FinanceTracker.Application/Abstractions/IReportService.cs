using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Abstractions;

public interface IReportService
{
    Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(Guid userId, int months, CancellationToken cancellationToken = default);
    Task<SpendingInsightDto> GetSpendingInsightAsync(Guid userId, CancellationToken cancellationToken = default);
}
