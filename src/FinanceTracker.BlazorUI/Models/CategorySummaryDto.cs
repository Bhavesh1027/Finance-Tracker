using FinanceTracker.Domain.Enums;

namespace FinanceTracker.BlazorUI.Models;

public sealed class CategorySummaryDto
{
    public Category Category { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
}

