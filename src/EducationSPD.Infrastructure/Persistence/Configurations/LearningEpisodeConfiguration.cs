using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Models;
using EducationSPD.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationSPD.Infrastructure.Persistence.Configurations;

internal sealed class LearningEpisodeConfiguration : IEntityTypeConfiguration<LearningEpisode>
{
    public void Configure(EntityTypeBuilder<LearningEpisode> builder)
    {
        builder.ToTable("LearningEpisodes");
        builder.HasKey(episode => episode.Id);

        builder.Property(episode => episode.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(episode => episode.EventId)
            .HasColumnName("event_id")
            .IsRequired();
        builder.Property(episode => episode.EpisodeId)
            .HasColumnName("episode_id")
            .IsRequired();
        builder.Property(episode => episode.StudentId)
            .HasColumnName("student_id")
            .IsRequired();
        builder.Property(episode => episode.ModuleKey)
            .HasColumnName("module_key")
            .HasMaxLength(Skill.ModuleKeyMaxLength)
            .IsRequired();
        builder.Property(episode => episode.TaskId)
            .HasColumnName("task_id")
            .IsRequired();
        builder.Property(episode => episode.FinalizedAt)
            .HasColumnName("finalized_at")
            .IsRequired();
        builder.Property(episode => episode.AppliedModelVersion)
            .HasColumnName("applied_model_version")
            .IsRequired();

        builder.HasOne<ModelVersion>()
            .WithMany()
            .HasPrincipalKey(version => version.Version)
            .HasForeignKey(episode => episode.AppliedModelVersion)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(episode => episode.Observations)
            .WithOne()
            .HasForeignKey(observation => observation.LearningEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(episode => episode.EventId)
            .IsUnique()
            .HasDatabaseName("UX_LearningEpisodes_EventId");
        builder.HasIndex(episode => new { episode.ModuleKey, episode.EpisodeId })
            .IsUnique()
            .HasDatabaseName("UX_LearningEpisodes_ModuleKey_EpisodeId");
        builder.HasIndex(episode => new { episode.StudentId, episode.FinalizedAt })
            .HasDatabaseName("IX_LearningEpisodes_StudentId_FinalizedAt");
    }
}
