using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class UserSettingsEntityConfiguration : IEntityTypeConfiguration<UserSettings>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings");

        builder.HasKey(s => s.UserId);

        builder.Property(s => s.UserId).HasColumnName("user_id");

        builder.Property(s => s.AvatarModelId).HasColumnName("avatar_model_id");

        builder.Property(s => s.PreferredLanguage)
            .HasColumnName("preferred_language")
            .HasMaxLength(10)
            .HasConversion(l => l == Language.Bn ? "bn" : "en",
                           v => v == "bn" ? Language.Bn : Language.En)
            .IsRequired();

        builder.Property(s => s.VoiceName).HasColumnName("voice_name").HasMaxLength(100).IsRequired();
        builder.Property(s => s.VoiceSpeed).HasColumnName("voice_speed").HasColumnType("numeric(3,2)").IsRequired();
        builder.Property(s => s.VoicePitch).HasColumnName("voice_pitch").IsRequired();

        builder.Property(s => s.DefaultExpression).HasColumnName("default_expression").HasMaxLength(20).IsRequired();
        builder.Property(s => s.ThemePreference).HasColumnName("theme_preference").HasMaxLength(20).IsRequired();
        builder.Property(s => s.ShowSubtitles).HasColumnName("show_subtitles").IsRequired();
        builder.Property(s => s.AutoPlayAudio).HasColumnName("auto_play_audio").IsRequired();

        builder.Property(s => s.EnabledAnimations)
            .HasColumnName("enabled_animations")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>())
            .IsRequired();

        builder.Property(s => s.BlinkEnabled).HasColumnName("blink_enabled").IsRequired();
        builder.Property(s => s.BlinkFrequency).HasColumnName("blink_frequency").HasMaxLength(20).IsRequired();
        builder.Property(s => s.ThinkingPoseEnabled).HasColumnName("thinking_pose_enabled").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<UserSettings>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AvatarModel>()
            .WithMany()
            .HasForeignKey(s => s.AvatarModelId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(s => s.DomainEvents);
    }
}
