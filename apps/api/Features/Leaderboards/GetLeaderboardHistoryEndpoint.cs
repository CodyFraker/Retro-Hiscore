using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Leaderboards;

public static class GetLeaderboardHistoryEndpoint
{
    public static RouteHandlerBuilder MapGetLeaderboardHistory(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/leaderboards/{raLeaderboardId:long}/history", async (
            long raLeaderboardId,
            AppDbContext db,
            int? limit,
            int? offset,
            CancellationToken ct) =>
        {
            var take = Math.Clamp(limit ?? 50, 1, 200);
            var skip = Math.Max(offset ?? 0, 0);

            var leaderboard = await db.Leaderboards
                .FirstOrDefaultAsync(l => l.RaLeaderboardId == raLeaderboardId, ct);

            if (leaderboard is null)
            {
                return Results.NotFound();
            }

            var query = db.LeaderboardEntrySnapshots
                .Where(s => s.LeaderboardId == leaderboard.Id)
                .OrderByDescending(s => s.SyncedAt)
                .ThenBy(s => s.Member.RaUsername);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip(skip)
                .Take(take)
                .Select(s => new LeaderboardHistoryItemDto(
                    s.Id,
                    s.MemberId,
                    s.Member.RaUsername ?? string.Empty,
                    s.Member.DisplayName ?? s.Member.RaUsername ?? string.Empty,
                    s.Member.AvatarUrl,
                    s.Score,
                    s.FormattedScore,
                    s.GlobalRank,
                    s.FriendRank,
                    s.SyncedAt))
                .ToListAsync(ct);

            return Results.Ok(new LeaderboardHistoryResponse(total, skip, take, items));
        })
        .WithName("GetLeaderboardHistory")
        .WithTags("Leaderboards")
        .RequireApiAuth();
}

public sealed record LeaderboardHistoryItemDto(
    Guid Id,
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    long Score,
    string FormattedScore,
    int? GlobalRank,
    int? FriendRank,
    DateTimeOffset SyncedAt);

public sealed record LeaderboardHistoryResponse(
    int Total,
    int Offset,
    int Limit,
    IReadOnlyList<LeaderboardHistoryItemDto> Items);
