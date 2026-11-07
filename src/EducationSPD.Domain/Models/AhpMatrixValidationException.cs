namespace EducationSPD.Domain.Models;

/// <summary>Описывает адресную ошибку конфигурации матрицы МАИ.</summary>
public sealed class AhpMatrixValidationException : ArgumentException
{
    /// <summary>Создаёт ошибку матрицы с необязательными координатами элемента.</summary>
    public AhpMatrixValidationException(
        AhpMatrixErrorCode errorCode,
        string message,
        int? row = null,
        int? column = null)
        : base(message, "ahpMatrix")
    {
        ErrorCode = errorCode;
        Row = row;
        Column = column;
    }

    /// <summary>Стабильный код нарушения.</summary>
    public AhpMatrixErrorCode ErrorCode { get; }

    /// <summary>Индекс строки проблемного элемента.</summary>
    public int? Row { get; }

    /// <summary>Индекс столбца проблемного элемента.</summary>
    public int? Column { get; }
}
