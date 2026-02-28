namespace FinanceTracker.Application.DTOs;

public sealed record MonthlyTrendDto(
    int Month,
    int Year,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetBalance);
