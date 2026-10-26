namespace EducationSPD.Contracts;

public static partial class ApiRoutes
{
    /// <summary>
    /// Шаблоны маршрутов для работы с примерами.
    /// </summary>
    public static class Examples
    {
        /// <summary>
        /// Маршрут коллекции примеров.
        /// </summary>
        public const string ExamplesList = PrefixV1 + "/examples";

        /// <summary>
        /// Шаблон маршрута примера по идентификатору.
        /// </summary>
        public const string Example = ExamplesList + "/{exampleId:guid}";

        /// <summary>
        /// Создаёт маршрут для конкретного примера.
        /// </summary>
        public static string ForExample(Guid exampleId) => ReplaceUrlSegment(Example, "exampleId:guid", exampleId.ToString());
    }
}
