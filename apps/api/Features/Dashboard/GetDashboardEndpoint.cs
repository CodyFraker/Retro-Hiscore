using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetDashboardEndpoint
{
    public static RouteHandlerBuilder MapGetDashboard(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard", async (
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            IOptions<SyncOptions> syncOptions,
            CancellationToken ct) =>
        {
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

            var recentGroupGames = await RecentGroupGamesBuilder.BuildAsync(db, syncOptions, raOptions, ct);

            return Results.Ok(new DashboardResponse(championship, recentGroupGames));
        })
        .WithName("GetDashboard")
        .WithTags("Dashboard")
        .WithSummary("Returns championship standings and group recent games for the home dashboard.")
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
    int? GlobalRank,
    int? GlobalEntryCount,
    int? GlobalRankDelta,
    string? FormattedScore);

public sealed record DashboardGameLeaderDto(string DisplayName, string RaUsername, string? AvatarUrl, int FriendRankOnes);

public sealed record DashboardGamePlayerAvatarDto(string DisplayName, string RaUsername, string AvatarUrl);

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
    int? MaxGlobalEntryCount,
    string? MaxGlobalEntryCountLeaderboardTitle,
    DashboardGameLeaderDto? FriendRankOneLeader,
    IReadOnlyList<DashboardGamePlayerAvatarDto> PlayersWithAvatars,
    DateTimeOffset? LastActivityAt,
    DateTimeOffset? LeaderboardScoresSyncedAt,
    int? TotalAchievementsInCatalog);

public sealed record RecentGroupGamePlayerDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    DateTimeOffset LastPlayedAt);

public sealed record RecentGroupGameDto(
    int RaGameId,
    string Title,
    int ConsoleId,
    string? ConsoleName,
    string? ConsoleIconUrl,
    string? ImageBoxArtUrl,
    string? ImageIconUrl,
    DateTimeOffset LastPlayedAt,
    bool IsTracked,
    IReadOnlyList<RecentGroupGamePlayerDto> Players);

public sealed record DashboardResponse(
    IReadOnlyList<ChampionshipRowDto> Championship,
    IReadOnlyList<RecentGroupGameDto> RecentGroupGames);
