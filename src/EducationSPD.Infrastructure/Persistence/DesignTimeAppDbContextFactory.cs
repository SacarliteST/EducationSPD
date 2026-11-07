using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EducationSPD.Infrastructure.Persistence;

/// <summary>
/// Создаёт контекст исключительно для команд EF Core design-time.
/// </summary>
public sealed class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc />
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=education_spd_design;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }
}
