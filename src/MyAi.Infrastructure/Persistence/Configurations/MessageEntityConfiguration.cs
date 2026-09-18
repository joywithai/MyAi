using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Domain.ValueObjects;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class MessageEntityConfiguration : IEntityTypeConfiguration<Message>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ConversationId).HasColumnName("conversation_id").IsRequired();
        builder.HasIndex(m => m.ConversationId, "idx_messages_conversation_id");
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt }, "idx_messages_conv_created");

        builder.Property(m => m.Role)
            .HasConversion(r => r == MessageRole.User ? "user" : "assistant",
                           v => v == "user" ? MessageRole.User : MessageRole.Assistant)
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(m => m.Role, "idx_messages_role");

        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.Script);

        builder.Property(m => m.Language)
            .HasConversion(l => l == Language.Bn ? "bn" : "en",
                           v => v == "bn" ? Language.Bn : Language.En)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.ExpressionSegments)
            .HasColumnName("expression_segments")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v is null ? null : JsonSerializer.Serialize(v, JsonOptions),
                v => v is null ? null : JsonSerializer.Deserialize<List<ExpressionSegment>>(v, JsonOptions));

        builder.Property(m => m.AudioUrl).HasMaxLength(1000).HasColumnName("audio_url");
        builder.Property(m => m.AudioDurationMs).HasColumnName("audio_duration_ms");
        builder.Property(m => m.AudioContentType).HasMaxLength(50).HasColumnName("audio_content_type");

        builder.Property(m => m.WordBoundaries)
            .HasColumnName("word_boundaries")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v is null ? null : JsonSerializer.Serialize(v, JsonOptions),
                v => v is null ? null : JsonSerializer.Deserialize<List<WordBoundary>>(v, JsonOptions));

        builder.Property(m => m.TokensUsed).HasColumnName("tokens_used");
        builder.Property(m => m.AiModelUsed).HasMaxLength(200).HasColumnName("ai_model_used");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");

        builder.Ignore(m => m.DomainEvents);
    }
}
