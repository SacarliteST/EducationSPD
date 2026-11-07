namespace EducationSPD.Host;

/// <summary>Точка входа процесса EducationSPD.</summary>
public sealed class Program
{
    /// <summary>Создаёт, настраивает и запускает веб-приложение.</summary>
    /// <param name="args">Аргументы командной строки.</param>
    public static async Task Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);
            Startup.ConfigureServices(builder);

            var app = builder.Build();
            await Startup.ConfigureAppAsync(app);

            app.Logger.LogInformation("Приложение EducationSPD запущено");
            await app.RunAsync();
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"EducationSPD аварийно завершён: {exception}");
            throw;
        }
        finally
        {
            Console.WriteLine("Остановка EducationSPD...");
        }
    }
}
