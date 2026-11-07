using EducationSPD.Domain.Models;

namespace EducationSPD.Tests.Models;

public sealed class AhpCalculatorTests
{
    [Fact]
    public void CalculateWeights_BaseMatrixProducesExpectedNormalizedWeights()
    {
        var weights = AhpCalculator.CalculateWeights(ModelVersionTests.CreateParameters().AhpMatrix);

        Assert.Equal(0.57m, weights[0], 2);
        Assert.Equal(0.25m, weights[1], 2);
        Assert.Equal(0.11m, weights[2], 2);
        Assert.Equal(0.07m, weights[3], 2);
        Assert.Equal(1m, weights.Sum(), 12);
    }

    [Theory]
    [MemberData(nameof(InvalidMatrices))]
    public void CalculateWeights_InvalidMatrixReturnsAddressableError(
        decimal[] matrix,
        AhpMatrixErrorCode expectedCode,
        int? expectedRow,
        int? expectedColumn)
    {
        var exception = Assert.Throws<AhpMatrixValidationException>(
            () => AhpCalculator.CalculateWeights(matrix));

        Assert.Equal(expectedCode, exception.ErrorCode);
        Assert.Equal(expectedRow, exception.Row);
        Assert.Equal(expectedColumn, exception.Column);
    }

    public static TheoryData<decimal[], AhpMatrixErrorCode, int?, int?> InvalidMatrices()
    {
        var nonPositive = BaseMatrix();
        nonPositive[1] = 0;
        var diagonal = BaseMatrix();
        diagonal[0] = 2;
        var notReciprocal = BaseMatrix();
        notReciprocal[4] = 0.5m;

        return new TheoryData<decimal[], AhpMatrixErrorCode, int?, int?>
        {
            { [1m], AhpMatrixErrorCode.InvalidSize, null, null },
            { nonPositive, AhpMatrixErrorCode.NonPositiveValue, 0, 1 },
            { diagonal, AhpMatrixErrorCode.InvalidDiagonal, 0, 0 },
            { notReciprocal, AhpMatrixErrorCode.NotReciprocal, 0, 1 },
            { InconsistentMatrix(), AhpMatrixErrorCode.Inconsistent, null, null },
        };
    }

    private static decimal[] BaseMatrix() => ModelVersionTests.CreateParameters().AhpMatrix.ToArray();

    private static decimal[] InconsistentMatrix() =>
    [
        1m, 9m, 1m, 1m,
        1m / 9m, 1m, 9m, 1m,
        1m, 1m / 9m, 1m, 9m,
        1m, 1m, 1m / 9m, 1m,
    ];
}
