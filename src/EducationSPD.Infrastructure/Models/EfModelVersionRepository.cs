using EducationSPD.Application.Models;
using EducationSPD.Domain.Models;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Models;

internal sealed class EfModelVersionRepository(AppDbContext dbContext) : IModelVersionRepository
{
    public Task<ModelVersion?> GetActiveAsync(CancellationToken cancellationToken) =>
        dbContext.ModelVersions
            .AsNoTracking()
            .SingleOrDefaultAsync(version => version.Status == ModelVersionStatus.Active, cancellationToken);

    public async Task<int> GetNextVersionAsync(CancellationToken cancellationToken)
    {
        var lastVersion = await dbContext.ModelVersions
            .MaxAsync(version => (int?)version.Version, cancellationToken);
        return (lastVersion ?? 0) + 1;
    }

    public async Task AddAsync(ModelVersion modelVersion, CancellationToken cancellationToken) =>
        await dbContext.ModelVersions.AddAsync(modelVersion, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
