using EducationSPD.Application.Common;
using EducationSPD.Domain.Examples;

namespace EducationSPD.Application.Examples;

/// <summary>Сценарии работы с примерами.</summary>
public interface IExamplesService
{
    /// <summary>Возвращает страницу примеров.</summary>
    /// <param name="pagination">Параметры страницы.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<PageResult<Example>> GetExamplesAsync(Pagination pagination, CancellationToken cancellationToken = default);

    /// <summary>Возвращает пример по идентификатору или <see langword="null"/>.</summary>
    /// <param name="exampleId">Идентификатор примера.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<Example?> GetExampleAsync(Guid exampleId, CancellationToken cancellationToken = default);

    /// <summary>Создаёт пример.</summary>
    /// <param name="command">Данные примера.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<Example> CreateExampleAsync(CreateExampleCommand command, CancellationToken cancellationToken = default);

    /// <summary>Изменяет пример.</summary>
    /// <param name="exampleId">Идентификатор примера.</param>
    /// <param name="command">Новые данные.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="ExampleNotFoundException">Пример не найден.</exception>
    Task<Example> UpdateExampleAsync(Guid exampleId, UpdateExampleCommand command, CancellationToken cancellationToken = default);

    /// <summary>Удаляет пример.</summary>
    /// <param name="exampleId">Идентификатор примера.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <exception cref="ExampleNotFoundException">Пример не найден.</exception>
    Task DeleteExampleAsync(Guid exampleId, CancellationToken cancellationToken = default);
}
