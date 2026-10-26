namespace EducationSPD.Application.Common;

/// <summary>
/// Результат постраничного запроса к хранилищу.
/// </summary>
public sealed record PageResult<TEntity>(
    IReadOnlyList<TEntity> Items,
    int TotalCount);
