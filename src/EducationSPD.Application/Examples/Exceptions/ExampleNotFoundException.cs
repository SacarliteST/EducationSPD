namespace EducationSPD.Application.Examples;

/// <summary>Пример с указанным идентификатором не найден.</summary>
public sealed class ExampleNotFoundException : Exception
{
    /// <summary>Создаёт экземпляр класса <see cref="ExampleNotFoundException"/>.</summary>
    /// <param name="exampleId">Идентификатор примера.</param>
    public ExampleNotFoundException(Guid exampleId)
        : base($"Пример {exampleId} не найден.")
    {
    }
}
