using EducationSPD.Domain.Skills;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationSPD.Infrastructure.Persistence.Configurations;

internal sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable(
            "Skills",
            table => table.HasCheckConstraint(
                "CK_Skills_Origin",
                "origin IN (1, 2)"));

        builder.HasKey(skill => skill.Id);

        builder.Property(skill => skill.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(skill => skill.ModuleKey)
            .HasColumnName("module_key")
            .HasMaxLength(Skill.ModuleKeyMaxLength)
            .IsRequired();

        builder.Property(skill => skill.SkillId)
            .HasColumnName("skill_id")
            .HasMaxLength(Skill.SkillIdMaxLength)
            .IsRequired();

        builder.Property(skill => skill.Name)
            .HasColumnName("name")
            .HasMaxLength(Skill.NameMaxLength)
            .IsRequired();

        builder.Property(skill => skill.Description)
            .HasColumnName("description")
            .HasMaxLength(Skill.DescriptionMaxLength);

        builder.Property(skill => skill.Origin)
            .HasColumnName("origin")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(skill => skill.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(skill => skill.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(skill => skill.SourceEventId)
            .HasColumnName("source_event_id")
            .IsRequired();

        builder.HasIndex(skill => new { skill.ModuleKey, skill.SkillId })
            .IsUnique()
            .HasDatabaseName("UX_Skills_ModuleKey_SkillId");
    }
}
