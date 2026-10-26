namespace EducationSPD.Application.Common;

/// <summary>
/// Описывает базовые операции репозитория.
/// </summary>
public interface IBaseRepository<TEntity, TKey>
    where TEntity : class
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Возвращает страницу сущностей.
    /// </summary>
    Task<PageResult<TEntity>> GetAllAsync(Pagination pagination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет сущность в хранилище.
    /// </summary>
    Task CreateAsync(TEntity item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет сущность в хранилище.
    /// </summary>
    Task UpdateAsync(TEntity item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает сущность по ключу.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет сущность из хранилища.
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}
