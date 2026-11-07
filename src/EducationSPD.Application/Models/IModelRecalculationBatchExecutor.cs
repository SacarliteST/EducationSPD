namespace EducationSPD.Application.Models;

/// <summary>Выполняет одну возобновляемую порцию полного пересчёта модели.</summary>
public interface IModelRecalculationBatchExecutor
{
    /// <summary>Обрабатывает одну порцию либо атомарно завершает активацию.</summary>
    Task<bool> ExecuteAsync(CancellationToken cancellationToken);
}
