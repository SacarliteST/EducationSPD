namespace EducationSPD.Domain.Models;

/// <summary>Неизменяемый набор параметров одной версии математической модели.</summary>
public sealed class ModelParameters
{
    /// <summary>Размер матрицы попарных сравнений МАИ.</summary>
    public const int AhpMatrixSize = 4;

    private const decimal ReciprocalTolerance = 0.0001m;
    private const decimal MaxConsistencyRatio = 0.10m;

    /// <summary>Создаёт и проверяет параметры модели.</summary>
    public ModelParameters(
        decimal initialMasteryProbability,
        decimal learnProbability,
        decimal validationRuleGuessProbability,
        decimal validationRuleSlipProbability,
        decimal taskTagGuessProbability,
        decimal taskTagSlipProbability,
        IReadOnlyList<decimal> ahpMatrix)
    {
        ValidateProbability(initialMasteryProbability, nameof(initialMasteryProbability));
        ValidateProbability(learnProbability, nameof(learnProbability));
        ValidateObservation(
            validationRuleGuessProbability,
            validationRuleSlipProbability,
            nameof(validationRuleGuessProbability));
        ValidateObservation(
            taskTagGuessProbability,
            taskTagSlipProbability,
            nameof(taskTagGuessProbability));

        if (validationRuleGuessProbability > taskTagGuessProbability
            || validationRuleSlipProbability > taskTagSlipProbability)
        {
            throw new ArgumentException("Точное правило не может быть менее надёжным, чем навык-тег.");
        }

        ArgumentNullException.ThrowIfNull(ahpMatrix);
        var matrix = ahpMatrix.ToArray();
        ValidateAhpMatrix(matrix);

        InitialMasteryProbability = initialMasteryProbability;
        LearnProbability = learnProbability;
        ValidationRuleGuessProbability = validationRuleGuessProbability;
        ValidationRuleSlipProbability = validationRuleSlipProbability;
        TaskTagGuessProbability = taskTagGuessProbability;
        TaskTagSlipProbability = taskTagSlipProbability;
        AhpMatrix = matrix;
        AhpWeights = CalculateWeights(matrix);
    }

    /// <summary>Начальная вероятность освоения.</summary>
    public decimal InitialMasteryProbability { get; }

    /// <summary>Вероятность обучения.</summary>
    public decimal LearnProbability { get; }

    /// <summary>Вероятность угадывания для точного правила.</summary>
    public decimal ValidationRuleGuessProbability { get; }

    /// <summary>Вероятность ошибки для точного правила.</summary>
    public decimal ValidationRuleSlipProbability { get; }

    /// <summary>Вероятность угадывания для навыка-тега.</summary>
    public decimal TaskTagGuessProbability { get; }

    /// <summary>Вероятность ошибки для навыка-тега.</summary>
    public decimal TaskTagSlipProbability { get; }

    /// <summary>Матрица МАИ в построчном порядке.</summary>
    public IReadOnlyList<decimal> AhpMatrix { get; }

    /// <summary>Нормализованные веса критериев.</summary>
    public IReadOnlyList<decimal> AhpWeights { get; }

    private static void ValidateProbability(decimal value, string parameterName)
    {
        if (value is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Вероятность должна находиться в диапазоне от 0 до 1.");
        }
    }

    private static void ValidateObservation(decimal guess, decimal slip, string parameterName)
    {
        ValidateProbability(guess, parameterName);
        ValidateProbability(slip, parameterName);
        if (guess + slip >= 1)
        {
            throw new ArgumentException("Сумма вероятностей угадывания и ошибки должна быть меньше 1.", parameterName);
        }
    }

    private static void ValidateAhpMatrix(decimal[] matrix)
    {
        if (matrix.Length != AhpMatrixSize * AhpMatrixSize)
        {
            throw new ArgumentException("Матрица МАИ должна иметь размер 4x4.", nameof(matrix));
        }

        for (var row = 0; row < AhpMatrixSize; row++)
        {
            for (var column = 0; column < AhpMatrixSize; column++)
            {
                var value = matrix[(row * AhpMatrixSize) + column];
                if (value <= 0)
                {
                    throw new ArgumentException("Все элементы матрицы МАИ должны быть положительными.", nameof(matrix));
                }

                if (row == column && value != 1)
                {
                    throw new ArgumentException("На диагонали матрицы МАИ должны находиться единицы.", nameof(matrix));
                }

                var reciprocal = matrix[(column * AhpMatrixSize) + row];
                if (Math.Abs((value * reciprocal) - 1) > ReciprocalTolerance)
                {
                    throw new ArgumentException("Матрица МАИ должна быть взаимно обратной.", nameof(matrix));
                }
            }
        }

        var weights = CalculateWeights(matrix);
        decimal lambdaMax = 0;
        for (var row = 0; row < AhpMatrixSize; row++)
        {
            decimal weightedSum = 0;
            for (var column = 0; column < AhpMatrixSize; column++)
            {
                weightedSum += matrix[(row * AhpMatrixSize) + column] * weights[column];
            }

            lambdaMax += weightedSum / weights[row];
        }

        lambdaMax /= AhpMatrixSize;
        var consistencyIndex = (lambdaMax - AhpMatrixSize) / (AhpMatrixSize - 1);
        const decimal randomIndex = 0.90m;
        if (consistencyIndex / randomIndex > MaxConsistencyRatio)
        {
            throw new ArgumentException("Коэффициент согласованности матрицы МАИ должен быть не больше 0,10.", nameof(matrix));
        }
    }

    private static decimal[] CalculateWeights(decimal[] matrix)
    {
        var geometricMeans = new decimal[AhpMatrixSize];
        for (var row = 0; row < AhpMatrixSize; row++)
        {
            double product = 1;
            for (var column = 0; column < AhpMatrixSize; column++)
            {
                product *= (double)matrix[(row * AhpMatrixSize) + column];
            }

            geometricMeans[row] = (decimal)Math.Pow(product, 1d / AhpMatrixSize);
        }

        var total = geometricMeans.Sum();
        return geometricMeans.Select(value => value / total).ToArray();
    }
}
