namespace EducationSPD.Contracts.Skills;

/// <summary>Страница состояний навыков студента для активной модели.</summary>
public sealed record StudentSkillsResponse(
    int Count,
    int Offset,
    int Limit,
    int ModelVersion,
    IReadOnlyList<StudentSkillResponse> Items);
