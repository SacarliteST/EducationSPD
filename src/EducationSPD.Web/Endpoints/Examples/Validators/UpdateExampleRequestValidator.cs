using EducationSPD.Contracts.Examples;
using FluentValidation;

namespace EducationSPD.Web.Endpoints;

/// <summary>Правила валидации запроса <see cref="UpdateExampleRequest"/>.</summary>
internal sealed class UpdateExampleRequestValidator : AbstractValidator<UpdateExampleRequest>
{
    /// <summary>Создаёт правила валидации.</summary>
    public UpdateExampleRequestValidator()
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
