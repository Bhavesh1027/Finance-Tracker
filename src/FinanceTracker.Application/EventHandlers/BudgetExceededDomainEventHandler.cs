using FinanceTracker.Application.Abstractions;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Events;
using FinanceTracker.Domain.Events;
using MediatR;

namespace FinanceTracker.Application.EventHandlers;

public sealed class BudgetExceededDomainEventHandler : INotificationHandler<BudgetExceededDomainEvent>
{
    private readonly IEventBus _eventBus;
    private readonly IBudgetNotificationService _notificationService;

    public BudgetExceededDomainEventHandler(IEventBus eventBus, IBudgetNotificationService notificationService)
    {
        _eventBus = eventBus;
        _notificationService = notificationService;
    }

    public async Task Handle(BudgetExceededDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new BudgetExceededIntegrationEvent(
            notification.UserId,
            notification.Category,
            notification.Spent,
            notification.Limit);

        await _eventBus.PublishAsync(integrationEvent, cancellationToken);

        var now = DateTime.UtcNow;
        var alertDto = new BudgetAlertDto(
            notification.UserId,
            notification.Category,
            notification.Spent,
            notification.Limit,
            now.Month,
            now.Year);

        await _notificationService.SendBudgetAlertAsync(notification.UserId, alertDto, cancellationToken);
    }
}
