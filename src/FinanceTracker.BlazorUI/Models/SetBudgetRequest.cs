using FinanceTracker.Domain.Enums;

namespace FinanceTracker.BlazorUI.Models;

public sealed class SetBudgetRequest
{
    public Category Category { get; set; }
    public decimal LimitAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
}

