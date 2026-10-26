namespace EducationSPD.Contracts;

/// <summary>
/// Централизованные шаблоны маршрутов API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Общий версионированный префикс API.
    /// </summary>
    public const string PrefixV1 = "api/v1";

    /// <summary>Подставляет значение в параметр маршрута с экранированием.</summary>
    private static string ReplaceUrlSegment(string template, string name, string value)
    {
        var escapedUri = Uri.EscapeDataString(value);
        return template.Replace("{" + name + "}", escapedUri);
    }
}
