using EducationSPD.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationSPD.Infrastructure.Persistence.Configurations;

internal sealed class ModelVersionConfiguration : IEntityTypeConfiguration<ModelVersion>
{
    public void Configure(EntityTypeBuilder<ModelVersion> builder)
    {
        builder.ToTable(
            "ModelVersions",
            table =>
            {
                table.HasCheckConstraint("CK_ModelVersions_Status", "status IN (1, 2, 3, 4)");
                table.HasCheckConstraint("CK_ModelVersions_Version", "version > 0");
                table.HasCheckConstraint(
                    "CK_ModelVersions_Probabilities",
                    "initial_mastery_probability BETWEEN 0 AND 1 " +
                    "AND learn_probability BETWEEN 0 AND 1 " +
                    "AND validation_rule_guess_probability BETWEEN 0 AND 1 " +
                    "AND validation_rule_slip_probability BETWEEN 0 AND 1 " +
                    "AND task_tag_guess_probability BETWEEN 0 AND 1 " +
                    "AND task_tag_slip_probability BETWEEN 0 AND 1 " +
                    "AND validation_rule_guess_probability + validation_rule_slip_probability < 1 " +
                    "AND task_tag_guess_probability + task_tag_slip_probability < 1 " +
                    "AND validation_rule_guess_probability <= task_tag_guess_probability " +
                    "AND validation_rule_slip_probability <= task_tag_slip_probability");
                table.HasCheckConstraint("CK_ModelVersions_AhpMatrixSize", "cardinality(ahp_matrix) = 16");
                table.HasCheckConstraint("CK_ModelVersions_AhpWeightsSize", "cardinality(ahp_weights) = 4");
            });

        builder.HasKey(version => version.Id);
        builder.Property(version => version.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(version => version.Version).HasColumnName("version").IsRequired();
        builder.Property(version => version.InitialMasteryProbability).HasColumnName("initial_mastery_probability").HasPrecision(10, 9);
        builder.Property(version => version.LearnProbability).HasColumnName("learn_probability").HasPrecision(10, 9);
        builder.Property(version => version.ValidationRuleGuessProbability).HasColumnName("validation_rule_guess_probability").HasPrecision(10, 9);
        builder.Property(version => version.ValidationRuleSlipProbability).HasColumnName("validation_rule_slip_probability").HasPrecision(10, 9);
        builder.Property(version => version.TaskTagGuessProbability).HasColumnName("task_tag_guess_probability").HasPrecision(10, 9);
        builder.Property(version => version.TaskTagSlipProbability).HasColumnName("task_tag_slip_probability").HasPrecision(10, 9);
        builder.Ignore(version => version.AhpMatrix);
        builder.Ignore(version => version.AhpWeights);
        builder.Property<decimal[]>("ahpMatrix").HasColumnName("ahp_matrix").IsRequired();
        builder.Property<decimal[]>("ahpWeights").HasColumnName("ahp_weights").IsRequired();
        builder.Property(version => version.Status).HasColumnName("status").HasConversion<short>().IsRequired();
        builder.Property(version => version.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(version => version.RecalculationStartedAt).HasColumnName("recalculation_started_at");
        builder.Property(version => version.ActivatedAt).HasColumnName("activated_at");
        builder.Property(version => version.ArchivedAt).HasColumnName("archived_at");

        builder.HasIndex(version => version.Version).IsUnique().HasDatabaseName("UX_ModelVersions_Version");
        builder.HasIndex(version => version.Status)
            .IsUnique()
            .HasFilter("status = 3")
            .HasDatabaseName("UX_ModelVersions_Active");
    }
}
