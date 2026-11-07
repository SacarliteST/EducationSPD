namespace EducationSPD.Contracts.Skills;

/// <summary>Параметры чтения состояний навыков студента.</summary>
public sealed record GetStudentSkillsRequest(
    bool IncludeDeleted = false,
    int Offset = 0,
    int Limit = 100);
