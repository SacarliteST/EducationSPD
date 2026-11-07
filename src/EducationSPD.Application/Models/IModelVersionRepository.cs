using EducationSPD.Domain.Models;

namespace EducationSPD.Application.Models;

/// <summary>Хранилище версий математической модели.</summary>
public interface IModelVersionRepository
{
    /// <summary>Возвращает текущую целостную активную версию.</summary>
    Task<ModelVersion?> GetActiveAsync(CancellationToken cancellationToken);

    /// <summary>Возвращает следующий свободный номер версии.</summary>
    Task<int> GetNextVersionAsync(CancellationToken cancellationToken);

    /// <summary>Добавляет новую версию, не изменяя историю.</summary>
    Task AddAsync(ModelVersion modelVersion, CancellationToken cancellationToken);

    /// <summary>Сохраняет изменения состояний версий.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
