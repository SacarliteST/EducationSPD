namespace EducationSPD.Application.Models;

/// <summary>Страница навыков студента, рассчитанная для активной версии модели.</summary>
public sealed record StudentSkillsPage(
    int Count,
    int Offset,
    int Limit,
    int ModelVersion,
    IReadOnlyList<StudentSkillReadItem> Items);
