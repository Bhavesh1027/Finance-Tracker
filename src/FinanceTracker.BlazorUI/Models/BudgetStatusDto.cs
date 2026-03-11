using FinanceTracker.Domain.Enums;

namespace FinanceTracker.BlazorUI.Models;

public sealed class BudgetStatusDto
{
    public Category Category { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PercentageUsed { get; set; }
    public bool IsExceeded { get; set; }
}

