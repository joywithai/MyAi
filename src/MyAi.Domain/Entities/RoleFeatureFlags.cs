using MyAi.Domain.Enums;
using MyAi.Domain.Interfaces;

namespace MyAi.Domain.Entities;

public class RoleFeatureFlags : Entity, IAuditableEntity
{
    private RoleFeatureFlags() { } // EF Core

    private RoleFeatureFlags(
        UserRole role,
        bool canUseCustomApiKey,
        bool canAccessAllExpressions,
        bool canAccessAllAnimations,
        bool canSelectAvatarModel,
        bool canCustomizeVoice,
        bool canAccessChatHistory,
        int maxConversationHistory,
        int maxMessagesPerDay)
    {
        Role = role;
        CanUseCustomApiKey = canUseCustomApiKey;
        CanAccessAllExpressions = canAccessAllExpressions;
        CanAccessAllAnimations = canAccessAllAnimations;
        CanSelectAvatarModel = canSelectAvatarModel;
        CanCustomizeVoice = canCustomizeVoice;
        CanAccessChatHistory = canAccessChatHistory;
        MaxConversationHistory = maxConversationHistory;
        MaxMessagesPerDay = maxMessagesPerDay;
        UpdatedAt = DateTime.UtcNow;
    }

    public UserRole Role { get; private set; }

    public bool CanUseCustomApiKey { get; private set; }

    public bool CanAccessAllExpressions { get; private set; }

    public bool CanAccessAllAnimations { get; private set; }

    public bool CanSelectAvatarModel { get; private set; }

    public bool CanCustomizeVoice { get; private set; }

    public bool CanAccessChatHistory { get; private set; }

    /// <summary>-1 means unlimited.</summary>
    public int MaxConversationHistory { get; private set; }

    /// <summary>-1 means unlimited.</summary>
    public int MaxMessagesPerDay { get; private set; }

    public DateTime UpdatedAt { get; set; }

    public static RoleFeatureFlags CreateDefault(UserRole role) => role switch
    {
        UserRole.Admin => new RoleFeatureFlags(
            role,
            canUseCustomApiKey: true,
            canAccessAllExpressions: true,
            canAccessAllAnimations: true,
            canSelectAvatarModel: true,
            canCustomizeVoice: true,
            canAccessChatHistory: true,
            maxConversationHistory: -1,
            maxMessagesPerDay: -1),
        UserRole.Subscriber => new RoleFeatureFlags(
            role,
            canUseCustomApiKey: true,
            canAccessAllExpressions: true,
            canAccessAllAnimations: true,
            canSelectAvatarModel: true,
            canCustomizeVoice: true,
            canAccessChatHistory: true,
            maxConversationHistory: -1,
            maxMessagesPerDay: 500),
        _ => new RoleFeatureFlags(
            UserRole.PublicUser,
            canUseCustomApiKey: false,
            canAccessAllExpressions: false,
            canAccessAllAnimations: false,
            canSelectAvatarModel: false,
            canCustomizeVoice: false,
            canAccessChatHistory: true,
            maxConversationHistory: 10,
            maxMessagesPerDay: 50)
    };

    public void Update(
        bool canUseCustomApiKey,
        bool canAccessAllExpressions,
        bool canAccessAllAnimations,
        bool canSelectAvatarModel,
        bool canCustomizeVoice,
        bool canAccessChatHistory,
        int maxConversationHistory,
        int maxMessagesPerDay)
    {
        CanUseCustomApiKey = canUseCustomApiKey;
        CanAccessAllExpressions = canAccessAllExpressions;
        CanAccessAllAnimations = canAccessAllAnimations;
        CanSelectAvatarModel = canSelectAvatarModel;
        CanCustomizeVoice = canCustomizeVoice;
        CanAccessChatHistory = canAccessChatHistory;
        MaxConversationHistory = maxConversationHistory;
        MaxMessagesPerDay = maxMessagesPerDay;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsFeatureEnabled(string featureName) => featureName switch
    {
        "canUseCustomApiKey" => CanUseCustomApiKey,
        "canAccessAllExpressions" => CanAccessAllExpressions,
        "canAccessAllAnimations" => CanAccessAllAnimations,
        "canSelectAvatarModel" => CanSelectAvatarModel,
        "canCustomizeVoice" => CanCustomizeVoice,
        "canAccessChatHistory" => CanAccessChatHistory,
        _ => false
    };
}
