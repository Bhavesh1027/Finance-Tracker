namespace FinanceTracker.BlazorUI.Models;

public sealed class MonthlySummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetBalance { get; set; }
    public List<CategorySummaryDto> BreakdownByCategory { get; set; } = new();
}

