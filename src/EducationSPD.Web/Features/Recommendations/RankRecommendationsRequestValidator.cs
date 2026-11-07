using EducationSPD.Contracts.Recommendations;
using EducationSPD.Domain.Skills;
using FluentValidation;

namespace EducationSPD.Web.Features.Recommendations;

internal sealed class RankRecommendationsRequestValidator : AbstractValidator<RankRecommendationsRequest>
{
    public RankRecommendationsRequestValidator()
    {
        RuleFor(request => request.StudentId)
            .NotNull()
            .NotEqual(Guid.Empty)
            .WithMessage("Идентификатор студента обязателен.");
        RuleFor(request => request.ModuleKey)
            .NotEmpty()
            .MaximumLength(Skill.ModuleKeyMaxLength)
            .Matches("^[a-z0-9-]+$")
            .WithMessage("Ключ модуля заполнен некорректно.");
        RuleFor(request => request.Candidates)
            .NotNull()
            .Must(candidates => candidates is { Count: >= 1 and <= 500 })
            .WithMessage("Передайте от 1 до 500 кандидатов.");
        RuleFor(request => request.Candidates)
            .Must(candidates => candidates is null
                || candidates.Where(candidate => candidate.TaskId.HasValue)
                    .Select(candidate => candidate.TaskId)
                    .Distinct()
                    .Count() == candidates.Count(candidate => candidate.TaskId.HasValue))
            .WithMessage("TaskId кандидатов не должны повторяться.");
        RuleForEach(request => request.Candidates)
            .SetValidator(new RecommendationCandidateRequestValidator())
            .When(request => request.Candidates is not null);
    }
}
