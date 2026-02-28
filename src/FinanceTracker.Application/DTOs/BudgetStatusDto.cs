using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public sealed record BudgetStatusDto(
    Category Category,
    decimal LimitAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    decimal PercentageUsed,
    bool IsExceeded);
