namespace EducationSPD.Application.Recommendations;

/// <summary>Сценарий персонального ранжирования переданных кандидатов.</summary>
public interface IRecommendationRankingHandler
{
    /// <summary>Проверяет каталог и ранжирует весь входной набор.</summary>
    Task<RecommendationRankingPage> RankAsync(
        RankRecommendationsInput input,
        CancellationToken cancellationToken);
}
