namespace EducationSPD.Application.Recommendations;

/// <summary>Ошибка ссылочной проверки кандидатов относительно каталога модуля.</summary>
public sealed class RecommendationValidationException : Exception
{
    /// <summary>Создаёт ошибку с полями в формате ValidationProblemDetails.</summary>
    public RecommendationValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Кандидаты содержат недоступные навыки.")
    {
        Errors = errors;
    }

    /// <summary>Ошибки полей запроса.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
