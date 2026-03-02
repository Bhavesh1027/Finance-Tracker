using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FinanceTracker.API.Health;

public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer? _connectionMultiplexer;

    public RedisHealthCheck(IConnectionMultiplexer? connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_connectionMultiplexer is null)
        {
            return HealthCheckResult.Healthy("Redis not configured.");
        }

        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            _ = await db.PingAsync();
            return HealthCheckResult.Healthy("Redis ping successful.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis ping failed.", ex);
        }
    }
}

