using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberHistoryEndpoint
{
    public static RouteHandlerBuilder MapGetMemberHistory(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/{raUsername}/history", async (
            string raUsername,
            AppDbContext db,
            int? limit,
            int? offset,
            CancellationToken ct) =>
        {
            var member = await db.Members
                .FirstOrDefaultAsync(
                    m => m.RaUsername != null && EF.Functions.ILike(m.RaUsername, raUsername),
                    ct);

            if (member is null)
            {
                return Results.NotFound();
            }

            var take = Math.Clamp(limit ?? 50, 1, 200);
            var skip = Math.Max(offset ?? 0, 0);

            var query = db.LeaderboardEntrySnapshots
                .Where(s => s.MemberId == member.Id)
                .OrderByDescending(s => s.SyncedAt)
                .ThenBy(s => s.Leaderboard.Title);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip(skip)
                .Take(take)
                .Select(s => new MemberHistoryItemDto(
                    s.Id,
                    s.Leaderboard.Game.RaGameId,
                    s.Leaderboard.Game.Title,
                    s.Leaderboard.RaLeaderboardId,
                    s.Leaderboard.Title,
                    s.Leaderboard.Format,
                    s.Score,
                    s.FormattedScore,
                    s.GlobalRank,
                    s.FriendRank,
                    s.SyncedAt))
                .ToListAsync(ct);

            return Results.Ok(new MemberHistoryResponse(total, skip, take, items));
        })
        .WithName("GetMemberHistory")
        .WithTags("Members")
        .WithSummary("Returns paginated leaderboard snapshot history for a tracked member.")
        .RequireApiAuth();
}

public sealed record MemberHistoryItemDto(
    Guid Id,
    int RaGameId,
    string GameTitle,
    long RaLeaderboardId,
    string LeaderboardTitle,
    string? Format,
    long Score,
    string FormattedScore,
    int? GlobalRank,
    int? FriendRank,
    DateTimeOffset SyncedAt);

public sealed record MemberHistoryResponse(
    int Total,
    int Offset,
    int Limit,
    IReadOnlyList<MemberHistoryItemDto> Items);
