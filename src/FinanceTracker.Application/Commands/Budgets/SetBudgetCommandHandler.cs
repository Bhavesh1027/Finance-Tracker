using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Repositories;
using FinanceTracker.Domain.ValueObjects;
using MediatR;

namespace FinanceTracker.Application.Commands.Budgets;

public sealed class SetBudgetCommandHandler : IRequestHandler<SetBudgetCommand, BudgetDto>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetBudgetCommandHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork)
    {
        _budgetRepository = budgetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BudgetDto> Handle(SetBudgetCommand request, CancellationToken cancellationToken)
    {
        var limitAmount = Money.Create(request.LimitAmount, "USD");

        var existing = await _budgetRepository.GetByUserCategoryMonthAsync(
            request.UserId,
            request.Category,
            request.Month,
            request.Year,
            cancellationToken);

        Budget budget;
        if (existing is not null)
        {
            existing.UpdateLimit(limitAmount);
            await _budgetRepository.UpdateAsync(existing, cancellationToken);
            budget = existing;
        }
        else
        {
            budget = Budget.Create(
                request.UserId,
                request.Category,
                limitAmount,
                request.Month,
                request.Year);
            await _budgetRepository.AddAsync(budget, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(budget);
    }

    private static BudgetDto MapToDto(Budget budget) =>
        new(
            budget.Id,
            budget.UserId,
            budget.Category,
            budget.LimitAmount.Amount,
            budget.LimitAmount.Currency,
            budget.Month,
            budget.Year);
}
