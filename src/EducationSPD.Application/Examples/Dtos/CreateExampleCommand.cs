namespace EducationSPD.Application.Examples;

/// <summary>Команда создания примера.</summary>
/// <param name="Name">Название.</param>
/// <param name="Description">Описание.</param>
public sealed record CreateExampleCommand(string Name, string? Description);
