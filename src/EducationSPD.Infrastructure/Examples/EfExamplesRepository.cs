using EducationSPD.Application.Examples;
using EducationSPD.Domain.Examples;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Examples;

/// <summary>EF Core-реализация хранилища примеров.</summary>
internal sealed class EfExamplesRepository : RepositoryBase<Example, Guid>, IExamplesRepository
{
    /// <summary>Создаёт экземпляр класса <see cref="EfExamplesRepository"/>.</summary>
    /// <param name="context">Контекст базы данных.</param>
    public EfExamplesRepository(AppDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public override Task<Example?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DatabaseContext.Examples
            .AsNoTracking()
            .FirstOrDefaultAsync(example => example.Id == id, cancellationToken);
    }
}
