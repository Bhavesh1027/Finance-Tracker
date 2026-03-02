using System.Threading;
using System.Threading.Tasks;
using FinanceTracker.Application.Abstractions;
using MassTransit;

namespace FinanceTracker.Infrastructure.Messaging;

public sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : class
    {
        return _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}

