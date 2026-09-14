using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;

namespace RetroHiscore.Api.Features.Sync;

public interface IMemberRaGameProgressSyncService
{
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
        if (string.IsNullOrWhiteSpace(member.RaUsername) || string.IsNullOrWhiteSpace(member.RaApiKey))
        {
            return;
        }

        var gameIds = CollectGameIds(summary);
        if (gameIds.Count == 0)
        {
            return;
        }

        var identity = !string.IsNullOrWhiteSpace(member.RaUlid) ? member.RaUlid! : member.RaUsername!;
        var mediaBaseUrl = _raOptions.MediaBaseUrl;

        foreach (var gameId in gameIds)
        {
            try
            {
                var progress = await apiKeyPool.ExecuteAsync(
                    [member.RaApiKey],
                    (key, ct) => raApiClient.GetGameInfoAndUserProgressAsync(gameId, identity, key, ct),
                    cancellationToken);

                if (progress?.Achievements is null || progress.Achievements.Count == 0)
                {
                    continue;
                }

                foreach (var achievement in progress.Achievements.Values)
                {
                    if (string.IsNullOrWhiteSpace(achievement.DateEarned))
                    {
                        continue;
                    }

                    await UpsertAchievementCatalogAsync(
                        gameId,
                        achievement,
                        syncedAt,
                        mediaBaseUrl,
                        cancellationToken);

                    await EnsureMemberUnlockAsync(
                        member.Id,
                        achievement,
                        syncedAt,
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to sync RA game progress for member {Member} game {GameId}",
                    member.RaUsername,
                    gameId);
            }
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
        Guid memberId,
        RaGameAchievementProgressDto source,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken)
    {
        var exists = await db.MemberRaAchievements
            .AnyAsync(
                m => m.MemberId == memberId && m.RaAchievementId == source.Id,
                cancellationToken);

        if (exists)
        {
            return;
        }

        db.MemberRaAchievements.Add(new MemberRaAchievement
        {
            MemberId = memberId,
            RaAchievementId = source.Id,
            DateEarned = ParseRaDateTime(source.DateEarned),
            DateEarnedHardcore = ParseRaDateTime(source.DateEarnedHardcore),
            FirstDetectedAt = syncedAt
        });

        await db.SaveChangesAsync(cancellationToken);
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
