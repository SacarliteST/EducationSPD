using EducationSPD.Domain.Common;

namespace EducationSPD.Domain.Examples;

/// <summary>
/// Пример доменной сущности. Замените на реальные сущности предметной области.
/// </summary>
public sealed class Example : Entity
{
    /// <summary>
    /// Название.
    /// </summary>
    public string Name { get; private set; } = String.Empty;

    /// <summary>
    /// Описание.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Момент создания (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    private Example()
    {
    }

    /// <summary>
    /// Создаёт пример.
    /// </summary>
    /// <param name="name">Название.</param>
    /// <param name="description">Описание.</param>
    /// <param name="createdAt">Момент создания (UTC).</param>
    public Example(string name, string? description, DateTimeOffset createdAt)
    {
        Name = name;
        Description = description;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Изменяет название и описание.
    /// </summary>
    /// <param name="name">Новое название.</param>
    /// <param name="description">Новое описание.</param>
    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
