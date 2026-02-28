using FinanceTracker.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.EventHandlers;

public sealed class TransactionCreatedDomainEventHandler : INotificationHandler<TransactionCreatedDomainEvent>
{
    private readonly ILogger<TransactionCreatedDomainEventHandler> _logger;

    public TransactionCreatedDomainEventHandler(ILogger<TransactionCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TransactionCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Transaction created: TransactionId={TransactionId}, UserId={UserId}",
            notification.TransactionId,
            notification.UserId);

        return Task.CompletedTask;
    }
}
