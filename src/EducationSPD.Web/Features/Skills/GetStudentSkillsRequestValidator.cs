using EducationSPD.Contracts.Skills;
using FluentValidation;

namespace EducationSPD.Web.Features.Skills;

internal sealed class GetStudentSkillsRequestValidator : AbstractValidator<GetStudentSkillsRequest>
{
    public GetStudentSkillsRequestValidator()
    {
        RuleFor(request => request.Offset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Смещение не может быть отрицательным.");
        RuleFor(request => request.Limit)
            .InclusiveBetween(1, 500)
            .WithMessage("Размер страницы должен находиться в диапазоне от 1 до 500.");
    }
}
