using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Enums;
using MediatR;

namespace FinanceTracker.Application.Commands.Budgets;

public sealed record SetBudgetCommand(
    Guid UserId,
    Category Category,
    decimal LimitAmount,
    int Month,
    int Year) : IRequest<BudgetDto>;
