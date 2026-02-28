using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.Events;

public sealed record BudgetExceededIntegrationEvent(
    Guid UserId,
    Category Category,
    decimal Spent,
    decimal Limit);
