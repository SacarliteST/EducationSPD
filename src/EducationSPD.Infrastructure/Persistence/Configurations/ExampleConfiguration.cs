using EducationSPD.Domain.Examples;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationSPD.Infrastructure.Persistence.Configurations;

/// <summary>Отображение сущности <see cref="Example"/> на таблицу базы данных.</summary>
internal sealed class ExampleConfiguration : IEntityTypeConfiguration<Example>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Example> builder)
    {
        builder.ToTable("Examples");
        builder.HasKey(example => example.Id);
        builder.Property(example => example.Id).HasColumnName("id");
        builder.Property(example => example.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(example => example.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(example => example.CreatedAt).HasColumnName("created_at");
    }
}
