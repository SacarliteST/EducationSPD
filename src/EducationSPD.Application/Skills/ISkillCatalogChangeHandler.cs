namespace EducationSPD.Application.Skills;

/// <summary>Применяет изменения внешнего каталога навыков к локальной проекции.</summary>
public interface ISkillCatalogChangeHandler
{
    /// <summary>Применяет состояние, если событие новее сохранённого.</summary>
    Task<bool> ApplyAsync(SkillCatalogChange change, CancellationToken cancellationToken);
}
