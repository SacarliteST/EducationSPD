namespace EducationSPD.Contracts.Examples;

/// <summary>Страница примеров.</summary>
/// <param name="Items">Элементы страницы.</param>
/// <param name="TotalCount">Общее число примеров.</param>
public sealed record ExamplesPageResponse(IReadOnlyList<ExampleResponse> Items, int TotalCount);
