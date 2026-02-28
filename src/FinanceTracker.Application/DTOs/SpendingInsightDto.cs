namespace FinanceTracker.Application.DTOs;

public sealed record SpendingInsightDto(
    string TopCategory,
    decimal AverageDailySpend,
    decimal BiggestTransaction,
    string BusiestDayOfWeek);
