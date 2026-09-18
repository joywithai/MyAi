using MyAi.Domain.Enums;

namespace MyAi.Application.Common.Behaviours;

/// <summary>Marker interface: requests implementing this are role-checked by AuthorizationBehaviour.</summary>
public interface IRequireRole
{
    UserRole MinimumRole { get; }
}
