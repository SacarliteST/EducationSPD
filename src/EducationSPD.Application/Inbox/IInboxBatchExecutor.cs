using EducationSPD.Domain.Inbox;

namespace EducationSPD.Application.Inbox;

/// <summary>Выполняет обработку готового пакета inbox под блокировкой строк.</summary>
public interface IInboxBatchExecutor
{
    /// <summary>
    /// Блокирует доступный пакет, выполняет обработчик и атомарно сохраняет изменения.
    /// </summary>
    Task<int> ExecuteReadyBatchAsync(
        InboxMessageType messageType,
        Func<IReadOnlyList<InboxMessage>, CancellationToken, Task> processBatch,
        CancellationToken cancellationToken);
}
