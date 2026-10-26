namespace EducationSPD.Domain.Common;

/// <summary>
/// Базовая доменная сущность с уникальным идентификатором.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Уникальный идентификатор сущности.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
