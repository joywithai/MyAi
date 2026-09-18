namespace MyAi.Application.Common.Constants;

public static class RoleConstants
{
    public const string Admin = "admin";

    public const string Subscriber = "subscriber";

    public const string PublicUser = "public_user";

    public static string ToRoleString(Domain.Enums.UserRole role) => role switch
    {
        Domain.Enums.UserRole.Admin => Admin,
        Domain.Enums.UserRole.Subscriber => Subscriber,
        _ => PublicUser
    };

    public static Domain.Enums.UserRole ToUserRole(string role) => role?.ToLowerInvariant() switch
    {
        Admin => Domain.Enums.UserRole.Admin,
        Subscriber => Domain.Enums.UserRole.Subscriber,
        _ => Domain.Enums.UserRole.PublicUser
    };

    /// <summary>Role hierarchy rank — higher wins.</summary>
    public static int Rank(Domain.Enums.UserRole role) => (int)role;
}
