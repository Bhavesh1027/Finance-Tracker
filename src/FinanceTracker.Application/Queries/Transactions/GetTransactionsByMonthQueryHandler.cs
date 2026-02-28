using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Repositories;
using MediatR;

namespace FinanceTracker.Application.Queries.Transactions;

public sealed class GetTransactionsByMonthQueryHandler : IRequestHandler<GetTransactionsByMonthQuery, List<TransactionDto>>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsByMonthQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<List<TransactionDto>> Handle(GetTransactionsByMonthQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByUserAndMonthAsync(
            request.UserId,
            request.Month,
            request.Year,
            cancellationToken);

        return transactions
            .Select(MapToDto)
            .ToList();
    }

    private static TransactionDto MapToDto(Domain.Entities.Transaction t) =>
        new(
            t.Id,
            t.UserId,
            t.Amount.Amount,
            t.Amount.Currency,
            t.Category,
            t.Description,
            t.Date,
            t.Type);
}
