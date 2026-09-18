namespace MyAi.Application.Common.Constants;

public static class PolicyConstants
{
    public const string AdminOnly = "RequireAdminRole";

    public const string SubscriberOrAdmin = "RequireSubscriberRole";

    public const string Authenticated = "RequireAuthenticatedUser";
}
