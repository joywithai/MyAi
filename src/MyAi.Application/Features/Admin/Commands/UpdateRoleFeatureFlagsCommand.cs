using MediatR;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Features.Settings;
using MyAi.Domain.Enums;

namespace MyAi.Application.Features.Admin.Commands;

public record UpdateRoleFeatureFlagsCommand(
    string Role,
    bool CanUseCustomApiKey,
    bool CanAccessAllExpressions,
    bool CanAccessAllAnimations,
    bool CanSelectAvatarModel,
    bool CanCustomizeVoice,
    bool CanAccessChatHistory,
    int MaxConversationHistory,
    int MaxMessagesPerDay) : IRequest<RoleFeatureFlagsDto>, IRequireRole
{
    public UserRole MinimumRole => UserRole.Admin;
}
