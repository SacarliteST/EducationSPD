using EducationSPD.Domain.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationSPD.Infrastructure.Persistence.Configurations;

internal sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable(
            "InboxMessages",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_InboxMessages_MessageType",
                    "message_type IN (1, 2)");
                table.HasCheckConstraint(
                    "CK_InboxMessages_Status",
                    "status IN (1, 2, 3, 4)");
                table.HasCheckConstraint(
                    "CK_InboxMessages_SchemaVersion",
                    "schema_version > 0");
                table.HasCheckConstraint(
                    "CK_InboxMessages_RetryCount",
                    "retry_count >= 0");
            });

        builder.HasKey(message => message.EventId);

        builder.Property(message => message.EventId)
            .HasColumnName("event_id")
            .ValueGeneratedNever();

        builder.Property(message => message.MessageType)
            .HasColumnName("message_type")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(message => message.SchemaVersion)
            .HasColumnName("schema_version")
            .IsRequired();

        builder.Property(message => message.MessageKey)
            .HasColumnName("message_key")
            .HasMaxLength(InboxMessage.MessageKeyMaxLength)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.PayloadHash)
            .HasColumnName("payload_hash")
            .HasMaxLength(InboxMessage.PayloadHashLength)
            .IsRequired();

        builder.Property(message => message.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(message => message.ErrorCode)
            .HasColumnName("error_code")
            .HasMaxLength(InboxMessage.ErrorCodeMaxLength);

        builder.Property(message => message.ReceivedAt)
            .HasColumnName("received_at")
            .IsRequired();

        builder.Property(message => message.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(message => message.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired();

        builder.Property(message => message.NextRetryAt)
            .HasColumnName("next_retry_at");

        builder.HasIndex(message => new
        {
            message.Status,
            message.NextRetryAt,
            message.ReceivedAt,
            message.EventId
        })
            .HasDatabaseName("IX_InboxMessages_ReadyBatch");
    }
}
