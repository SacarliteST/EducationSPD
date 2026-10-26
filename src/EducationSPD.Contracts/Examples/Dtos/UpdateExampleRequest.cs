namespace EducationSPD.Contracts.Examples;

/// <summary>Запрос на изменение примера.</summary>
/// <param name="Name">Новое название (до 200 символов).</param>
/// <param name="Description">Новое описание (до 2000 символов).</param>
public sealed record UpdateExampleRequest(string Name, string? Description);
