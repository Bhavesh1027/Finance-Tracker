using FinanceTracker.BlazorUI.Models;

namespace FinanceTracker.BlazorUI.Services;

public interface IBudgetService
{
    Task<List<BudgetStatusDto>> GetBudgetStatusAsync(int month, int year);
    Task<BudgetDto> SetBudgetAsync(SetBudgetRequest request);
}

