using EducationSPD.Contracts.Examples;
using FluentValidation;

namespace EducationSPD.Web.Features.Examples.Validators;

/// <summary>
/// Проверяет запрос создания примера.
/// </summary>
internal sealed class CreateExampleRequestValidator : AbstractValidator<CreateExampleRequest>
{
    /// <summary>
    /// Создаёт правила проверки.
    /// </summary>
    public CreateExampleRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Название обязательно.")
            .MaximumLength(200)
            .WithMessage("Название не должно превышать 200 символов.");

        RuleFor(request => request.Description)
            .MaximumLength(2000)
            .WithMessage("Описание не должно превышать 2000 символов.");
    }
}
