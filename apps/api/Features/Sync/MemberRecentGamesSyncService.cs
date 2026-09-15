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

        foreach (var member in members)
        {
            try
            {
                var ids = await SyncMemberInternalAsync(member, syncedAt, cancellationToken);
                foreach (var id in ids)
                {
                    consoleIds.Add(id);
                }
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
    }

    public async Task SyncMemberAsync(
        Member member,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default)
    {
        var consoleIds = await SyncMemberInternalAsync(member, syncedAt, cancellationToken);
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
    }

    private async Task<HashSet<int>> SyncMemberInternalAsync(
        Member member,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken)
    {
        var consoleIds = new HashSet<int>();
        if (string.IsNullOrWhiteSpace(member.RaUsername))
        {
            return consoleIds;
        }

        var identity = !string.IsNullOrWhiteSpace(member.RaUlid) ? member.RaUlid! : member.RaUsername!;
        var preferredKeys = string.IsNullOrWhiteSpace(member.RaApiKey)
            ? Array.Empty<string>()
            : new[] { member.RaApiKey };

        var games = await apiKeyPool.ExecuteAsync(
            preferredKeys,
            (key, ct) => raApiClient.GetUserRecentlyPlayedGamesAsync(
                identity,
                _syncOptions.RecentGamesPerMember,
                0,
                key,
                ct),
            cancellationToken);

        var existing = await db.MemberRecentGamePlays
            .Where(p => p.MemberId == member.Id)
            .ToListAsync(cancellationToken);
        db.MemberRecentGamePlays.RemoveRange(existing);

        foreach (var game in games)
        {
            if (game.GameId <= 0 || string.IsNullOrWhiteSpace(game.Title))
            {
                continue;
            }

            var lastPlayed = RaDateTime.Parse(game.LastPlayed) ?? syncedAt;
            var numPossible = game.NumPossibleAchievements ?? game.AchievementsTotal ?? 0;
            var numAchieved = game.NumAchieved ?? 0;

            consoleIds.Add(game.ConsoleId);

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

        return consoleIds;
    }
}
