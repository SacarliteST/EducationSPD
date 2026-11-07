using EducationSPD.Application.Inbox;
using EducationSPD.Domain.Inbox;
using EducationSPD.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationSPD.Infrastructure.Inbox;

internal sealed class EfPendingCatalogNotifier(AppDbContext dbContext)
    : IPendingCatalogNotifier
{
    public async Task<int> ResumeAsync(
        string moduleKey,
        string skillId,
        CancellationToken cancellationToken)
    {
        var pendingStatus = (short)InboxStatus.PendingCatalog;
        var episodeType = (short)InboxMessageType.LearningEpisodeFinalized;
        var messages = await dbContext.InboxMessages
            .FromSqlInterpolated($"""
                SELECT message.*
                FROM "InboxMessages" AS message
                WHERE message.status = {pendingStatus}
                  AND message.message_type = {episodeType}
                  AND message.payload->>'moduleKey' = {moduleKey}
                  AND EXISTS (
                      SELECT 1
                      FROM jsonb_array_elements(message.payload->'skillResults') AS result
                      WHERE result->>'skillId' = {skillId})
                FOR UPDATE
                """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.ResumeAfterCatalogUpdate();
        }

        return messages.Count;
    }
}
