using EducationSPD.Contracts.Recommendations;
using EducationSPD.Domain.Skills;
using FluentValidation;

namespace EducationSPD.Web.Features.Recommendations;

internal sealed class RecommendationSkillRequestValidator : AbstractValidator<RecommendationSkillRequest>
{
    public RecommendationSkillRequestValidator()
    {
        RuleFor(skill => skill.SkillId)
            .NotEmpty()
            .MaximumLength(Skill.SkillIdMaxLength)
            .WithMessage("SkillId обязателен и не должен превышать допустимую длину.");
        RuleFor(skill => skill.Source)
            .NotNull()
            .IsInEnum()
            .WithMessage("Источник навыка не поддерживается.");
    }
}
