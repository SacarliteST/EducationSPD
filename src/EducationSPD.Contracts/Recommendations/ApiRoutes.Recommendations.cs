namespace EducationSPD.Contracts;

public static partial class ApiRoutes
{
    /// <summary>Маршруты персональных рекомендаций.</summary>
    public static class Recommendations
    {
        /// <summary>Ранжирование переданного набора заданий.</summary>
        public const string Rank = PrefixV1 + "/recommendations/rank";
    }
}
