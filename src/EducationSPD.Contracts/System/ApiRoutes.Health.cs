namespace EducationSPD.Contracts;

public static partial class ApiRoutes
{
    /// <summary>
    /// Системные маршруты сервиса.
    /// </summary>
    public static class Health
    {
        /// <summary>
        /// Проверка доступности HTTP-процесса.
        /// </summary>
        public const string Status = "health";
    }
}
