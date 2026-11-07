using EducationSPD.Domain.Skills;

namespace EducationSPD.Application.Skills;

/// <summary>Хранилище локального каталога навыков.</summary>
public interface ISkillRepository
{
    /// <summary>Находит отслеживаемую проекцию навыка по внешней идентичности.</summary>
    Task<Skill?> FindAsync(string moduleKey, string skillId, CancellationToken cancellationToken);

    /// <summary>Добавляет новую проекцию навыка.</summary>
    Task AddAsync(Skill skill, CancellationToken cancellationToken);

    /// <summary>Сохраняет изменения каталога.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
