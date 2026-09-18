using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Common.Constants;
using MyAi.Application.Features.Avatar;

namespace MyAi.Application.Features.Avatar.Queries;

public record GetAvailableAvatarModelsQuery : IRequest<List<AvatarModelDto>>, ICacheable
{
    public string CacheKey => CacheKeys.AvatarModelsActive();

    public TimeSpan Duration => TimeSpan.FromMinutes(CacheKeys.CatalogTtlMinutes);
}
