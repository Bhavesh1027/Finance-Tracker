namespace FinanceTracker.Application.DTOs;

public sealed record MonthlySummaryDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetBalance,
    IReadOnlyList<CategorySummaryDto> BreakdownByCategory);
