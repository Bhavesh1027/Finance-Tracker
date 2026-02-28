using FinanceTracker.Domain.Common;
using MediatR;

namespace FinanceTracker.Domain.Events;

public sealed record TransactionCreatedDomainEvent(Guid TransactionId, Guid UserId) : IDomainEvent, INotification;
