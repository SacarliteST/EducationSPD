namespace EducationSPD.Domain.Models;

/// <summary>Код нарушения требований к матрице МАИ.</summary>
public enum AhpMatrixErrorCode
{
    /// <summary>Матрица имеет неверный размер.</summary>
    InvalidSize = 1,

    /// <summary>Элемент матрицы не является положительным.</summary>
    NonPositiveValue = 2,

    /// <summary>Диагональный элемент не равен единице.</summary>
    InvalidDiagonal = 3,

    /// <summary>Парные элементы не являются взаимно обратными.</summary>
    NotReciprocal = 4,

    /// <summary>Коэффициент согласованности превышает допустимый предел.</summary>
    Inconsistent = 5,
}
