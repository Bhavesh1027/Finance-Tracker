using FinanceTracker.Application.DTOs;
using MediatR;

namespace FinanceTracker.Application.Queries.Transactions;

public sealed record GetMonthlySummaryQuery(
    Guid UserId,
    int Month,
    int Year) : IRequest<MonthlySummaryDto>;
