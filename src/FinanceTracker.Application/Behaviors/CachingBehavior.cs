using FinanceTracker.Application.Abstractions;
using FinanceTracker.Application.Caching;
using MediatR;

namespace FinanceTracker.Application.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;

    public CachingBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheable cacheableRequest)
        {
            return await next();
        }

        var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheableRequest.CacheKey, cancellationToken);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        var response = await next();

        await _cacheService.SetAsync(cacheableRequest.CacheKey, response, cacheableRequest.CacheDuration, cancellationToken);

        return response;
    }
}

