using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class GetGameLeaderboardPopulationHistoryEndpoint
{
    public static RouteHandlerBuilder MapGetGameLeaderboardPopulationHistory(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/games/{raGameId:int}/leaderboard-population-history", async (
            int raGameId,
            AppDbContext db,
            int? limit,
            CancellationToken ct) =>
        {
            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
            if (game is null)
            {
                return Results.NotFound();
            }

            var takePerBoard = Math.Clamp(limit ?? 100, 1, 500);

            var leaderboards = await db.Leaderboards
                .AsNoTracking()
                .Where(l => l.GameId == game.Id)
                .OrderBy(l => l.Title)
                .ToListAsync(ct);

            var leaderboardIds = leaderboards.Select(l => l.Id).ToList();
            var snapshots = await db.LeaderboardPopulationSnapshots
                .AsNoTracking()
                .Where(p => leaderboardIds.Contains(p.LeaderboardId))
                .OrderByDescending(p => p.SyncedAt)
                .ToListAsync(ct);

            var boards = leaderboards.Select(l =>
            {
                var points = snapshots
                    .Where(p => p.LeaderboardId == l.Id)
                    .GroupBy(p => p.SyncedAt)
                    .Select(g => g.First())
                    .OrderByDescending(p => p.SyncedAt)
                    .Take(takePerBoard)
                    .OrderBy(p => p.SyncedAt)
                    .Select(p => new LeaderboardPopulationPointDto(p.SyncedAt, p.EntryCount))
                    .ToList();

                return new LeaderboardPopulationSeriesDto(
                    l.RaLeaderboardId,
                    l.Title,
                    points);
            }).ToList();

            return Results.Ok(new GameLeaderboardPopulationHistoryResponse(boards));
        })
        .WithName("GetGameLeaderboardPopulationHistory")
        .WithTags("Games")
        .WithSummary("Returns per-board ranked player count snapshots over time for a tracked game.")
        .RequireApiAuth();
}

public sealed record LeaderboardPopulationPointDto(DateTimeOffset SyncedAt, int EntryCount);

public sealed record LeaderboardPopulationSeriesDto(
    long RaLeaderboardId,
    string Title,
    IReadOnlyList<LeaderboardPopulationPointDto> Points);

public sealed record GameLeaderboardPopulationHistoryResponse(
    IReadOnlyList<LeaderboardPopulationSeriesDto> Boards);
