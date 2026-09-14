using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Leaderboards;

public static class GetLeaderboardEndpoint
{
    public static RouteHandlerBuilder MapGetLeaderboard(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/leaderboards/{raLeaderboardId:long}", async (long raLeaderboardId, AppDbContext db, CancellationToken ct) =>
        {
            var leaderboard = await db.Leaderboards
                .Include(l => l.Game)
                .Include(l => l.Entries)
                .FirstOrDefaultAsync(l => l.RaLeaderboardId == raLeaderboardId, ct);

            if (leaderboard is null)
            {
                return Results.NotFound();
            }

            var members = await db.Members
                .Where(m => m.RaUsername != null)
                .OrderBy(m => m.RaUsername)
                .ToListAsync(ct);

            var standings = members.Select(m =>
            {
                var entry = leaderboard.Entries.FirstOrDefault(e => e.MemberId == m.Id);
                return new FriendStandingDto(
                    m.Id,
                    m.RaUsername!,
                    MemberAuthHelper.DisplayLabel(m),
                    m.AvatarUrl,
                    entry?.Score,
                    entry?.FormattedScore,
                    entry?.GlobalRank,
                    entry?.FriendRank,
                    entry?.ScoreUpdatedAt);
            })
            .OrderBy(s => s.FriendRank ?? int.MaxValue)
            .ThenBy(s => s.RaUsername)
            .ToList();

            return Results.Ok(new LeaderboardDetailDto(
                leaderboard.Id,
                leaderboard.RaLeaderboardId,
                leaderboard.Game.RaGameId,
                leaderboard.Game.Title,
                leaderboard.Title,
                leaderboard.Description,
                leaderboard.Format,
                leaderboard.RankAsc,
                leaderboard.GlobalEntryCount,
                leaderboard.GlobalEntryCountSyncedAt,
                standings));
        })
        .WithName("GetLeaderboard")
        .WithTags("Leaderboards")
        .WithSummary("Returns friend standings and metadata for a single leaderboard.")
        .RequireApiAuth();
}

public sealed record LeaderboardDetailDto(
    Guid Id,
    long RaLeaderboardId,
    int RaGameId,
    string GameTitle,
    string Title,
    string? Description,
    string? Format,
    bool RankAsc,
    int? GlobalEntryCount,
    DateTimeOffset? GlobalEntryCountSyncedAt,
    IReadOnlyList<FriendStandingDto> Standings);
