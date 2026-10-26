using EducationSPD.Application.Common;
using EducationSPD.Domain.Examples;
using Microsoft.Extensions.Logging;

namespace EducationSPD.Application.Examples;

/// <summary>Сценарии работы с примерами.</summary>
public sealed class ExamplesService : IExamplesService
{
    private readonly IExamplesRepository repository;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<ExamplesService> logger;

    /// <summary>Создаёт экземпляр класса <see cref="ExamplesService"/>.</summary>
    /// <param name="repository">Хранилище примеров.</param>
    /// <param name="timeProvider">Источник времени.</param>
    /// <param name="logger">Журнал.</param>
    public ExamplesService(
        IExamplesRepository repository,
        TimeProvider timeProvider,
        ILogger<ExamplesService> logger)
    {
        this.repository = repository;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    /// <inheritdoc />
    public Task<PageResult<Example>> GetExamplesAsync(Pagination pagination, CancellationToken cancellationToken = default)
    {
        return repository.GetAllAsync(pagination, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Example?> GetExampleAsync(Guid exampleId, CancellationToken cancellationToken = default)
    {
        return repository.GetByIdAsync(exampleId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Example> CreateExampleAsync(CreateExampleCommand command, CancellationToken cancellationToken = default)
    {
        var example = new Example(command.Name, command.Description, timeProvider.GetUtcNow());
        await repository.CreateAsync(example, cancellationToken);
        logger.LogInformation("Создан пример {ExampleId}", example.Id);

        return example;
    }

    /// <inheritdoc />
    public async Task<Example> UpdateExampleAsync(
        Guid exampleId,
        UpdateExampleCommand command,
        CancellationToken cancellationToken = default)
    {
        var example = await repository.GetByIdAsync(exampleId, cancellationToken)
            ?? throw new ExampleNotFoundException(exampleId);

        example.Update(command.Name, command.Description);
        await repository.UpdateAsync(example, cancellationToken);

        return example;
    }

    /// <inheritdoc />
    public async Task DeleteExampleAsync(Guid exampleId, CancellationToken cancellationToken = default)
    {
        var example = await repository.GetByIdAsync(exampleId, cancellationToken)
            ?? throw new ExampleNotFoundException(exampleId);

        await repository.DeleteAsync(example, cancellationToken);
    }
}
