using AutoMapper;
using MyAi.Application.Features.Ai;
using MyAi.Application.Features.Avatar;
using MyAi.Application.Features.Conversation;
using MyAi.Application.Features.Settings;
using MyAi.Application.Features.Subscription;
using MyAi.Application.Features.User;
using MyAi.Application.Features.Admin;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;

namespace MyAi.Application.Common.Mapping;

/// <summary>Entity → DTO mapping profiles (AutoMapper).</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => RoleConstants.ToRoleString(s.Role)))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString().ToLowerInvariant()));

        CreateMap<Conversation, ConversationDto>();

        CreateMap<Message, MessageDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role == MessageRole.User ? "user" : "assistant"))
            .ForMember(d => d.Language, o => o.MapFrom(s => s.Language == Language.Bn ? "bn" : "en"))
            .ForMember(d => d.ExpressionSegments, o => o.MapFrom(s =>
                s.ExpressionSegments == null
                    ? null
                    : s.ExpressionSegments.Select(x => new ExpressionSegmentDto(x.Expression, x.Text)).ToList()));

        CreateMap<AvatarModel, AvatarModelDto>()
            .ForMember(d => d.MinRole, o => o.MapFrom(s => RoleConstants.ToRoleString(s.MinRole)));

        CreateMap<Expression, ExpressionDto>()
            .ForMember(d => d.MinRole, o => o.MapFrom(s => RoleConstants.ToRoleString(s.MinRole)));

        CreateMap<Animation, AnimationDto>()
            .ForMember(d => d.MinRole, o => o.MapFrom(s => RoleConstants.ToRoleString(s.MinRole)));

        CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
            .ForMember(d => d.RoleGranted, o => o.MapFrom(s => RoleConstants.ToRoleString(s.RoleGranted)))
            .ForMember(d => d.BillingCycle, o => o.MapFrom(s => s.BillingCycle == BillingCycle.Yearly ? "yearly" : "monthly"));

        CreateMap<UserSubscription, UserSubscriptionDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString().ToLowerInvariant()));

        CreateMap<RoleFeatureFlags, RoleFeatureFlagsDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => RoleConstants.ToRoleString(s.Role)));

        CreateMap<SystemSetting, SystemSettingDto>();

        CreateMap<UserSettings, UserSettingsDto>()
            .ForMember(d => d.PreferredLanguage, o => o.MapFrom(s => s.PreferredLanguage == Language.Bn ? "bn" : "en"))
            .ForMember(d => d.DefaultExpression, o => o.MapFrom(s => s.DefaultExpression))
            .ForMember(d => d.SelectedAvatarModelId, o => o.MapFrom(s => s.AvatarModelId));

        CreateMap<User, AdminUserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => RoleConstants.ToRoleString(s.Role)))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString().ToLowerInvariant()));

        CreateMap<AuditLog, AuditLogDto>();
    }
}
