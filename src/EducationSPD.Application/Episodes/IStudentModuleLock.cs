namespace EducationSPD.Application.Episodes;

/// <summary>Сериализует изменение истории одного студента внутри модуля.</summary>
public interface IStudentModuleLock
{
    /// <summary>Получает транзакционную блокировку пары студент/модуль.</summary>
    Task AcquireAsync(
        Guid studentId,
        string moduleKey,
        CancellationToken cancellationToken);
}
