using FinanceTracker.Application.DTOs;
using MediatR;

namespace FinanceTracker.Application.Queries.Transactions;

public sealed record GetTransactionsByMonthQuery(
    Guid UserId,
    int Month,
    int Year) : IRequest<List<TransactionDto>>;
