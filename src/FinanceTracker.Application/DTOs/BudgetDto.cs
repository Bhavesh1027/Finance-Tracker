using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public sealed record BudgetDto(
    Guid Id,
    Guid UserId,
    Category Category,
    decimal LimitAmount,
    string Currency,
    int Month,
    int Year);
