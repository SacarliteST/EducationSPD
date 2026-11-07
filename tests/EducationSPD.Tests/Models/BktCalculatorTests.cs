using EducationSPD.Domain.Episodes;
using EducationSPD.Domain.Models;

namespace EducationSPD.Tests.Models;

public sealed class BktCalculatorTests
{
    [Theory]
    [InlineData(SkillObservationOutcome.Succeeded, 0.7230769230769230769230769231)]
    [InlineData(SkillObservationOutcome.Failed, 0.1243243243243243243243243243)]
    public void Calculate_AppliesBayesUpdateAndLearningTransition(
        SkillObservationOutcome outcome,
        decimal expected)
    {
        var result = BktCalculator.Calculate(
            0.2m,
            outcome,
            SkillObservationSource.ValidationRule,
            ModelVersionTests.CreateParameters());

        Assert.Equal(expected, result, 12);
    }

    [Fact]
    public void Calculate_UsesDifferentParametersForRuleAndTag()
    {
        var parameters = ModelVersionTests.CreateParameters();

        var ruleResult = BktCalculator.Calculate(
            0.2m,
            SkillObservationOutcome.Succeeded,
            SkillObservationSource.ValidationRule,
            parameters);
        var tagResult = BktCalculator.Calculate(
            0.2m,
            SkillObservationOutcome.Succeeded,
            SkillObservationSource.TaskTag,
            parameters);

        Assert.Equal(0.4857142857142857142857142857m, tagResult, 12);
        Assert.True(ruleResult > tagResult);
    }

    [Fact]
    public void Calculate_ProducesDeterministicSequence()
    {
        var parameters = ModelVersionTests.CreateParameters();
        var observations = new[]
        {
            SkillObservationOutcome.Succeeded,
            SkillObservationOutcome.Failed,
            SkillObservationOutcome.Succeeded,
        };

        var firstRun = ApplySequence(observations, parameters);
        var secondRun = ApplySequence(observations, parameters);

        Assert.Equal(firstRun, secondRun);
        Assert.Equal(0.8163727959697732997481108311m, firstRun, 12);
    }

    [Theory]
    [InlineData(0, SkillObservationOutcome.Succeeded, 1)]
    [InlineData(1, SkillObservationOutcome.Failed, 0)]
    public void Calculate_HandlesZeroDenominatorWithoutLeavingProbabilityRange(
        decimal previousMastery,
        SkillObservationOutcome outcome,
        decimal expected)
    {
        var parameters = new ModelParameters(
            0.2m,
            0m,
            0m,
            0m,
            0m,
            0m,
            ModelVersionTests.CreateParameters().AhpMatrix);

        var result = BktCalculator.Calculate(
            previousMastery,
            outcome,
            SkillObservationSource.ValidationRule,
            parameters);

        Assert.Equal(expected, result);
        Assert.InRange(result, 0, 1);
    }

    [Fact]
    public void Calculate_RejectsUnknownSource()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BktCalculator.Calculate(
                0.2m,
                SkillObservationOutcome.Succeeded,
                (SkillObservationSource)99,
                ModelVersionTests.CreateParameters()));
    }

    private static decimal ApplySequence(
        IEnumerable<SkillObservationOutcome> observations,
        ModelParameters parameters)
    {
        var mastery = parameters.InitialMasteryProbability;
        foreach (var observation in observations)
        {
            mastery = BktCalculator.Calculate(
                mastery,
                observation,
                SkillObservationSource.ValidationRule,
                parameters);
        }

        return mastery;
    }
}
