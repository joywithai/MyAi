using MyAi.Domain.Entities;

namespace MyAi.Application.Common.Interfaces;

/// <summary>Abstraction over the HTTP principal (JWT claims).</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    string? Role { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    Guid GetUserId();

    string GetUserRole();

    string GetUserEmail();

    string? GetIpAddress();

    string? GetUserAgent();
}
