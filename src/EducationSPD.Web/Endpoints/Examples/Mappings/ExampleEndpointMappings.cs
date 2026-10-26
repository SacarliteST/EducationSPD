using EducationSPD.Application.Common;
using EducationSPD.Application.Examples;
using EducationSPD.Contracts.Examples;
using EducationSPD.Domain.Examples;

namespace EducationSPD.Web.Endpoints;

/// <summary>Преобразование данных раздела «примеры» между контрактами API и слоем приложения.</summary>
internal static class ExampleEndpointMappings
{
    /// <summary>Преобразует <see cref="CreateExampleRequest"/> в команду приложения.</summary>
    /// <param name="request">Исходные данные.</param>
    public static CreateExampleCommand ToCommand(this CreateExampleRequest request)
    {
        return new CreateExampleCommand(request.Name, request.Description);
    }

    /// <summary>Преобразует <see cref="UpdateExampleRequest"/> в команду приложения.</summary>
    /// <param name="request">Исходные данные.</param>
    public static UpdateExampleCommand ToCommand(this UpdateExampleRequest request)
    {
        return new UpdateExampleCommand(request.Name, request.Description);
    }

    /// <summary>Преобразует <see cref="Example"/> в ответ API.</summary>
    /// <param name="example">Исходные данные.</param>
    public static ExampleResponse ToResponse(this Example example)
    {
        return new ExampleResponse(example.Id, example.Name, example.Description, example.CreatedAt);
    }

    /// <summary>Преобразует страницу примеров в ответ API.</summary>
    /// <param name="page">Исходные данные.</param>
    public static ExamplesPageResponse ToResponse(this PageResult<Example> page)
    {
        return new ExamplesPageResponse(page.Items.Select(ToResponse).ToList(), page.TotalCount);
    }
}
