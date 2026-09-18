using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyAi.Domain.Entities;

namespace MyAi.Infrastructure.Persistence.Configurations;

public class ConversationEntityConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(c => c.Title).HasMaxLength(200);
        builder.Property(c => c.MessageCount).HasColumnName("message_count");
        builder.Property(c => c.IsDeleted).HasColumnName("is_deleted");
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(c => c.UserId, "idx_conversations_user_id");
        builder.HasIndex(c => new { c.UserId, c.IsDeleted }, "idx_conversations_user_deleted");
        builder.HasIndex(c => c.CreatedAt, "idx_conversations_created_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global soft-delete filter.
        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.Ignore(c => c.DomainEvents);
    }
}
