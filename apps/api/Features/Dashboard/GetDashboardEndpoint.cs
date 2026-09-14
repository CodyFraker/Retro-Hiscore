using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetDashboardEndpoint
{
    public static RouteHandlerBuilder MapGetDashboard(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard", async (
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            CancellationToken ct) =>
        {
            var mediaBaseUrl = raOptions.Value.MediaBaseUrl;

            var entries = await db.LeaderboardEntries
                .Include(e => e.Member)
                .ToListAsync(ct);

            var championship = entries
                .GroupBy(e => e.MemberId)
                .Select(g =>
                {
                    var member = g.First().Member;
                    return new ChampionshipRowDto(
                        g.Key,
                        member.RaUsername ?? string.Empty,
                        MemberAuthHelper.DisplayLabel(member),
                        member.AvatarUrl,
                        g.Count(e => e.FriendRank == 1),
                        g.Count());
                })
                .OrderByDescending(c => c.FriendRankOnes)
                .ThenBy(c => c.DisplayName)
                .ToList();

            var activity = await ActivityBuilder.BuildAsync(db, ct);

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
            var gameEntries = await db.LeaderboardEntries
                .Include(e => e.Member)
                .Include(e => e.Leaderboard)
                .Where(e => gameIds.Contains(e.Leaderboard.GameId))
                .ToListAsync(ct);

            var games = gamesRaw.Select(g =>
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

                DashboardGameLeaderDto? leader = winRows is null
                    ? null
                    : new DashboardGameLeaderDto(winRows.DisplayName, winRows.RaUsername, winRows.AvatarUrl, winRows.FriendRankOnes);

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
                    leader,
                    lastActivityAt);
            })
            .OrderByDescending(g => g.LastActivityAt ?? DateTimeOffset.MinValue)
            .ThenBy(g => g.Title)
            .ToList();

            return Results.Ok(new DashboardResponse(championship, activity, games));
        })
        .WithName("GetDashboard")
        .WithTags("Dashboard")
        .WithSummary("Returns championship standings, recent activity, and enriched game cards for the home dashboard.")
        .RequireApiAuth();
}

public sealed record ChampionshipRowDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    int FriendRankOnes,
    int BoardsWithScore);

public sealed record ActivityItemDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    int RaGameId,
    string GameTitle,
    long RaLeaderboardId,
    string LeaderboardTitle,
    long? ScoreDelta,
    int? FriendRankDelta,
    string? FormattedScore);

public sealed record DashboardGameLeaderDto(string DisplayName, string RaUsername, string? AvatarUrl, int FriendRankOnes);

public sealed record DashboardGameDto(
    Guid Id,
    int RaGameId,
    string Title,
    string? ConsoleName,
    string? ConsoleIconUrl,
    string? ImageBoxArtUrl,
    string? ImageIconUrl,
    string? ImageTitleUrl,
    string? ImageIngameUrl,
    int LeaderboardCount,
    DashboardGameLeaderDto? FriendRankOneLeader,
    DateTimeOffset? LastActivityAt);

public sealed record DashboardResponse(
    IReadOnlyList<ChampionshipRowDto> Championship,
    IReadOnlyList<ActivityItemDto> Activity,
    IReadOnlyList<DashboardGameDto> Games);
