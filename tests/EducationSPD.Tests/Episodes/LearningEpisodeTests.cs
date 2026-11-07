using EducationSPD.Domain.Episodes;

namespace EducationSPD.Tests.Episodes;

public sealed class LearningEpisodeTests
{
    [Fact]
    public void Create_RejectsRepeatedSkillInsideEpisode()
    {
        var skillId = Guid.NewGuid();
        var observations = new[]
        {
            NewObservation(skillId, SkillObservationOutcome.Succeeded),
            NewObservation(skillId, SkillObservationOutcome.Failed)
        };

        var exception = Assert.Throws<ArgumentException>(() => CreateEpisode(observations));

        Assert.Contains("не может повторяться", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Model_ContainsOnlyImmutableMinimalEpisodeData()
    {
        var episodeProperties = typeof(LearningEpisode).GetProperties();
        var forbiddenProperties = new[]
        {
            "Score",
            "CompletionReason",
            "CourseId",
            "GroupId",
            "TaskSnapshot"
        };

        Assert.All(
            episodeProperties,
            property => Assert.True(property.SetMethod is null || !property.SetMethod.IsPublic));
        Assert.All(
            typeof(SkillObservation).GetProperties(),
            property => Assert.True(property.SetMethod is null || !property.SetMethod.IsPublic));
        Assert.DoesNotContain(
            episodeProperties,
            property => forbiddenProperties.Contains(property.Name, StringComparer.Ordinal));
    }

    private static LearningEpisode CreateEpisode(
        IReadOnlyCollection<NewSkillObservation> observations)
    {
        return LearningEpisode.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "sql",
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            observations);
    }

    private static NewSkillObservation NewObservation(
        Guid skillId,
        SkillObservationOutcome outcome)
    {
        return new NewSkillObservation(
            Guid.NewGuid(),
            skillId,
            SkillObservationSource.ValidationRule,
            outcome);
    }
}
