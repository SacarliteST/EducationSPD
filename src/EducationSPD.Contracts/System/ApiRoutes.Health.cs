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

        /// <summary>Проверка жизнеспособности HTTP-процесса.</summary>
        public const string Live = "health/live";

        /// <summary>Проверка готовности инфраструктурных зависимостей.</summary>
        public const string Ready = "health/ready";
    }
}
