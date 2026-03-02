using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinanceTracker.API.Health;

public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IBus? _bus;

    public RabbitMqHealthCheck(IBus? bus)
    {
        _bus = bus;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_bus is null)
        {
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ not configured."));
        }

        try
        {
            // Probe the bus topology; if this throws, connection is unhealthy
            var _ = _bus.GetProbeResult();
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ bus is healthy."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ bus probe failed.", ex));
        }
    }
}

