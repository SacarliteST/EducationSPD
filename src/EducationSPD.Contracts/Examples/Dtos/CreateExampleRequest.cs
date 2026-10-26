namespace EducationSPD.Contracts.Examples;

/// <summary>Запрос на создание примера.</summary>
/// <param name="Name">Название (до 200 символов).</param>
/// <param name="Description">Описание (до 2000 символов).</param>
public sealed record CreateExampleRequest(string Name, string? Description);
