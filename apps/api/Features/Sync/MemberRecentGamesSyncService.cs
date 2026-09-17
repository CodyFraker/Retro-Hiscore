using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface IMemberRecentGamesSyncService
{
    Task SyncAllMembersAsync(DateTimeOffset syncedAt, CancellationToken cancellationToken = default);
    Task SyncMemberAsync(Member member, DateTimeOffset syncedAt, CancellationToken cancellationToken = default);
}

public sealed class MemberRecentGamesSyncService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    IConsoleIconSyncService consoleIconSync,
    IGameTrackQueueService gameTrackQueueService,
    ILeaderboardSyncActivityEnqueueService leaderboardSyncActivityEnqueue,
    IOptions<SyncOptions> syncOptions,
    ILogger<MemberRecentGamesSyncService> logger) : IMemberRecentGamesSyncService
{
    private readonly SyncOptions _syncOptions = syncOptions.Value;

    public async Task SyncAllMembersAsync(DateTimeOffset syncedAt, CancellationToken cancellationToken = default)
    {
        var members = await db.Members
            .Where(m => m.RaUsername != null)
            .ToListAsync(cancellationToken);

        var consoleIds = new HashSet<int>();
        var allChanges = new List<MemberRecentGamePlayChange>();

        foreach (var member in members)
        {
            try
            {
                var result = await SyncMemberInternalAsync(member, syncedAt, cancellationToken);
                foreach (var id in result.ConsoleIds)
                {
                    consoleIds.Add(id);
                }

                allChanges.AddRange(result.Changes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync recent games for member {Member}", member.RaUsername);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var consoleId in consoleIds)
        {
            try
            {
                await consoleIconSync.EnsureConsoleIconAsync(consoleId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to ensure console icon for console {ConsoleId}", consoleId);
            }
        }

        await gameTrackQueueService.EnqueueEligibleAsync(syncedAt, cancellationToken);
        await leaderboardSyncActivityEnqueue.EnqueueForMemberChangesAsync(
            allChanges,
            SyncTrigger.Scheduled,
            cancellationToken);
    }

    public async Task SyncMemberAsync(
        Member member,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default)
    {
        var result = await SyncMemberInternalAsync(member, syncedAt, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var consoleId in result.ConsoleIds)
        {
            try
            {
                await consoleIconSync.EnsureConsoleIconAsync(consoleId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to ensure console icon for console {ConsoleId}", consoleId);
            }
        }

        await gameTrackQueueService.EnqueueEligibleAsync(syncedAt, cancellationToken);
        await leaderboardSyncActivityEnqueue.EnqueueForMemberChangesAsync(
            result.Changes,
            SyncTrigger.Scheduled,
            cancellationToken);
    }

    private async Task<MemberRecentGamesSyncResult> SyncMemberInternalAsync(
        Member member,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken)
    {
        var consoleIds = new HashSet<int>();
        var changes = new List<MemberRecentGamePlayChange>();
        if (string.IsNullOrWhiteSpace(member.RaUsername))
        {
            return new MemberRecentGamesSyncResult(consoleIds, changes);
        }

        var trackedRaGameIds = await db.Games
            .AsNoTracking()
            .Select(g => g.RaGameId)
            .ToListAsync(cancellationToken);
        var trackedSet = trackedRaGameIds.ToHashSet();

        var identity = !string.IsNullOrWhiteSpace(member.RaUlid) ? member.RaUlid! : member.RaUsername!;
        var preferredKeys = string.IsNullOrWhiteSpace(member.RaApiKey)
            ? Array.Empty<string>()
            : new[] { member.RaApiKey };

        var fetchCount = Math.Clamp(_syncOptions.RecentGamesPerMember, 1, 50);
        var games = await apiKeyPool.ExecuteAsync(
            preferredKeys,
            (key, ct) => raApiClient.GetUserRecentlyPlayedGamesAsync(
                identity,
                fetchCount,
                0,
                key,
                ct),
            cancellationToken);

        var existing = await db.MemberRecentGamePlays
            .Where(p => p.MemberId == member.Id)
            .ToListAsync(cancellationToken);
        var existingByRaGameId = existing.ToDictionary(p => p.RaGameId);

        var returnedRaGameIds = new HashSet<int>();

        foreach (var game in games)
        {
            if (game.GameId <= 0 || string.IsNullOrWhiteSpace(game.Title))
            {
                continue;
            }

            returnedRaGameIds.Add(game.GameId);
            var lastPlayed = RaDateTime.Parse(game.LastPlayed) ?? syncedAt;
            var numPossible = game.NumPossibleAchievements ?? game.AchievementsTotal ?? 0;
            var numAchieved = game.NumAchieved ?? 0;

            consoleIds.Add(game.ConsoleId);

            var isNewOrUpdated = false;
            if (existingByRaGameId.TryGetValue(game.GameId, out var row))
            {
                isNewOrUpdated = lastPlayed > row.LastPlayedAt
                    || numAchieved > row.NumAchieved
                    || numPossible > row.NumPossibleAchievements;

                row.Title = game.Title.Trim();
                row.ConsoleId = game.ConsoleId;
                row.ConsoleName = game.ConsoleName;
                row.ImageIcon = game.ImageIcon;
                row.ImageBoxArt = game.ImageBoxArt;
                row.LastPlayedAt = lastPlayed;
                row.NumAchieved = numAchieved;
                row.NumPossibleAchievements = numPossible;
                row.SyncedAt = syncedAt;
            }
            else
            {
                isNewOrUpdated = true;
                db.MemberRecentGamePlays.Add(new MemberRecentGamePlay
                {
                    MemberId = member.Id,
                    RaGameId = game.GameId,
                    Title = game.Title.Trim(),
                    ConsoleId = game.ConsoleId,
                    ConsoleName = game.ConsoleName,
                    ImageIcon = game.ImageIcon,
                    ImageBoxArt = game.ImageBoxArt,
                    LastPlayedAt = lastPlayed,
                    NumAchieved = numAchieved,
                    NumPossibleAchievements = numPossible,
                    SyncedAt = syncedAt
                });
            }

            changes.Add(new MemberRecentGamePlayChange(
                member.Id,
                game.GameId,
                lastPlayed,
                numAchieved,
                numPossible,
                isNewOrUpdated));
        }

        foreach (var row in existing)
        {
            if (returnedRaGameIds.Contains(row.RaGameId))
            {
                continue;
            }

            if (trackedSet.Contains(row.RaGameId))
            {
                continue;
            }

            db.MemberRecentGamePlays.Remove(row);
        }

        return new MemberRecentGamesSyncResult(consoleIds, changes);
    }

    private sealed record MemberRecentGamesSyncResult(
        HashSet<int> ConsoleIds,
        List<MemberRecentGamePlayChange> Changes);
}
