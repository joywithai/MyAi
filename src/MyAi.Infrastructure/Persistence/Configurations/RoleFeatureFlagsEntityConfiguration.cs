using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class RoleFeatureFlagsEntityConfiguration : IEntityTypeConfiguration<RoleFeatureFlags>
{
    public void Configure(EntityTypeBuilder<RoleFeatureFlags> builder)
    {
        builder.ToTable("role_feature_flags");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Role)
            .HasMaxLength(20)
            .HasConversion(
                r => r.ToString().ToLowerInvariant(),
                v => UserEntityConfiguration.ParseRole(v))
            .IsRequired();

        builder.HasIndex(f => f.Role).IsUnique();

        builder.Property(f => f.CanUseCustomApiKey).HasColumnName("can_use_custom_api_key");
        builder.Property(f => f.CanAccessAllExpressions).HasColumnName("can_access_all_expressions");
        builder.Property(f => f.CanAccessAllAnimations).HasColumnName("can_access_all_animations");
        builder.Property(f => f.CanSelectAvatarModel).HasColumnName("can_select_avatar_model");
        builder.Property(f => f.CanCustomizeVoice).HasColumnName("can_customize_voice");
        builder.Property(f => f.CanAccessChatHistory).HasColumnName("can_access_chat_history");
        builder.Property(f => f.MaxConversationHistory).HasColumnName("max_conversation_history");
        builder.Property(f => f.MaxMessagesPerDay).HasColumnName("max_messages_per_day");
        builder.Property(f => f.UpdatedAt).HasColumnName("updated_at");

        builder.Ignore(f => f.DomainEvents);
    }
}
