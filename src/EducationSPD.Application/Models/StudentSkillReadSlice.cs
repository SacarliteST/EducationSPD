namespace EducationSPD.Application.Models;

/// <summary>Результат запроса страницы навыков к хранилищу.</summary>
public sealed record StudentSkillReadSlice(
    int Count,
    IReadOnlyList<StudentSkillReadItem> Items);
