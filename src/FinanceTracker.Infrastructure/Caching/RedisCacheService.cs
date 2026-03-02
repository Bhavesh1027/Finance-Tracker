using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FinanceTracker.Application.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FinanceTracker.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _database = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            _logger.LogDebug("Cache miss for key {CacheKey}", key);
            return default;
        }

        _logger.LogDebug("Cache hit for key {CacheKey}", key);

        return JsonSerializer.Deserialize<T>(value!, SerializerOptions);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, SerializerOptions);
        return _database.StringSetAsync(key, json, expiry);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        foreach (var endPoint in _connectionMultiplexer.GetEndPoints())
        {
            var server = _connectionMultiplexer.GetServer(endPoint);

            if (!server.IsConnected)
            {
                continue;
            }

            var keys = server.Keys(_database.Database, pattern: pattern);

            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key).ConfigureAwait(false);
            }
        }
    }
}

