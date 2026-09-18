namespace MyAi.Application.Common.Constants;

/// <summary>Redis cache key templates (mirrors section 5.4 of the architecture doc).</summary>
public static class CacheKeys
{
    public const int SessionTtlMinutes = 30;
    public const int SettingsTtlMinutes = 15;
    public const int FeatureFlagsTtlMinutes = 60;
    public const int CatalogTtlMinutes = 60;
    public const int ConversationsTtlMinutes = 5;

    public static string Session(Guid userId) => $"session:{userId}";

    public static string Settings(Guid userId) => $"settings:{userId}";

    public static string FeatureFlags(string role) => $"feature_flags:{role}";

    public static string AvatarModelsActive() => "avatar_models:active";

    public static string Expressions(string role) => $"expressions:{role}";

    public static string Animations(string role) => $"animations:{role}";

    public static string SubscriptionPlansActive() => "subscription_plans:active";

    public static string ConversationsPage(Guid userId, int page) => $"conversations:{userId}:page:{page}";

    public static string SystemSettings() => "system_settings";

    public static string User(Guid userId) => $"user:{userId}";
}
