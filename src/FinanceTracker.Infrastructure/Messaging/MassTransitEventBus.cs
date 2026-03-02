using System.Threading;
using System.Threading.Tasks;
using FinanceTracker.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Infrastructure.Messaging;

public sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventBus> _logger;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint, ILogger<MassTransitEventBus> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class
    {
        var eventType = typeof(T).Name;
        var userId = integrationEvent?.GetType().GetProperty("UserId")?.GetValue(integrationEvent);

        _logger.LogInformation(
            "MassTransit event published {EventType} for UserId={UserId}",
            eventType,
            userId);

        return _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}

