using EducationSPD.Domain.Models;
using EducationSPD.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationSPD.Infrastructure.Persistence.Configurations;

internal sealed class StudentSkillStateConfiguration : IEntityTypeConfiguration<StudentSkillState>
{
    public void Configure(EntityTypeBuilder<StudentSkillState> builder)
    {
        builder.ToTable(
            "StudentSkillStates",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_StudentSkillStates_MasteryProbability",
                    "mastery_probability BETWEEN 0 AND 1");
                table.HasCheckConstraint(
                    "CK_StudentSkillStates_ObservationsCount",
                    "observations_count > 0");
            });

        builder.HasKey(state => new
        {
            state.StudentId,
            state.ModuleKey,
            state.SkillId,
            state.ModelVersion,
        });
        builder.Property(state => state.StudentId).HasColumnName("student_id");
        builder.Property(state => state.ModuleKey)
            .HasColumnName("module_key")
            .HasMaxLength(Skill.ModuleKeyMaxLength);
        builder.Property(state => state.SkillId).HasColumnName("skill_id");
        builder.Property(state => state.ModelVersion).HasColumnName("model_version");
        builder.Property(state => state.MasteryProbability)
            .HasColumnName("mastery_probability")
            .HasPrecision(18, 16);
        builder.Property(state => state.ObservationsCount).HasColumnName("observations_count");
        builder.Property(state => state.LastObservedAt).HasColumnName("last_observed_at");
        builder.Property(state => state.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne<Skill>()
            .WithMany()
            .HasForeignKey(state => state.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ModelVersion>()
            .WithMany()
            .HasPrincipalKey(version => version.Version)
            .HasForeignKey(state => state.ModelVersion)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
