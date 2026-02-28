using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Repositories;
using MediatR;

namespace FinanceTracker.Application.Queries.Transactions;

public sealed class GetMonthlySummaryQueryHandler : IRequestHandler<GetMonthlySummaryQuery, MonthlySummaryDto>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetMonthlySummaryQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<MonthlySummaryDto> Handle(GetMonthlySummaryQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByUserAndMonthAsync(
            request.UserId,
            request.Month,
            request.Year,
            cancellationToken);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount.Amount);

        var netBalance = totalIncome - totalExpenses;

        var breakdownByCategory = transactions
            .GroupBy(t => t.Category)
            .Select(g => new CategorySummaryDto(
                g.Key,
                g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount.Amount),
                g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount.Amount)))
            .OrderBy(c => c.Category)
            .ToList();

        return new MonthlySummaryDto(totalIncome, totalExpenses, netBalance, breakdownByCategory);
    }
}
