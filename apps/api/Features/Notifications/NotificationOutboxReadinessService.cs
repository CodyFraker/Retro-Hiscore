using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Notifications;

public interface INotificationOutboxReadinessService
{
    Task MarkNotificationsReadyForSyncRunAsync(Guid syncRunId, CancellationToken cancellationToken = default);
}

public sealed class NotificationOutboxReadinessService(AppDbContext db) : INotificationOutboxReadinessService
{
    public async Task MarkNotificationsReadyForSyncRunAsync(Guid syncRunId, CancellationToken cancellationToken = default)
    {
        var run = await db.SyncRuns
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == syncRunId, cancellationToken);

        if (run is null || run.Status is not (SyncRunStatus.Succeeded or SyncRunStatus.PartialSuccess))
        {
            return;
        }

        var readyAt = run.FinishedAt ?? DateTimeOffset.UtcNow;
        await db.NotificationOutbox
            .Where(o => o.SourceSyncRunId == syncRunId && o.ReadyAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(o => o.ReadyAt, readyAt),
                cancellationToken);
    }
}
