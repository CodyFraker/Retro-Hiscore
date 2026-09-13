using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class GetGameHistoryEndpoint
{
    public static RouteHandlerBuilder MapGetGameHistory(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/games/{raGameId:int}/history", async (
            int raGameId,
            AppDbContext db,
            int? limit,
            int? offset,
            CancellationToken ct) =>
        {
            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
            if (game is null)
            {
                return Results.NotFound();
            }

            var take = Math.Clamp(limit ?? 50, 1, 200);
            var skip = Math.Max(offset ?? 0, 0);

            var query = db.LeaderboardEntrySnapshots
                .Where(s => s.Leaderboard.GameId == game.Id)
                .OrderByDescending(s => s.SyncedAt)
                .ThenBy(s => s.Leaderboard.Title)
                .ThenBy(s => s.Member.RaUsername);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip(skip)
                .Take(take)
                .Select(s => new GameHistoryItemDto(
                    s.Id,
                    s.MemberId,
                    s.Member.RaUsername,
                    s.Member.DisplayName ?? s.Member.RaUsername,
                    s.Leaderboard.RaLeaderboardId,
                    s.Leaderboard.Title,
                    s.Leaderboard.Format,
                    s.Score,
                    s.FormattedScore,
                    s.GlobalRank,
                    s.FriendRank,
                    s.SyncedAt))
                .ToListAsync(ct);

            return Results.Ok(new GameHistoryResponse(total, skip, take, items));
        })
        .WithName("GetGameHistory")
        .WithTags("Games")
        .WithSummary("Returns paginated leaderboard snapshot history for a tracked game.")
        .RequireApiAuth();
}

public sealed record GameHistoryItemDto(
    Guid Id,
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    long RaLeaderboardId,
    string LeaderboardTitle,
    string? Format,
    long Score,
    string FormattedScore,
    int? GlobalRank,
    int? FriendRank,
    DateTimeOffset SyncedAt);

public sealed record GameHistoryResponse(
    int Total,
    int Offset,
    int Limit,
    IReadOnlyList<GameHistoryItemDto> Items);
