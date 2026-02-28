using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Application.DTOs;

public sealed record CategorySummaryDto(
    Category Category,
    decimal Income,
    decimal Expenses);
