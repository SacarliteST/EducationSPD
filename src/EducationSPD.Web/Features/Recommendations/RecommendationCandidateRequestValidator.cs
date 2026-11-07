using EducationSPD.Contracts.Recommendations;
using FluentValidation;

namespace EducationSPD.Web.Features.Recommendations;

internal sealed class RecommendationCandidateRequestValidator
    : AbstractValidator<RecommendationCandidateRequest>
{
    public RecommendationCandidateRequestValidator()
    {
        RuleFor(candidate => candidate.TaskId)
            .NotNull()
            .NotEqual(Guid.Empty)
            .WithMessage("TaskId обязателен.");
        RuleFor(candidate => candidate.DifficultyLevel)
            .NotNull()
            .InclusiveBetween((short)1, (short)5)
            .WithMessage("Сложность должна находиться в диапазоне от 1 до 5.");
        RuleFor(candidate => candidate.Skills)
            .NotNull()
            .Must(skills => skills is { Count: >= 1 and <= 100 })
            .WithMessage("Кандидат должен содержать от 1 до 100 навыков.");
        RuleFor(candidate => candidate.Skills)
            .Must(skills => skills is null
                || skills.Where(skill => !string.IsNullOrWhiteSpace(skill.SkillId))
                    .Select(skill => skill.SkillId)
                    .Distinct(StringComparer.Ordinal)
                    .Count() == skills.Count(skill => !string.IsNullOrWhiteSpace(skill.SkillId)))
            .WithMessage("SkillId не должны повторяться внутри кандидата.");
        RuleForEach(candidate => candidate.Skills)
            .SetValidator(new RecommendationSkillRequestValidator())
            .When(candidate => candidate.Skills is not null);
    }
}
