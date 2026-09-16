using System.Text.Json;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Notifications;

public interface INotificationOutboxWriter
{
    Task EnqueueAsync<T>(
        DiscordNotificationEventKind kind,
        T payload,
        CancellationToken cancellationToken = default,
        EnqueueOutboxOptions? options = null);
}

public sealed class NotificationOutboxWriter(AppDbContext db) : INotificationOutboxWriter
{
    public async Task EnqueueAsync<T>(
        DiscordNotificationEventKind kind,
        T payload,
        CancellationToken cancellationToken = default,
        EnqueueOutboxOptions? options = null)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var waitForSync = options?.WaitForSync == true;
        db.NotificationOutbox.Add(new NotificationOutbox
        {
            EventKind = kind,
            PayloadJson = JsonSerializer.Serialize(payload, NotificationPayloadJson.Options),
            OccurredAt = occurredAt,
            ReadyAt = waitForSync ? null : options?.ReadyAt ?? occurredAt,
            SourceSyncRunId = options?.SourceSyncRunId
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
