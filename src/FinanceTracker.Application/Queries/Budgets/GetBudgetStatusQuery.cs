using FinanceTracker.Application.DTOs;
using MediatR;

namespace FinanceTracker.Application.Queries.Budgets;

public sealed record GetBudgetStatusQuery(
    Guid UserId,
    int Month,
    int Year) : IRequest<List<BudgetStatusDto>>;
