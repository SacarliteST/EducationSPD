using EducationSPD.Domain.Models;

namespace EducationSPD.Tests.Models;

public sealed class ModelVersionTests
{
    [Fact]
    public void Parameters_CalculateNormalizedAhpWeights()
    {
        var parameters = CreateParameters();

        Assert.Equal(4, parameters.AhpWeights.Count);
        Assert.Equal(1m, parameters.AhpWeights.Sum(), 8);
    }

    [Fact]
    public void Parameters_RejectRuleThatIsLessReliableThanTag()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new ModelParameters(
                0.2m,
                0.1m,
                0.3m,
                0.1m,
                0.25m,
                0.25m,
                CreateMatrix()));

        Assert.Contains("менее надёжным", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NewParameters_CreateAnotherVersionInsteadOfChangingPreviousOne()
    {
        var createdAt = new DateTimeOffset(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);
        var first = ModelVersion.CreateDraft(Guid.NewGuid(), 1, CreateParameters(), createdAt);
        var changedParameters = new ModelParameters(
            0.3m,
            0.1m,
            0.1m,
            0.1m,
            0.25m,
            0.25m,
            CreateMatrix());

        var second = ModelVersion.CreateDraft(Guid.NewGuid(), 2, changedParameters, createdAt.AddMinutes(1));

        Assert.Equal(0.2m, first.InitialMasteryProbability);
        Assert.Equal(0.3m, second.InitialMasteryProbability);
        Assert.Equal(ModelVersionStatus.Draft, second.Status);
    }

    internal static ModelParameters CreateParameters() =>
        new(0.2m, 0.1m, 0.1m, 0.1m, 0.25m, 0.25m, CreateMatrix());

    private static decimal[] CreateMatrix() =>
    [
        1m, 3m, 5m, 7m,
        1m / 3m, 1m, 3m, 4m,
        1m / 5m, 1m / 3m, 1m, 2m,
        1m / 7m, 1m / 4m, 1m / 2m, 1m,
    ];
}
