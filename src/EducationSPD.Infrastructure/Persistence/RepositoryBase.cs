using EducationSPD.Application.Common;
using EducationSPD.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Persistence;

/// <summary>Базовый EF Core-репозиторий: постраничное чтение, создание, изменение и удаление сущностей.</summary>
/// <typeparam name="TEntity">Тип сущности.</typeparam>
/// <typeparam name="TKey">Тип первичного ключа.</typeparam>
internal abstract class RepositoryBase<TEntity, TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : Entity
    where TKey : IEquatable<TKey>
{
    /// <summary>Контекст базы данных.</summary>
    protected readonly AppDbContext DatabaseContext;

    /// <summary>Создаёт экземпляр класса <see cref="RepositoryBase{TEntity, TKey}"/>.</summary>
    /// <param name="databaseContext">Контекст базы данных.</param>
    protected RepositoryBase(AppDbContext databaseContext)
    {
        DatabaseContext = databaseContext;
    }

    /// <inheritdoc />
    public virtual async Task<PageResult<TEntity>> GetAllAsync(
        Pagination pagination,
        CancellationToken cancellationToken = default)
    {
        var entitySet = DatabaseContext.Set<TEntity>();
        var totalCount = await entitySet.CountAsync(cancellationToken);
        var items = await entitySet
            .OrderBy(entity => entity.Id)
            .Skip(pagination.Offset)
            .Take(pagination.Limit)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new PageResult<TEntity>(items, totalCount);
    }

    /// <inheritdoc />
    public virtual async Task CreateAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        await DatabaseContext.Set<TEntity>().AddAsync(item, cancellationToken);
        await DatabaseContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual Task UpdateAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        DatabaseContext.Update(item);
        return DatabaseContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public abstract Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DatabaseContext.Set<TEntity>().Remove(entity);
        return DatabaseContext.SaveChangesAsync(cancellationToken);
    }
}
