using EducationSPD.Application.Common;
using EducationSPD.Application.Examples;
using EducationSPD.Contracts.Examples;
using EducationSPD.Domain.Examples;

namespace EducationSPD.Web.Features.Examples;

/// <summary>
/// Преобразует данные примеров между HTTP-контрактами и приложением.
/// </summary>
internal static class ExampleEndpointMappings
{
    public static CreateExampleCommand ToCommand(this CreateExampleRequest request) =>
        new(request.Name, request.Description);

    public static UpdateExampleCommand ToCommand(this UpdateExampleRequest request) =>
        new(request.Name, request.Description);

    public static ExampleResponse ToResponse(this Example example) =>
        new(example.Id, example.Name, example.Description, example.CreatedAt);

    public static ExamplesPageResponse ToResponse(this PageResult<Example> page) =>
        new(page.Items.Select(ToResponse).ToList(), page.TotalCount);
}
