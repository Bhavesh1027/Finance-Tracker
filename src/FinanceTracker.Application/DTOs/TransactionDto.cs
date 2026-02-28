using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public sealed record TransactionDto(
    Guid Id,
    Guid UserId,
    decimal Amount,
    string Currency,
    Category Category,
    string Description,
    DateTime Date,
    TransactionType TransactionType);
