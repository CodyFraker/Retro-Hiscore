using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GroupActivityBuilder
{
    public static async Task<DashboardGroupActivityResponse> BuildAsync(
        AppDbContext db,
        int limit,
        CancellationToken ct)
    {
        var window = await ActivityBuilder.TryResolveGlobalSnapshotWindowAsync(db, ct);
        if (window is null)
        {
            return new DashboardGroupActivityResponse(true, null, null, []);
        }

        var take = Math.Clamp(limit, 1, 100);
        var items = new List<DashboardGroupActivityItemDto>();

        var leaderboardMoves = await ActivityBuilder.BuildAsync(db, ct);
        foreach (var move in leaderboardMoves)
        {
            var title = move.FriendRankDelta is { } frd && frd != 0
                ? $"{move.DisplayName} moved on {move.LeaderboardTitle}"
                : $"{move.DisplayName} scored on {move.LeaderboardTitle}";
            var subtitle = move.FormattedScore;
            items.Add(new DashboardGroupActivityItemDto(
                "LeaderboardMove",
                window.CurrentSync,
                title,
                subtitle,
                move.RaUsername,
                move.DisplayName,
                move.RaGameId,
                move.RaLeaderboardId,
                move.FriendRankDelta,
                move.ScoreDelta));
        }

        var trackedIds = await db.Games.AsNoTracking().Select(g => g.RaGameId).ToListAsync(ct);
        var gameTitles = await db.Games.AsNoTracking().ToDictionaryAsync(g => g.RaGameId, g => g.Title, ct);
        var achievementRows = await GetDashboardAchievementActivityEndpoint.FilteredQuery(
                db,
                trackedIds,
                raGameId: null,
                raUsername: null)
            .Where(m => m.DateEarned != null && m.DateEarned > window.PreviousSync && m.DateEarned <= window.CurrentSync)
            .OrderByDescending(m => m.DateEarned)
            .Take(take)
            .Select(m => new
            {
                m.DateEarned,
                m.Achievement.RaGameId,
                m.Achievement.Title,
                m.Member.RaUsername,
                DisplayName = m.Member.DisplayName ?? m.Member.RaUsername!
            })
            .ToListAsync(ct);

        foreach (var row in achievementRows)
        {
            items.Add(new DashboardGroupActivityItemDto(
                "AchievementUnlock",
                row.DateEarned ?? window.CurrentSync,
                $"{row.DisplayName} unlocked {row.Title}",
                gameTitles.GetValueOrDefault(row.RaGameId),
                row.RaUsername,
                row.DisplayName,
                row.RaGameId,
                null,
                null,
                null));
        }

        var trackedGames = await db.NotificationOutbox
            .AsNoTracking()
            .Where(o => o.EventKind == DiscordNotificationEventKind.GameTracked
                && o.OccurredAt > window.PreviousSync
                && o.OccurredAt <= window.CurrentSync)
            .OrderByDescending(o => o.OccurredAt)
            .Take(take)
            .ToListAsync(ct);

        foreach (var entry in trackedGames)
        {
            var raGameId = TryReadRaGameId(entry.PayloadJson);
            var title = TryReadTitle(entry.PayloadJson) ?? (raGameId is not null ? $"Game #{raGameId}" : "New game");
            items.Add(new DashboardGroupActivityItemDto(
                "GameTracked",
                entry.OccurredAt,
                $"{title} added to tracked games",
                null,
                null,
                null,
                raGameId,
                null,
                null,
                null));
        }

        var ordered = items
            .OrderByDescending(i => i.OccurredAt)
            .Take(take)
            .ToList();

        return new DashboardGroupActivityResponse(false, window.PreviousSync, window.CurrentSync, ordered);
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

    private static string? TryReadTitle(string payloadJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            if (doc.RootElement.TryGetProperty("title", out var prop))
            {
                return prop.GetString();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}

public sealed record DashboardGroupActivityItemDto(
    string Kind,
    DateTimeOffset OccurredAt,
    string Title,
    string? Subtitle,
    string? MemberRaUsername,
    string? MemberDisplayName,
    int? RaGameId,
    long? RaLeaderboardId,
    int? FriendRankDelta,
    long? ScoreDelta);

public sealed record DashboardGroupActivityResponse(
    bool WindowUnavailable,
    DateTimeOffset? WindowStart,
    DateTimeOffset? WindowEnd,
    IReadOnlyList<DashboardGroupActivityItemDto> Items);
