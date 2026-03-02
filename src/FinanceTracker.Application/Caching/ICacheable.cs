using System;

namespace FinanceTracker.Application.Caching;

public interface ICacheable
{
    string CacheKey { get; }

    TimeSpan CacheDuration { get; }
}

