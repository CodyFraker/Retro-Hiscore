using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Features.Ra;

namespace RetroHiscore.Api.Features.Sync;

public interface IMemberRaGameProgressSyncService
{
    Task SyncMemberGameProgressAsync(
        Member member,
        int raGameId,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default);

    Task SyncMemberTrackedGamesAsync(
        Member member,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default);

    Task SyncMemberGamesFromSummaryAsync(
        Member member,
        RaUserSummaryDto summary,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default);
}

public sealed class MemberRaGameProgressSyncService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    IConsoleIconDownloader badgeDownloader,
    INotificationOutboxWriter notificationOutboxWriter,
    IOptions<RaOptions> raOptions,
    ILogger<MemberRaGameProgressSyncService> logger) : IMemberRaGameProgressSyncService
{
    private readonly RaOptions _raOptions = raOptions.Value;

    public async Task SyncMemberGamesFromSummaryAsync(
        Member member,
        RaUserSummaryDto summary,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default)
    {
        var gameIds = CollectGameIds(summary);
        foreach (var gameId in gameIds)
        {
            await SyncMemberGameProgressAsync(member, gameId, syncedAt, cancellationToken);
        }
    }

    public async Task SyncMemberTrackedGamesAsync(
        Member member,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(member.RaUsername) || string.IsNullOrWhiteSpace(member.RaApiKey))
        {
            return;
        }

        var trackedGameIds = await db.Games
            .AsNoTracking()
            .Select(g => g.RaGameId)
            .ToListAsync(cancellationToken);

        foreach (var raGameId in trackedGameIds)
        {
            await SyncMemberGameProgressAsync(member, raGameId, syncedAt, cancellationToken);
        }
    }

    public async Task SyncMemberGameProgressAsync(
        Member member,
        int raGameId,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(member.RaUsername) || string.IsNullOrWhiteSpace(member.RaApiKey))
        {
            return;
        }

        var identity = !string.IsNullOrWhiteSpace(member.RaUlid) ? member.RaUlid! : member.RaUsername!;
        var mediaBaseUrl = _raOptions.MediaBaseUrl;

        try
        {
            var progress = await apiKeyPool.ExecuteAsync(
                [member.RaApiKey],
                (key, ct) => raApiClient.GetGameInfoAndUserProgressAsync(raGameId, identity, key, ct),
                cancellationToken);

            if (progress?.Achievements is null || progress.Achievements.Count == 0)
            {
                return;
            }

            foreach (var achievement in progress.Achievements.Values)
            {
                await UpsertAchievementCatalogAsync(
                    raGameId,
                    achievement,
                    syncedAt,
                    mediaBaseUrl,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(achievement.DateEarned))
                {
                    await EnsureMemberUnlockAsync(
                        member,
                        raGameId,
                        achievement,
                        syncedAt,
                        cancellationToken);
                }
            }

            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, cancellationToken);
            if (game is not null)
            {
                game.AchievementProgressSyncedAt = syncedAt;
                await db.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to sync RA game progress for member {Member} game {GameId}",
                member.RaUsername,
                raGameId);
        }
    }

    private static HashSet<int> CollectGameIds(RaUserSummaryDto summary)
    {
        var ids = new HashSet<int>();
        if (summary.LastGame is not null)
        {
            ids.Add(summary.LastGame.Id);
        }

        if (summary.RecentlyPlayed is not null)
        {
            foreach (var game in summary.RecentlyPlayed)
            {
                ids.Add(game.GameId);
            }
        }

        return ids;
    }

    private async Task UpsertAchievementCatalogAsync(
        int raGameId,
        RaGameAchievementProgressDto source,
        DateTimeOffset syncedAt,
        string mediaBaseUrl,
        CancellationToken cancellationToken)
    {
        var existing = await db.RaAchievements
            .FirstOrDefaultAsync(a => a.RaAchievementId == source.Id, cancellationToken);

        if (existing is null)
        {
            existing = new RaAchievement
            {
                RaAchievementId = source.Id,
                RaGameId = raGameId,
                Title = source.Title,
                Description = source.Description,
                Points = source.Points,
                TrueRatio = source.TrueRatio,
                BadgeName = source.BadgeName,
                DisplayOrder = source.DisplayOrder,
                Type = source.Type,
                SyncedAt = syncedAt
            };
            db.RaAchievements.Add(existing);
        }
        else
        {
            existing.RaGameId = raGameId;
            existing.Title = source.Title;
            existing.Description = source.Description;
            existing.Points = source.Points;
            existing.TrueRatio = source.TrueRatio;
            existing.BadgeName = source.BadgeName;
            existing.DisplayOrder = source.DisplayOrder;
            existing.Type = source.Type;
            existing.SyncedAt = syncedAt;
        }

        if (existing.BadgeData is null or { Length: 0 })
        {
            var badgeUrl = RaMediaUrl.ToBadgeUrl(source.BadgeName, mediaBaseUrl);
            if (!string.IsNullOrWhiteSpace(badgeUrl))
            {
                try
                {
                    var download = await badgeDownloader.DownloadAsync(badgeUrl, cancellationToken);
                    existing.BadgeData = download.Data;
                    existing.BadgeContentType = download.ContentType;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Failed to download RA achievement badge {BadgeName}",
                        source.BadgeName);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureMemberUnlockAsync(
        Member member,
        int raGameId,
        RaGameAchievementProgressDto source,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken)
    {
        var exists = await db.MemberRaAchievements
            .AnyAsync(
                m => m.MemberId == member.Id && m.RaAchievementId == source.Id,
                cancellationToken);

        if (exists)
        {
            return;
        }

        db.MemberRaAchievements.Add(new MemberRaAchievement
        {
            MemberId = member.Id,
            RaAchievementId = source.Id,
            DateEarned = ParseRaDateTime(source.DateEarned),
            DateEarnedHardcore = ParseRaDateTime(source.DateEarnedHardcore),
            FirstDetectedAt = syncedAt
        });

        await db.SaveChangesAsync(cancellationToken);

        var gameTitle = await db.Games
            .AsNoTracking()
            .Where(g => g.RaGameId == raGameId)
            .Select(g => g.Title)
            .FirstOrDefaultAsync(cancellationToken) ?? $"Game #{raGameId}";

        var earnedAt = ParseRaDateTime(source.DateEarned) ?? syncedAt;
        var payload = new AchievementUnlockedNotificationPayload(
            member.Id,
            member.RaUsername ?? "",
            member.DisplayName ?? member.RaUsername ?? "",
            raGameId,
            gameTitle,
            source.Id,
            source.Title ?? $"Achievement #{source.Id}",
            source.Points,
            !string.IsNullOrWhiteSpace(source.DateEarnedHardcore),
            earnedAt);

        await notificationOutboxWriter.EnqueueAsync(
            DiscordNotificationEventKind.AchievementUnlocked,
            payload,
            cancellationToken);
    }

    private static DateTimeOffset? ParseRaDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            return parsed;
        }

        return null;
    }
}
