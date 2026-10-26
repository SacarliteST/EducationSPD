namespace EducationSPD.Web.Endpoints;

/// <summary>Регистрация всех групп маршрутов API.</summary>
internal static class EndpointGroups
{
    /// <summary>Регистрирует маршруты всех разделов API.</summary>
    /// <param name="app">Построитель маршрутов.</param>
    /// <returns>Тот же построитель маршрутов.</returns>
    public static IEndpointRouteBuilder MapAppEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapExamplesEndpointGroup();

        return app;
    }
}
