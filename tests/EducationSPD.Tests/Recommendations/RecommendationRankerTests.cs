using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Recommendations;
using EducationSPD.Tests.Models;

namespace EducationSPD.Tests.Recommendations;

public sealed class RecommendationRankerTests
{
    [Fact]
    public void Rank_UsesInitialMasteryAndAllFinalizedEpisodes()
    {
        var model = ModelVersionTests.CreateParameters();
        var candidate = CreateCandidate(
            Guid.NewGuid(),
            difficulty: 1,
            episodeCount: 3,
            mastery: null,
            SkillObservationSource.TaskTag);

        var ranked = Assert.Single(RecommendationRanker.Rank([candidate], model));

        var deficit = Assert.Single(ranked.Reasons, reason => reason.Code == RecommendationReasonCode.TargetsWeakSkills);
        var difficulty = Assert.Single(ranked.Reasons, reason => reason.Code == RecommendationReasonCode.DifficultyMatchesCurrentLevel);
        var novelty = Assert.Single(ranked.Reasons, reason => reason.Code == RecommendationReasonCode.TaskIsNewForStudent);
        var diagnostic = Assert.Single(ranked.Reasons, reason => reason.Code == RecommendationReasonCode.ProvidesPreciseAssessment);
        Assert.Equal(model.AhpWeights[0] * 0.8m, deficit.Contribution);
        Assert.Equal(model.AhpWeights[1], difficulty.Contribution);
        Assert.Equal(model.AhpWeights[2] * 0.25m, novelty.Contribution);
        Assert.Equal(0, diagnostic.Contribution);
        Assert.Equal(ranked.Reasons.Sum(reason => reason.Contribution), ranked.RankScore);
    }

    [Fact]
    public void Rank_CalculatesDiagnosticValueFromPreciseSkills()
    {
        var model = ModelVersionTests.CreateParameters();
        var candidate = new RankingCandidate(
            Guid.NewGuid(),
            3,
            0,
            [
                new RankingCandidateSkill("join", SkillObservationSource.ValidationRule, 0.5m),
                new RankingCandidateSkill("filter", SkillObservationSource.TaskTag, 0.5m),
            ]);

        var ranked = Assert.Single(RecommendationRanker.Rank([candidate], model));
        var reason = Assert.Single(
            ranked.Reasons,
            item => item.Code == RecommendationReasonCode.ProvidesPreciseAssessment);

        Assert.Equal(model.AhpWeights[3] * 0.5m, reason.Contribution);
        Assert.Equal(["join"], reason.SkillIds);
    }

    [Fact]
    public void Rank_UsesTaskIdAsDeterministicTieBreakAndDoesNotAddCandidates()
    {
        var firstId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var secondId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var candidates = new[]
        {
            CreateCandidate(secondId, 3, 0, 0.6m, SkillObservationSource.ValidationRule),
            CreateCandidate(firstId, 3, 0, 0.6m, SkillObservationSource.ValidationRule),
        };

        var result = RecommendationRanker.Rank(candidates, ModelVersionTests.CreateParameters());

        Assert.Equal(candidates.Length, result.Count);
        Assert.Equal(firstId, result[0].TaskId);
        Assert.Equal(1, result[0].Rank);
        Assert.Equal(secondId, result[1].TaskId);
        Assert.Equal(2, result[1].Rank);
    }

    [Theory]
    [InlineData(0.29, 1)]
    [InlineData(0.30, 2)]
    [InlineData(0.50, 3)]
    [InlineData(0.70, 4)]
    [InlineData(0.85, 5)]
    public void Rank_DifficultyFitUsesDefinedMasteryBoundaries(decimal mastery, short difficulty)
    {
        var model = ModelVersionTests.CreateParameters();
        var candidate = CreateCandidate(
            Guid.NewGuid(),
            difficulty,
            0,
            mastery,
            SkillObservationSource.ValidationRule);

        var result = Assert.Single(RecommendationRanker.Rank([candidate], model));
        var reason = Assert.Single(
            result.Reasons,
            item => item.Code == RecommendationReasonCode.DifficultyMatchesCurrentLevel);

        Assert.Equal(model.AhpWeights[1], reason.Contribution);
    }

    private static RankingCandidate CreateCandidate(
        Guid taskId,
        short difficulty,
        int episodeCount,
        decimal? mastery,
        SkillObservationSource source) =>
        new(
            taskId,
            difficulty,
            episodeCount,
            [new RankingCandidateSkill("skill", source, mastery)]);
}
