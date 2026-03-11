namespace FinanceTracker.BlazorUI.Models;

public sealed class MonthlyTrendDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetBalance { get; set; }
}

