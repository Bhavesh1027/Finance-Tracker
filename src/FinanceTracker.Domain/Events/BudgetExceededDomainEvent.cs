using FinanceTracker.Domain.Common;
using MediatR;

namespace FinanceTracker.Domain.Events;

public sealed record BudgetExceededDomainEvent(
    Guid UserId,
    Enums.Category Category,
    decimal Spent,
    decimal Limit) : IDomainEvent, INotification;
