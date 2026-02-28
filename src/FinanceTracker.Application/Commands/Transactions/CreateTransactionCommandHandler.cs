using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Repositories;
using FinanceTracker.Domain.ValueObjects;
using MediatR;

namespace FinanceTracker.Application.Commands.Transactions;

public sealed class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IBudgetRepository budgetRepository,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _budgetRepository = budgetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var amount = Money.Create(request.Amount, request.Currency);
        var transaction = Transaction.Create(
            request.UserId,
            amount,
            request.Category,
            request.Description,
            request.Date,
            request.TransactionType);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Check budget for expenses only
        if (request.TransactionType == TransactionType.Expense)
        {
            var budget = await _budgetRepository.GetByUserCategoryMonthAsync(
                request.UserId,
                request.Category,
                request.Date.Month,
                request.Date.Year,
                cancellationToken);

            if (budget is not null)
            {
                var spent = await _transactionRepository.GetTotalByUserAndCategoryAsync(
                    request.UserId,
                    request.Category,
                    request.Date.Month,
                    request.Date.Year,
                    cancellationToken);

                budget.CheckBudgetExceeded(spent);
            }
        }

        return MapToDto(transaction);
    }

    private static TransactionDto MapToDto(Transaction transaction) =>
        new(
            transaction.Id,
            transaction.UserId,
            transaction.Amount.Amount,
            transaction.Amount.Currency,
            transaction.Category,
            transaction.Description,
            transaction.Date,
            transaction.Type);
}
