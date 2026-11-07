namespace EducationSPD.Application.Models;

/// <summary>Сценарий чтения состояний навыков студента.</summary>
public interface IStudentSkillsQueryHandler
{
    /// <summary>Возвращает состояния навыков для активной версии модели.</summary>
    Task<StudentSkillsPage> GetAsync(
        string moduleKey,
        Guid studentId,
        bool includeDeleted,
        int offset,
        int limit,
        CancellationToken cancellationToken);
}
