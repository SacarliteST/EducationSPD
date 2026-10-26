namespace EducationSPD.Contracts.Examples;

/// <summary>Пример в ответе API.</summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Name">Название.</param>
/// <param name="Description">Описание.</param>
/// <param name="CreatedAt">Момент создания (UTC).</param>
public sealed record ExampleResponse(Guid Id, string Name, string? Description, DateTimeOffset CreatedAt);
