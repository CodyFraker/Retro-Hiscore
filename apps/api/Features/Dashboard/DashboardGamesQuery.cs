using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public enum DashboardGameSort
{
    Recent,
    Name,
    Boards,
    Population
}

public static class DashboardGamesQuery
{
    public static DashboardGameSort ParseSort(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return DashboardGameSort.Recent;
        }

        return raw.Trim().ToLowerInvariant() switch
        {
            "name" => DashboardGameSort.Name,
            "boards" => DashboardGameSort.Boards,
            "population" => DashboardGameSort.Population,
            _ => DashboardGameSort.Recent
        };
    }

    public static async Task<DashboardGamesResponse> GetPageAsync(
        AppDbContext db,
        IOptions<RaOptions> raOptions,
        int? limit,
        int? offset,
        string? sort,
        string? query,
        CancellationToken ct)
    {
        var take = Math.Clamp(limit ?? 20, 1, 100);
        var skip = Math.Max(offset ?? 0, 0);
        var sortKey = ParseSort(sort);
        var normalizedQuery = query?.Trim();

        var all = await BuildAllAsync(db, raOptions, ct);
        IEnumerable<DashboardGameDto> filtered = all;

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            filtered = all.Where(g =>
                g.Title.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                || (g.ConsoleName?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var sorted = Sort(filtered, sortKey).ToList();
        var total = sorted.Count;
        var items = sorted.Skip(skip).Take(take).ToList();

        return new DashboardGamesResponse(total, skip, take, items);
    }

    public static IEnumerable<DashboardGameDto> Sort(IEnumerable<DashboardGameDto> games, DashboardGameSort sortKey)
    {
        return sortKey switch
        {
            DashboardGameSort.Name => games.OrderBy(g => g.Title, StringComparer.OrdinalIgnoreCase),
            DashboardGameSort.Boards => games
                .OrderByDescending(g => g.LeaderboardCount)
                .ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase),
            DashboardGameSort.Population => games
                .OrderByDescending(g => g.MaxGlobalEntryCount ?? 0)
                .ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase),
            _ => games
                .OrderByDescending(g => g.LastActivityAt ?? DateTimeOffset.MinValue)
                .ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase)
        };
    }

    public static async Task<IReadOnlyList<DashboardGameDto>> BuildAllAsync(
        AppDbContext db,
        IOptions<RaOptions> raOptions,
        CancellationToken ct)
    {
        var mediaBaseUrl = raOptions.Value.MediaBaseUrl;

        var gamesRaw = await db.Games
            .OrderBy(g => g.Title)
            .Select(g => new
            {
                g.Id,
                g.RaGameId,
                g.Title,
                g.ConsoleId,
                g.ConsoleName,
                g.ImageBoxArt,
                g.ImageIcon,
                g.ImageTitle,
                g.ImageIngame,
                g.LeaderboardScoresSyncedAt,
                LeaderboardCount = g.Leaderboards.Count,
                ConsoleIconData = db.Consoles
                    .Where(c => c.RaConsoleId == g.ConsoleId)
                    .Select(c => c.IconData)
                    .FirstOrDefault(),
                ConsoleIconContentType = db.Consoles
                    .Where(c => c.RaConsoleId == g.ConsoleId)
                    .Select(c => c.IconContentType)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var gameIds = gamesRaw.Select(g => g.Id).ToList();
        var raGameIds = gamesRaw.Select(g => g.RaGameId).ToList();

        var catalogCounts = await db.RaAchievements
            .AsNoTracking()
            .Where(a => raGameIds.Contains(a.RaGameId))
            .GroupBy(a => a.RaGameId)
            .Select(g => new { RaGameId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RaGameId, x => x.Count, ct);

        var gameEntries = await db.LeaderboardEntries
            .Include(e => e.Member)
            .Include(e => e.Leaderboard)
            .Where(e => gameIds.Contains(e.Leaderboard.GameId))
            .ToListAsync(ct);

        return gamesRaw.Select(g =>
        {
            var entriesForGame = gameEntries.Where(e => e.Leaderboard.GameId == g.Id).ToList();
            var winRows = entriesForGame
                .GroupBy(e => e.MemberId)
                .Select(memberGroup =>
                {
                    var member = memberGroup.First().Member;
                    return new
                    {
                        RaUsername = member.RaUsername ?? string.Empty,
                        DisplayName = MemberAuthHelper.DisplayLabel(member),
                        member.AvatarUrl,
                        FriendRankOnes = memberGroup.Count(e => e.FriendRank == 1)
                    };
                })
                .Where(row => row.FriendRankOnes > 0)
                .OrderByDescending(row => row.FriendRankOnes)
                .ThenBy(row => row.DisplayName)
                .FirstOrDefault();

            var lastActivityAt = entriesForGame
                .Where(e => e.ScoreUpdatedAt is not null)
                .Select(e => e.ScoreUpdatedAt)
                .OrderByDescending(d => d)
                .FirstOrDefault();

            var boardPopulations = entriesForGame
                .Select(e => e.Leaderboard)
                .DistinctBy(l => l.Id)
                .Where(l => l.GlobalEntryCount is not null)
                .ToList();

            var busiestBoard = boardPopulations
                .OrderByDescending(l => l.GlobalEntryCount)
                .FirstOrDefault();

            DashboardGameLeaderDto? leader = winRows is null
                ? null
                : new DashboardGameLeaderDto(winRows.DisplayName, winRows.RaUsername, winRows.AvatarUrl, winRows.FriendRankOnes);

            var playersWithAvatars = entriesForGame
                .GroupBy(e => e.MemberId)
                .Select(memberGroup =>
                {
                    var member = memberGroup.First().Member;
                    return new
                    {
                        RaUsername = member.RaUsername ?? string.Empty,
                        DisplayName = MemberAuthHelper.DisplayLabel(member),
                        member.AvatarUrl
                    };
                })
                .Where(row => !string.IsNullOrWhiteSpace(row.AvatarUrl))
                .Select(row => new DashboardGamePlayerAvatarDto(
                    row.DisplayName,
                    row.RaUsername,
                    row.AvatarUrl!))
                .OrderBy(row => row.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            catalogCounts.TryGetValue(g.RaGameId, out var catalogTotal);

            return new DashboardGameDto(
                g.Id,
                g.RaGameId,
                g.Title,
                g.ConsoleName,
                ConsoleIconSyncService.ToDataUrl(g.ConsoleIconData, g.ConsoleIconContentType),
                RaMediaUrl.ToAbsolute(g.ImageBoxArt, mediaBaseUrl),
                RaMediaUrl.ToAbsolute(g.ImageIcon, mediaBaseUrl),
                RaMediaUrl.ToAbsolute(g.ImageTitle, mediaBaseUrl),
                RaMediaUrl.ToAbsolute(g.ImageIngame, mediaBaseUrl),
                g.LeaderboardCount,
                busiestBoard?.GlobalEntryCount,
                busiestBoard?.Title,
                leader,
                playersWithAvatars,
                lastActivityAt,
                g.LeaderboardScoresSyncedAt,
                catalogTotal > 0 ? catalogTotal : null);
        }).ToList();
    }
}

public sealed record DashboardGamesResponse(
    int Total,
    int Offset,
    int Limit,
    IReadOnlyList<DashboardGameDto> Items);
