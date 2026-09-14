using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberRaSummaryEndpoint
{
    public static RouteHandlerBuilder MapGetMemberRaSummary(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/{raUsername}/ra-summary", async (
            string raUsername,
            AppDbContext db,
            IRaApiClient raApiClient,
            IRaApiKeyPool apiKeyPool,
            IOptions<RaOptions> raOptions,
            CancellationToken ct) =>
        {
            var member = await db.Members
                .FirstOrDefaultAsync(m => EF.Functions.ILike(m.RaUsername, raUsername), ct);

            if (member is null)
            {
                return Results.NotFound();
            }

            var targetUsername = member.RaUsername ?? raUsername;
            var preferredKeys = string.IsNullOrWhiteSpace(member.RaApiKey)
                ? Array.Empty<string>()
                : new[] { member.RaApiKey };

            RaUserSummaryDto? raw;
            try
            {
                raw = await apiKeyPool.ExecuteAsync(
                    preferredKeys,
                    (key, token) => raApiClient.GetUserSummaryAsync(
                        targetUsername,
                        key,
                        recentGamesCount: 3,
                        recentAchievementsCount: 8,
                        cancellationToken: token),
                    ct);
            }
            catch (InvalidOperationException)
            {
                return Results.Ok(new MemberRaSummaryResponse(
                    false,
                    "No RetroAchievements API keys are configured.",
                    null));
            }
            catch (Exception)
            {
                return Results.Ok(new MemberRaSummaryResponse(
                    false,
                    "Could not load RetroAchievements summary right now.",
                    null));
            }

            if (raw is null || string.IsNullOrWhiteSpace(raw.User))
            {
                return Results.Ok(new MemberRaSummaryResponse(
                    false,
                    "RetroAchievements did not return a summary for this user.",
                    null));
            }

            var mediaBaseUrl = raOptions.Value.MediaBaseUrl;
            var siteBaseUrl = RaMediaUrl.SiteBaseUrlFromApiBase(raOptions.Value.BaseUrl);

            var consoleIds = new HashSet<int>();
            if (raw.LastGame is not null)
            {
                consoleIds.Add(raw.LastGame.ConsoleId);
            }

            if (raw.RecentlyPlayed is not null)
            {
                foreach (var game in raw.RecentlyPlayed)
                {
                    consoleIds.Add(game.ConsoleId);
                }
            }

            var consoleRows = await db.Consoles
                .AsNoTracking()
                .Where(c => consoleIds.Contains(c.RaConsoleId))
                .Select(c => new { c.RaConsoleId, c.IconData, c.IconContentType })
                .ToListAsync(ct);

            var consoleIcons = consoleRows.ToDictionary(
                c => c.RaConsoleId,
                c => ConsoleIconSyncService.ToDataUrl(c.IconData, c.IconContentType));

            var trackedGameIds = await db.Games
                .AsNoTracking()
                .Select(g => g.RaGameId)
                .ToListAsync(ct);
            var trackedSet = trackedGameIds.ToHashSet();

            var awarded = raw.Awarded ?? new Dictionary<string, RaUserSummaryAwardedDto>();

            MemberRaGameProgressDto? ProgressFor(int gameId, int? achievementsTotalFallback)
            {
                if (!awarded.TryGetValue(gameId.ToString(), out var a))
                {
                    if (achievementsTotalFallback is null)
                    {
                        return null;
                    }

                    return new MemberRaGameProgressDto(
                        0,
                        achievementsTotalFallback.Value,
                        0,
                        null,
                        null,
                        null);
                }

                return new MemberRaGameProgressDto(
                    a.NumAchieved ?? 0,
                    a.NumPossibleAchievements ?? achievementsTotalFallback ?? 0,
                    a.ScoreAchieved ?? 0,
                    a.PossibleScore,
                    a.NumAchievedHardcore,
                    a.ScoreAchievedHardcore);
            }

            MemberRaPresenceGameDto? presence = null;
            if (raw.LastGame is not null)
            {
                presence = MapPresenceGame(
                    raw.LastGame.Id,
                    raw.LastGame.Title,
                    raw.LastGame.ConsoleId,
                    raw.LastGame.ConsoleName,
                    raw.LastGame.ImageBoxArt,
                    raw.LastGame.ImageIcon,
                    raw.RichPresenceMsg,
                    raw.RichPresenceMsgDate,
                    ProgressFor(raw.LastGame.Id, null),
                    consoleIcons,
                    mediaBaseUrl,
                    trackedSet);
            }

            var recentlyPlayed = (raw.RecentlyPlayed ?? [])
                .Select(g => MapRecentGame(
                    g.GameId,
                    g.Title,
                    g.ConsoleId,
                    g.ConsoleName,
                    g.ImageBoxArt,
                    g.ImageIcon,
                    g.LastPlayed,
                    ProgressFor(g.GameId, g.AchievementsTotal),
                    consoleIcons,
                    mediaBaseUrl,
                    trackedSet))
                .ToList();

            var recentAchievements = FlattenRecentAchievements(raw.RecentAchievements, mediaBaseUrl, trackedSet);

            var gameIdsForUnlocks = new HashSet<int>();
            if (presence is not null)
            {
                gameIdsForUnlocks.Add(presence.RaGameId);
            }

            foreach (var game in recentlyPlayed)
            {
                gameIdsForUnlocks.Add(game.RaGameId);
            }

            var unlocksByGame = await LoadUnlockedAchievementsByGameAsync(
                db,
                member.Id,
                gameIdsForUnlocks,
                ct);

            if (presence is not null)
            {
                presence = presence with
                {
                    UnlockedAchievements = unlocksByGame.GetValueOrDefault(presence.RaGameId) ?? []
                };
            }

            recentlyPlayed = recentlyPlayed
                .Select(g => g with
                {
                    UnlockedAchievements = unlocksByGame.GetValueOrDefault(g.RaGameId) ?? []
                })
                .ToList();

            var summary = new MemberRaSummaryDto(
                raw.User,
                raw.Ulid,
                FormatRaMotto(raw.Motto),
                RaMediaUrl.ToAbsolute(raw.UserPic, siteBaseUrl),
                ParseRaDateTime(raw.MemberSince),
                FormatRaStatus(raw.Status),
                raw.Rank,
                raw.TotalRanked,
                raw.TotalPoints,
                raw.TotalSoftcorePoints,
                raw.TotalTruePoints,
                presence,
                recentlyPlayed,
                recentAchievements);

            return Results.Ok(new MemberRaSummaryResponse(true, null, summary));
        })
        .WithName("GetMemberRaSummary")
        .WithTags("Members")
        .WithSummary("Fetches normalized RetroAchievements user summary for a member profile.")
        .RequireApiAuth();

    private static MemberRaPresenceGameDto MapPresenceGame(
        int gameId,
        string title,
        int consoleId,
        string? consoleName,
        string? imageBoxArt,
        string? imageIcon,
        string? richPresenceMsg,
        string? richPresenceMsgDate,
        MemberRaGameProgressDto? progress,
        IReadOnlyDictionary<int, string?> consoleIcons,
        string mediaBaseUrl,
        HashSet<int> trackedGameIds)
        => new(
            gameId,
            title,
            consoleId,
            consoleName,
            consoleIcons.GetValueOrDefault(consoleId),
            RaMediaUrl.ToAbsolute(imageBoxArt, mediaBaseUrl),
            RaMediaUrl.ToAbsolute(imageIcon, mediaBaseUrl),
            trackedGameIds.Contains(gameId),
            richPresenceMsg,
            ParseRaDateTime(richPresenceMsgDate),
            progress,
            []);

    private static MemberRaRecentGameDto MapRecentGame(
        int gameId,
        string title,
        int consoleId,
        string? consoleName,
        string? imageBoxArt,
        string? imageIcon,
        string? lastPlayed,
        MemberRaGameProgressDto? progress,
        IReadOnlyDictionary<int, string?> consoleIcons,
        string mediaBaseUrl,
        HashSet<int> trackedGameIds)
        => new(
            gameId,
            title,
            consoleId,
            consoleName,
            consoleIcons.GetValueOrDefault(consoleId),
            RaMediaUrl.ToAbsolute(imageBoxArt, mediaBaseUrl),
            RaMediaUrl.ToAbsolute(imageIcon, mediaBaseUrl),
            trackedGameIds.Contains(gameId),
            ParseRaDateTime(lastPlayed),
            progress,
            []);

    private static async Task<Dictionary<int, List<MemberRaUnlockedAchievementDto>>> LoadUnlockedAchievementsByGameAsync(
        AppDbContext db,
        Guid memberId,
        IReadOnlySet<int> gameIds,
        CancellationToken cancellationToken)
    {
        if (gameIds.Count == 0)
        {
            return new Dictionary<int, List<MemberRaUnlockedAchievementDto>>();
        }

        var rows = await db.MemberRaAchievements
            .AsNoTracking()
            .Where(m => m.MemberId == memberId && gameIds.Contains(m.Achievement.RaGameId))
            .Select(m => new
            {
                m.RaAchievementId,
                m.DateEarned,
                m.DateEarnedHardcore,
                m.Achievement.RaGameId,
                m.Achievement.Title,
                m.Achievement.Description,
                m.Achievement.Points,
                m.Achievement.DisplayOrder,
                m.Achievement.BadgeData,
                m.Achievement.BadgeContentType
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(r => r.RaGameId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderBy(r => r.DisplayOrder)
                    .ThenByDescending(r => r.DateEarned ?? DateTimeOffset.MinValue)
                    .Select(r => new MemberRaUnlockedAchievementDto(
                        r.RaAchievementId,
                        r.Title,
                        r.Description,
                        r.Points,
                        ConsoleIconSyncService.ToDataUrl(r.BadgeData, r.BadgeContentType),
                        r.DateEarned,
                        r.DateEarnedHardcore.HasValue))
                    .ToList());
    }

    private static List<MemberRaRecentAchievementDto> FlattenRecentAchievements(
        Dictionary<string, Dictionary<string, RaUserSummaryRecentAchievementDto>>? nested,
        string mediaBaseUrl,
        HashSet<int> trackedGameIds)
    {
        if (nested is null || nested.Count == 0)
        {
            return [];
        }

        var list = new List<MemberRaRecentAchievementDto>();
        foreach (var gameBucket in nested.Values)
        {
            foreach (var achievement in gameBucket.Values)
            {
                list.Add(new MemberRaRecentAchievementDto(
                    achievement.Id,
                    achievement.GameId,
                    achievement.GameTitle,
                    achievement.Title,
                    achievement.Description,
                    achievement.Points,
                    RaMediaUrl.ToBadgeUrl(achievement.BadgeName, mediaBaseUrl),
                    ParseRaDateTime(achievement.DateAwarded),
                    achievement.HardcoreAchieved == 1,
                    trackedGameIds.Contains(achievement.GameId)));
            }
        }

        return list
            .OrderByDescending(a => a.DateAwarded ?? DateTimeOffset.MinValue)
            .ToList();
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

    private static string? FormatRaMotto(JsonElement motto)
    {
        if (motto.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (motto.ValueKind == JsonValueKind.String)
        {
            return motto.GetString();
        }

        if (motto.ValueKind == JsonValueKind.Object)
        {
            if (motto.TryGetProperty("Motto", out var nested) && nested.ValueKind == JsonValueKind.String)
            {
                return nested.GetString();
            }

            if (motto.TryGetProperty("motto", out var nestedCamel) && nestedCamel.ValueKind == JsonValueKind.String)
            {
                return nestedCamel.GetString();
            }
        }

        return null;
    }

    private static string? FormatRaStatus(JsonElement status)
    {
        if (status.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (status.ValueKind == JsonValueKind.String)
        {
            return status.GetString();
        }

        if (status.ValueKind == JsonValueKind.Object
            && status.TryGetProperty("Status", out var inner)
            && inner.ValueKind == JsonValueKind.String)
        {
            return inner.GetString();
        }

        return null;
    }
}

public sealed record MemberRaSummaryResponse(
    bool Available,
    string? UnavailableReason,
    MemberRaSummaryDto? Summary);

public sealed record MemberRaSummaryDto(
    string RaUsername,
    string? RaUlid,
    string? Motto,
    string? UserPicUrl,
    DateTimeOffset? MemberSince,
    string? Status,
    int? Rank,
    int? TotalRanked,
    int? TotalPoints,
    int? TotalSoftcorePoints,
    int? TotalTruePoints,
    MemberRaPresenceGameDto? Presence,
    IReadOnlyList<MemberRaRecentGameDto> RecentlyPlayed,
    IReadOnlyList<MemberRaRecentAchievementDto> RecentAchievements);

public sealed record MemberRaPresenceGameDto(
    int RaGameId,
    string Title,
    int ConsoleId,
    string? ConsoleName,
    string? ConsoleIconUrl,
    string? ImageBoxArtUrl,
    string? ImageIconUrl,
    bool IsTracked,
    string? RichPresenceMsg,
    DateTimeOffset? RichPresenceAt,
    MemberRaGameProgressDto? Progress,
    IReadOnlyList<MemberRaUnlockedAchievementDto> UnlockedAchievements);

public sealed record MemberRaRecentGameDto(
    int RaGameId,
    string Title,
    int ConsoleId,
    string? ConsoleName,
    string? ConsoleIconUrl,
    string? ImageBoxArtUrl,
    string? ImageIconUrl,
    bool IsTracked,
    DateTimeOffset? LastPlayedAt,
    MemberRaGameProgressDto? Progress,
    IReadOnlyList<MemberRaUnlockedAchievementDto> UnlockedAchievements);

public sealed record MemberRaUnlockedAchievementDto(
    int RaAchievementId,
    string Title,
    string? Description,
    int Points,
    string? BadgeUrl,
    DateTimeOffset? DateEarned,
    bool HardcoreAchieved);

public sealed record MemberRaGameProgressDto(
    int AchievementsEarned,
    int AchievementsTotal,
    int PointsEarned,
    int? PointsPossible,
    int? AchievementsEarnedHardcore,
    int? PointsEarnedHardcore);

public sealed record MemberRaRecentAchievementDto(
    int RaAchievementId,
    int RaGameId,
    string GameTitle,
    string Title,
    string? Description,
    int Points,
    string? BadgeUrl,
    DateTimeOffset? DateAwarded,
    bool HardcoreAchieved,
    bool IsTracked);
