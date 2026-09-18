namespace MyAi.Application.Common.Constants;

public static class ErrorMessages
{
    public const string EmailAlreadyExists = "This email is already registered.";
    public const string InvalidCredentials = "Invalid email or password.";
    public const string AccountBanned = "Your account has been suspended.";
    public const string AccountInactive = "Your account is inactive.";
    public const string ConversationNotFound = "Conversation not found.";
    public const string ConversationNotOwned = "You do not have access to this conversation.";
    public const string DailyLimitReached = "Daily message limit reached. Upgrade to a subscription for more messages.";
    public const string FeatureLocked = "This feature is not available on your plan.";
    public const string AiUnavailable = "The AI service is currently unavailable. Please try again shortly.";
    public const string TtsUnavailable = "Speech generation failed. Showing text response only.";
    public const string AvatarModelNotFound = "Avatar model not found.";
    public const string AvatarModelLocked = "This avatar model requires a higher plan.";
    public const string CannotChangeOwnRole = "Admins cannot change their own role.";
    public const string InvalidRefreshToken = "Invalid or expired refresh token.";
}
