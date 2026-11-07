using EducationSPD.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationSPD.Infrastructure.Persistence.Configurations;

internal sealed class RecalculationStudentModuleConfiguration
    : IEntityTypeConfiguration<RecalculationStudentModule>
{
    public void Configure(EntityTypeBuilder<RecalculationStudentModule> builder)
    {
        builder.HasNoKey();
        builder.ToView("LearningEpisodes");
        builder.Property(item => item.StudentId).HasColumnName("student_id");
        builder.Property(item => item.ModuleKey).HasColumnName("module_key");
    }
}
