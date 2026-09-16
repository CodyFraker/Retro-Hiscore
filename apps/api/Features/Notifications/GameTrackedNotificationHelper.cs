using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Notifications;

public static class GameTrackedNotificationHelper
{
    public static async Task EnqueuePendingForSyncRunAsync(
        AppDbContext db,
        INotificationOutboxWriter outboxWriter,
        Guid gameId,
        Guid syncRunId,
        CancellationToken cancellationToken)
    {
        var game = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken);
        if (game is null)
        {
            return;
        }

        var payload = new GameTrackedPendingNotificationPayload(game.RaGameId);
        await outboxWriter.EnqueueAsync(
            DiscordNotificationEventKind.GameTracked,
            payload,
            cancellationToken,
            new EnqueueOutboxOptions(SourceSyncRunId: syncRunId, WaitForSync: true));
    }

    public static async Task<string> BuildPayloadJsonAsync(
        AppDbContext db,
        int raGameId,
        DateTimeOffset trackedAt,
        CancellationToken cancellationToken)
    {
        var game = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.RaGameId == raGameId, cancellationToken);
        if (game is null)
        {
            return JsonSerializer.Serialize(
                new GameTrackedPendingNotificationPayload(raGameId),
                NotificationPayloadJson.Options);
        }

        var numLeaderboards = await db.Leaderboards.CountAsync(l => l.GameId == game.Id, cancellationToken);
        var numAchievements = await db.RaAchievements.CountAsync(a => a.RaGameId == game.RaGameId, cancellationToken);
        var leaderboardIds = await db.Leaderboards
            .Where(l => l.GameId == game.Id)
            .Select(l => l.Id)
            .ToListAsync(cancellationToken);

        var totalEntries = 0;
        if (leaderboardIds.Count > 0)
        {
            var latestPerBoard = await db.LeaderboardPopulationSnapshots
                .AsNoTracking()
                .Where(p => leaderboardIds.Contains(p.LeaderboardId))
                .GroupBy(p => p.LeaderboardId)
                .Select(g => g.OrderByDescending(p => p.SyncedAt).First().EntryCount)
                .ToListAsync(cancellationToken);
            totalEntries = latestPerBoard.Sum();
        }

        var payload = new GameTrackedNotificationPayload(
            game.RaGameId,
            game.Title,
            game.ConsoleName,
            numLeaderboards,
            numAchievements,
            totalEntries,
            trackedAt);

        return JsonSerializer.Serialize(payload, NotificationPayloadJson.Options);
    }

    public static async Task<string> ResolvePayloadJsonForDispatchAsync(
        AppDbContext db,
        NotificationOutbox entry,
        CancellationToken cancellationToken)
    {
        if (entry.EventKind != DiscordNotificationEventKind.GameTracked)
        {
            return entry.PayloadJson;
        }

        var raGameId = TryReadRaGameId(entry.PayloadJson);
        if (raGameId is null)
        {
            return entry.PayloadJson;
        }

        var trackedAt = entry.ReadyAt ?? entry.OccurredAt;
        return await BuildPayloadJsonAsync(db, raGameId.Value, trackedAt, cancellationToken);
    }

    private static int? TryReadRaGameId(string payloadJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
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
