namespace EducationSPD.Application.Models;

/// <summary>Читает каталог навыков вместе с BKT-проекциями.</summary>
public interface IStudentSkillStateReader
{
    /// <summary>Возвращает стабильную страницу навыков одного модуля.</summary>
    Task<StudentSkillReadSlice> ReadAsync(
        string moduleKey,
        Guid studentId,
        int modelVersion,
        decimal initialMasteryProbability,
        bool includeDeleted,
        int offset,
        int limit,
        CancellationToken cancellationToken);
}
