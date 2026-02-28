using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.Events;

public sealed record TransactionCreatedDomainEvent(Guid TransactionId, Guid UserId) : IDomainEvent;
