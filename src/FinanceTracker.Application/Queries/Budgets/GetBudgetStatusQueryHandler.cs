using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Repositories;
using MediatR;

namespace FinanceTracker.Application.Queries.Budgets;

public sealed class GetBudgetStatusQueryHandler : IRequestHandler<GetBudgetStatusQuery, List<BudgetStatusDto>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetBudgetStatusQueryHandler(
        IBudgetRepository budgetRepository,
        ITransactionRepository transactionRepository)
    {
        _budgetRepository = budgetRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<List<BudgetStatusDto>> Handle(GetBudgetStatusQuery request, CancellationToken cancellationToken)
    {
        var categories = Enum.GetValues<Category>();
        var statuses = new List<BudgetStatusDto>();

        foreach (var category in categories)
        {
            var budget = await _budgetRepository.GetByUserCategoryMonthAsync(
                request.UserId,
                category,
                request.Month,
                request.Year,
                cancellationToken);

            var spent = await _transactionRepository.GetTotalByUserAndCategoryAsync(
                request.UserId,
                category,
                request.Month,
                request.Year,
                cancellationToken);

            // Include only categories that have a budget set
            if (budget is null)
                continue;

            var limitAmount = budget.LimitAmount.Amount;
            var remainingAmount = limitAmount - spent;
            var percentageUsed = limitAmount > 0 ? Math.Round((spent / limitAmount) * 100, 2) : 0;
            var isExceeded = spent > limitAmount;

            statuses.Add(new BudgetStatusDto(
                category,
                limitAmount,
                spent,
                remainingAmount,
                percentageUsed,
                isExceeded));
        }

        return statuses.OrderBy(s => s.Category).ToList();
    }
}
