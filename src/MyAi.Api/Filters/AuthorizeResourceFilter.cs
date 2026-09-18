using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Api.Filters;

/// <summary>
/// Resource ownership guard for conversation routes ({id} must belong to the caller;
/// admins bypass). Applied to Conversation/Message endpoints.
/// </summary>
public class AuthorizeResourceFilter : IAsyncAuthorizationFilter
{
    private readonly IConversationOwnershipChecker _ownershipChecker;

    public AuthorizeResourceFilter(IConversationOwnershipChecker ownershipChecker)
    {
        _ownershipChecker = ownershipChecker;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            return; // [Authorize] handles authentication.
        }

        var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (string.Equals(role, RoleConstants.Admin, StringComparison.OrdinalIgnoreCase))
        {
            return; // Admins bypass ownership checks.
        }

        var routeValues = context.RouteData.Values;

        if (!routeValues.TryGetValue("conversationId", out var rawId)
            && !routeValues.TryGetValue("id", out rawId))
        {
            return;
        }

        if (!Guid.TryParse(rawId?.ToString(), out var resourceId))
        {
            return;
        }

        var userIdValue = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var isOwner = await _ownershipChecker.OwnsConversationAsync(userId, resourceId);

        if (!isOwner)
        {
            context.Result = new ObjectResult(new
            {
                error = true,
                message = "You do not have access to this resource."
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
