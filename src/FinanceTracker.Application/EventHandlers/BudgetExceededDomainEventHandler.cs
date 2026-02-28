using FinanceTracker.Application.Abstractions;
using FinanceTracker.Application.Events;
using FinanceTracker.Domain.Events;
using MediatR;

namespace FinanceTracker.Application.EventHandlers;

public sealed class BudgetExceededDomainEventHandler : INotificationHandler<BudgetExceededDomainEvent>
{
    private readonly IEventBus _eventBus;

    public BudgetExceededDomainEventHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Handle(BudgetExceededDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new BudgetExceededIntegrationEvent(
            notification.UserId,
            notification.Category,
            notification.Spent,
            notification.Limit);

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
