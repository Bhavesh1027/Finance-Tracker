namespace FinanceTracker.Application.Events;

public sealed record TransactionCreatedIntegrationEvent(
    Guid TransactionId,
    Guid UserId,
    decimal Amount,
    string Category);

