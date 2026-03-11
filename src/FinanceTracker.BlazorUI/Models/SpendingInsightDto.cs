namespace FinanceTracker.BlazorUI.Models;

public sealed class SpendingInsightDto
{
    public string TopCategory { get; set; } = string.Empty;
    public decimal AverageDailySpend { get; set; }
    public decimal BiggestTransaction { get; set; }
    public string BusiestDayOfWeek { get; set; } = string.Empty;
}

