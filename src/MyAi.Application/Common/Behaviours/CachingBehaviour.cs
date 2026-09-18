using MediatR;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Application.Common.Behaviours;

/// <summary>Cache envelope so value-type handler results can pass the class-constrained cache.</summary>
public sealed class CacheBox
{
    public object? Value { get; init; }
}

/// <summary>MediatR pipeline behaviour: cache-aside for requests implementing ICacheable.</summary>
public class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cacheService;

    public CachingBehaviour(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheable cacheable)
        {
            return await next();
        }

        var cached = await _cacheService.GetAsync<CacheBox>(cacheable.CacheKey, cancellationToken);

        if (cached?.Value is TResponse hit)
        {
            return hit;
        }

        var response = await next();

        await _cacheService.SetAsync(
            cacheable.CacheKey, new CacheBox { Value = response }, cacheable.Duration, cancellationToken);

        return response;
    }
}
