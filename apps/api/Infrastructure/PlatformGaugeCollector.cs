using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Infrastructure;

public sealed class PlatformGaugeCollector(
    IServiceProvider serviceProvider,
    IOptions<TelemetryOptions> telemetryOptions,
    ILogger<PlatformGaugeCollector> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = Math.Clamp(telemetryOptions.Value.GaugeIntervalSeconds, 5, 300);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Platform gauge collection failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }

    private async Task CollectAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var membersTotal = await db.Members.CountAsync(m => m.DiscordId != null, cancellationToken);
        var membersWithKey = await db.Members.CountAsync(
            m => m.DiscordId != null && m.RaApiKey != null && m.RaApiKey != "",
            cancellationToken);
        var trackQueuePending = await db.GameTrackQueues.CountAsync(
            q => q.Status == GameTrackQueueStatus.Pending,
            cancellationToken);
        var outboxUndispatched = await db.NotificationOutbox.CountAsync(
            o => o.DispatchedAt == null,
            cancellationToken);
        var leaderboardInProgress = await db.SyncRuns.CountAsync(
            r => r.Kind == SyncKind.LeaderboardScores
                && r.Status == SyncRunStatus.Running
                && r.FinishedAt == null,
            cancellationToken);

        PlatformMetrics.SetMembersTotal(membersTotal);
        PlatformMetrics.SetMembersWithRaApiKey(membersWithKey);
        PlatformMetrics.SetGameTrackQueuePending(trackQueuePending);
        PlatformMetrics.SetNotificationOutboxUndispatched(outboxUndispatched);
        PlatformMetrics.SetLeaderboardSyncInProgress(leaderboardInProgress);
    }
}
