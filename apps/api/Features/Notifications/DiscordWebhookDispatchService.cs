using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Notifications;

public interface IDiscordWebhookDispatchService
{
    Task DispatchPendingAsync(CancellationToken cancellationToken = default);
}

public sealed class DiscordWebhookDispatchService(
    AppDbContext db,
    IDiscordWebhookTemplateRenderer templateRenderer,
    IDiscordWebhookClient webhookClient,
    IWebhookUrlProtector urlProtector,
    ILogger<DiscordWebhookDispatchService> logger) : IDiscordWebhookDispatchService
{
    private const int MaxPostsPerWebhookPerRun = 10;

    public async Task DispatchPendingAsync(CancellationToken cancellationToken = default)
    {
        var run = new NotificationDispatchRun
        {
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        };
        db.NotificationDispatchRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var errors = new List<string>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            var webhooks = await db.DiscordWebhookConfigs
                .Include(w => w.EventSubscriptions)
                .Where(w => w.Enabled)
                .ToListAsync(cancellationToken);

            var pending = await db.NotificationOutbox
                .Where(o => o.DispatchedAt == null)
                .Where(o => o.ReadyAt != null && o.ReadyAt <= now)
                .Where(o => o.SourceSyncRunId == null
                    || !db.SyncRuns.Any(r => r.Id == o.SourceSyncRunId && r.Status == SyncRunStatus.Running))
                .OrderBy(o => o.ReadyAt ?? o.OccurredAt)
                .ToListAsync(cancellationToken);

            run.EventsProcessed = pending.Count;

            foreach (var webhook in webhooks)
            {
                if (!IsDigestReady(webhook, now))
                {
                    continue;
                }

                var subscribedKinds = webhook.EventSubscriptions.Select(s => s.EventKind).ToHashSet();
                if (subscribedKinds.Count == 0)
                {
                    continue;
                }

                var matching = pending
                    .Where(e => subscribedKinds.Contains(e.EventKind))
                    .Where(e => MatchesGameFilter(webhook, e))
                    .ToList();

                if (matching.Count == 0)
                {
                    continue;
                }

                var postsThisRun = 0;
                foreach (var entry in matching)
                {
                    if (postsThisRun >= MaxPostsPerWebhookPerRun)
                    {
                        break;
                    }

                    if (!IsEventReadyForWebhook(entry, webhook, now))
                    {
                        continue;
                    }

                    var delivery = await db.NotificationOutboxDeliveries
                        .FirstOrDefaultAsync(
                            d => d.OutboxId == entry.Id && d.WebhookConfigId == webhook.Id,
                            cancellationToken);

                    if (delivery?.DispatchedAt is not null)
                    {
                        continue;
                    }

                    if (delivery is null)
                    {
                        delivery = new NotificationOutboxDelivery
                        {
                            OutboxId = entry.Id,
                            WebhookConfigId = webhook.Id
                        };
                        db.NotificationOutboxDeliveries.Add(delivery);
                    }

                    try
                    {
                        var payloadJson = await GameTrackedNotificationHelper.ResolvePayloadJsonForDispatchAsync(
                            db,
                            entry,
                            cancellationToken);
                        var rendered = templateRenderer.Render(
                            webhook.PayloadTemplateJson,
                            entry.EventKind,
                            payloadJson);
                        var url = urlProtector.Unprotect(webhook.WebhookUrlProtected);
                        var success = await webhookClient.PostAsync(url, rendered, cancellationToken);
                        if (success)
                        {
                            delivery.DispatchedAt = now;
                            delivery.LastError = null;
                            run.PostsSucceeded++;
                            postsThisRun++;
                        }
                        else
                        {
                            delivery.AttemptCount++;
                            delivery.LastError = "Discord webhook POST failed";
                            entry.AttemptCount++;
                            entry.LastError = delivery.LastError;
                            run.PostsFailed++;
                            errors.Add($"Webhook {webhook.Name}: POST failed for outbox {entry.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        delivery.AttemptCount++;
                        delivery.LastError = ex.Message.Length > 4000 ? ex.Message[..4000] : ex.Message;
                        entry.AttemptCount++;
                        entry.LastError = delivery.LastError;
                        run.PostsFailed++;
                        logger.LogError(ex, "Failed to dispatch outbox {OutboxId} for webhook {WebhookName}", entry.Id, webhook.Name);
                        errors.Add($"{webhook.Name}: {ex.Message}");
                    }
                }

                webhook.LastDispatchedAt = now;
                webhook.UpdatedAt = now;
            }

            await db.SaveChangesAsync(cancellationToken);
            await MarkFullyDispatchedOutboxAsync(pending, cancellationToken);

            run.Status = errors.Count == 0
                ? SyncRunStatus.Succeeded
                : run.PostsSucceeded > 0
                    ? SyncRunStatus.PartialSuccess
                    : SyncRunStatus.Failed;
            run.Error = errors.Count == 0 ? null : string.Join("; ", errors.Take(5));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Discord notification dispatch run failed");
            run.Status = SyncRunStatus.Failed;
            run.Error = ex.Message;
        }
        finally
        {
            run.FinishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task MarkFullyDispatchedOutboxAsync(
        IReadOnlyList<NotificationOutbox> pending,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in pending)
        {
            var hasEnabledWebhook = await db.DiscordWebhookConfigs
                .AnyAsync(
                    w => w.Enabled
                        && w.EventSubscriptions.Any(s => s.EventKind == entry.EventKind),
                    cancellationToken);

            if (!hasEnabledWebhook)
            {
                entry.DispatchedAt = now;
                continue;
            }

            var anyDelivered = await db.NotificationOutboxDeliveries
                .AnyAsync(d => d.OutboxId == entry.Id && d.DispatchedAt != null, cancellationToken);

            if (anyDelivered)
            {
                entry.DispatchedAt = now;
            }
        }
    }

    private static bool IsDigestReady(DiscordWebhookConfig webhook, DateTimeOffset now)
    {
        if (webhook.DigestIntervalMinutes <= 0)
        {
            return true;
        }

        if (webhook.LastDispatchedAt is null)
        {
            return true;
        }

        return webhook.LastDispatchedAt.Value.AddMinutes(webhook.DigestIntervalMinutes) <= now;
    }

    private static bool IsEventReadyForWebhook(
        NotificationOutbox entry,
        DiscordWebhookConfig webhook,
        DateTimeOffset now)
    {
        if (webhook.DigestIntervalMinutes <= 0)
        {
            return true;
        }

        var effectiveReadyAt = entry.ReadyAt ?? entry.OccurredAt;
        return effectiveReadyAt.AddMinutes(webhook.DigestIntervalMinutes) <= now;
    }

    private static bool MatchesGameFilter(DiscordWebhookConfig webhook, NotificationOutbox entry)
    {
        if (webhook.AllowedRaGameIds is not { Length: > 0 })
        {
            return true;
        }

        var raGameId = TryReadRaGameId(entry);
        return raGameId is not null && webhook.AllowedRaGameIds.Contains(raGameId.Value);
    }

    private static int? TryReadRaGameId(NotificationOutbox entry)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(entry.PayloadJson);
            if (doc.RootElement.TryGetProperty("raGameId", out var prop) && prop.TryGetInt32(out var id))
            {
                return id;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
