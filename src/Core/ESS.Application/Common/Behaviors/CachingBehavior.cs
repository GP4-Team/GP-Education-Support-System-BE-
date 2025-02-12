using ESS.Application.Common.Caching;
using ESS.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ESS.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheKey cacheKey)
        {
            // Request doesn't implement ICacheKey, skip caching
            return await next();
        }

        var cachedResponse = await _cache.GetAsync<TResponse>(cacheKey.CacheKey);
        if (cachedResponse != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey.CacheKey);
            return cachedResponse;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey.CacheKey);
        var response = await next();

        if (response != null)
        {
            await _cache.SetAsync(
                cacheKey.CacheKey,
                response,
                cacheKey.Expiration ?? CacheConfiguration.Tenants.DefaultExpiration);
        }

        return response;
    }
}