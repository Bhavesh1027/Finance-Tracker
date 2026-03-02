using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public sealed record BudgetAlertDto(
    Guid UserId,
    Category Category,
    decimal SpentAmount,
    decimal LimitAmount,
    int Month,
    int Year);

