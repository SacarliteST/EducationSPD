namespace EducationSPD.Application.Examples;

/// <summary>Команда изменения примера.</summary>
/// <param name="Name">Новое название.</param>
/// <param name="Description">Новое описание.</param>
public sealed record UpdateExampleCommand(string Name, string? Description);
