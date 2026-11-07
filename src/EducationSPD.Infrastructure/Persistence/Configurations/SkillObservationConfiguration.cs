using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationSPD.Infrastructure.Persistence.Configurations;

internal sealed class SkillObservationConfiguration : IEntityTypeConfiguration<SkillObservation>
{
    public void Configure(EntityTypeBuilder<SkillObservation> builder)
    {
        builder.ToTable(
            "SkillObservations",
            table =>
            {
                table.HasCheckConstraint("CK_SkillObservations_Source", "source IN (1, 2)");
                table.HasCheckConstraint("CK_SkillObservations_Outcome", "outcome IN (1, 2)");
            });
        builder.HasKey(observation => observation.Id);

        builder.Property(observation => observation.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(observation => observation.LearningEpisodeId)
            .HasColumnName("learning_episode_id")
            .IsRequired();
        builder.Property(observation => observation.SkillId)
            .HasColumnName("skill_id")
            .IsRequired();
        builder.Property(observation => observation.Source)
            .HasColumnName("source")
            .HasConversion<short>()
            .IsRequired();
        builder.Property(observation => observation.Outcome)
            .HasColumnName("outcome")
            .HasConversion<short>()
            .IsRequired();

        builder.HasOne<Skill>()
            .WithMany()
            .HasForeignKey(observation => observation.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(observation => new { observation.LearningEpisodeId, observation.SkillId })
            .IsUnique()
            .HasDatabaseName("UX_SkillObservations_EpisodeId_SkillId");
    }
}
