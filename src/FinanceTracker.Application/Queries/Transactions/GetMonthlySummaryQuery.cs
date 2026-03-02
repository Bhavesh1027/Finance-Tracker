using System;
using FinanceTracker.Application.Caching;
using FinanceTracker.Application.DTOs;
using MediatR;

namespace FinanceTracker.Application.Queries.Transactions;

public sealed record GetMonthlySummaryQuery(
    Guid UserId,
    int Month,
    int Year) : IRequest<MonthlySummaryDto>, ICacheable
{
    public string CacheKey => $"summary:{UserId}:{Month}:{Year}";

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
