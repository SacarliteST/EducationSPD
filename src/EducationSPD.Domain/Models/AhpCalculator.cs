namespace EducationSPD.Domain.Models;

/// <summary>Проверяет матрицу МАИ и рассчитывает нормализованные веса критериев.</summary>
public static class AhpCalculator
{
    private const decimal ReciprocalTolerance = 0.0001m;
    private const decimal MaxConsistencyRatio = 0.10m;

    /// <summary>Возвращает веса методом геометрических средних строк.</summary>
    public static decimal[] CalculateWeights(IReadOnlyList<decimal> ahpMatrix)
    {
        ArgumentNullException.ThrowIfNull(ahpMatrix);
        var matrix = ahpMatrix.ToArray();
        ValidateStructure(matrix);

        var weights = CalculateNormalizedWeights(matrix);
        ValidateConsistency(matrix, weights);
        return weights;
    }

    private static void ValidateStructure(decimal[] matrix)
    {
        if (matrix.Length != ModelParameters.AhpMatrixSize * ModelParameters.AhpMatrixSize)
        {
            throw new AhpMatrixValidationException(
                AhpMatrixErrorCode.InvalidSize,
                "Матрица МАИ должна иметь размер 4x4.");
        }

        for (var row = 0; row < ModelParameters.AhpMatrixSize; row++)
        {
            for (var column = 0; column < ModelParameters.AhpMatrixSize; column++)
            {
                var value = matrix[(row * ModelParameters.AhpMatrixSize) + column];
                if (value <= 0)
                {
                    throw new AhpMatrixValidationException(
                        AhpMatrixErrorCode.NonPositiveValue,
                        "Все элементы матрицы МАИ должны быть положительными.",
                        row,
                        column);
                }

                if (row == column && value != 1)
                {
                    throw new AhpMatrixValidationException(
                        AhpMatrixErrorCode.InvalidDiagonal,
                        "На диагонали матрицы МАИ должны находиться единицы.",
                        row,
                        column);
                }

                var reciprocal = matrix[(column * ModelParameters.AhpMatrixSize) + row];
                if (Math.Abs((value * reciprocal) - 1) > ReciprocalTolerance)
                {
                    throw new AhpMatrixValidationException(
                        AhpMatrixErrorCode.NotReciprocal,
                        "Матрица МАИ должна быть взаимно обратной.",
                        row,
                        column);
                }
            }
        }
    }

    private static void ValidateConsistency(decimal[] matrix, decimal[] weights)
    {
        decimal lambdaMax = 0;
        for (var row = 0; row < ModelParameters.AhpMatrixSize; row++)
        {
            decimal weightedSum = 0;
            for (var column = 0; column < ModelParameters.AhpMatrixSize; column++)
            {
                weightedSum += matrix[(row * ModelParameters.AhpMatrixSize) + column] * weights[column];
            }

            lambdaMax += weightedSum / weights[row];
        }

        lambdaMax /= ModelParameters.AhpMatrixSize;
        var consistencyIndex = (lambdaMax - ModelParameters.AhpMatrixSize)
            / (ModelParameters.AhpMatrixSize - 1);
        const decimal randomIndex = 0.90m;
        if (consistencyIndex / randomIndex > MaxConsistencyRatio)
        {
            throw new AhpMatrixValidationException(
                AhpMatrixErrorCode.Inconsistent,
                "Коэффициент согласованности матрицы МАИ должен быть не больше 0,10.");
        }
    }

    private static decimal[] CalculateNormalizedWeights(decimal[] matrix)
    {
        var geometricMeans = new decimal[ModelParameters.AhpMatrixSize];
        for (var row = 0; row < ModelParameters.AhpMatrixSize; row++)
        {
            double product = 1;
            for (var column = 0; column < ModelParameters.AhpMatrixSize; column++)
            {
                product *= (double)matrix[(row * ModelParameters.AhpMatrixSize) + column];
            }

            geometricMeans[row] = (decimal)Math.Pow(product, 1d / ModelParameters.AhpMatrixSize);
        }

        var total = geometricMeans.Sum();
        return geometricMeans.Select(value => value / total).ToArray();
    }
}
