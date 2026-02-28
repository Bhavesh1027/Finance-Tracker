using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Enums;
using MediatR;

namespace FinanceTracker.Application.Commands.Transactions;

public sealed record CreateTransactionCommand(
    Guid UserId,
    decimal Amount,
    string Currency,
    Category Category,
    string Description,
    DateTime Date,
    TransactionType TransactionType) : IRequest<TransactionDto>;
