using System.Security.Claims;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;

namespace MyAi.Infrastructure.Identity;

/// <summary>Extracts current user info from JWT claims on the HttpContext.</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? Principal?.FindFirst("sub")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Role => Principal?.FindFirst(ClaimTypes.Role)?.Value
                           ?? Principal?.FindFirst("role")?.Value;

    public string? Email => Principal?.FindFirst(ClaimTypes.Email)?.Value
                            ?? Principal?.FindFirst("email")?.Value;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true && UserId.HasValue;

    public Guid GetUserId() =>
        UserId ?? throw new UnauthorizedException("Authentication required.");

    public string GetUserRole() => Role ?? RoleConstants.PublicUser;

    public string GetUserEmail() => Email ?? string.Empty;

    public string? GetIpAddress() =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? GetUserAgent() =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
