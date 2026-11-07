namespace EducationSPD.Application.Inbox;

/// <summary>Возобновляет эпизоды, ожидающие определения навыка.</summary>
public interface IPendingCatalogNotifier
{
    /// <summary>Возвращает в очередь эпизоды, которые ссылаются на обновлённый навык.</summary>
    Task<int> ResumeAsync(
        string moduleKey,
        string skillId,
        CancellationToken cancellationToken);
}
