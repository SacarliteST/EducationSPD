using EducationSPD.Domain.Models;

namespace EducationSPD.Application.Models;

/// <summary>Хранилище BKT-проекций студентов.</summary>
public interface IStudentSkillStateRepository
{
    /// <summary>Находит отслеживаемую проекцию навыка для версии модели.</summary>
    Task<StudentSkillState?> FindAsync(
        Guid studentId,
        string moduleKey,
        Guid skillId,
        int modelVersion,
        CancellationToken cancellationToken);

    /// <summary>Добавляет новую проекцию.</summary>
    Task AddAsync(StudentSkillState state, CancellationToken cancellationToken);
}
