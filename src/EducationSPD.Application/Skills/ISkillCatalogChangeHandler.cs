namespace EducationSPD.Application.Skills;

/// <summary>Применяет изменения внешнего каталога навыков к локальной проекции.</summary>
public interface ISkillCatalogChangeHandler
{
    /// <summary>Применяет состояние, если событие новее сохранённого.</summary>
    Task ApplyAsync(SkillCatalogChange change, CancellationToken cancellationToken);
}
