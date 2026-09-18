using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Application.Common.Behaviours;

/// <summary>MediatR pipeline behaviour: enforces IRequireRole minimum role.</summary>
public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUserService;

    public AuthorizationBehaviour(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IRequireRole requireRole)
        {
            if (!_currentUserService.IsAuthenticated)
            {
                throw new UnauthorizedException();
            }

            var currentRole = RoleConstants.ToUserRole(_currentUserService.GetUserRole());

            if (RoleConstants.Rank(currentRole) < RoleConstants.Rank(requireRole.MinimumRole))
            {
                throw new ForbiddenException("Your role does not allow this operation.");
            }
        }

        return next();
    }
}
