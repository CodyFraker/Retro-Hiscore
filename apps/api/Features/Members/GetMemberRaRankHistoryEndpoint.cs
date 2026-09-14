using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberRaRankHistoryEndpoint
{
    public static RouteHandlerBuilder MapGetMemberRaRankHistory(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/{raUsername}/ra-rank-history", async (
            string raUsername,
            AppDbContext db,
            int? limit,
            CancellationToken ct) =>
        {
            var member = await db.Members
                .FirstOrDefaultAsync(m => EF.Functions.ILike(m.RaUsername, raUsername), ct);

            if (member is null)
            {
                return Results.NotFound();
            }

            var take = Math.Clamp(limit ?? 200, 1, 500);

            var recent = await db.MemberRaRankSnapshots
                .AsNoTracking()
                .Where(s => s.MemberId == member.Id)
                .OrderByDescending(s => s.SyncedAt)
                .Take(take)
                .ToListAsync(ct);

            var items = recent
                .OrderBy(s => s.SyncedAt)
                .Select(s => new MemberRaRankHistoryItemDto(
                    s.SyncedAt,
                    s.Rank,
                    s.TotalRanked,
                    s.TotalPoints,
                    s.TotalTruePoints,
                    s.TotalSoftcorePoints))
                .ToList();

            return Results.Ok(new MemberRaRankHistoryResponse(items));
        })
        .WithName("GetMemberRaRankHistory")
        .WithTags("Members")
        .WithSummary("Returns RetroAchievements rank and points snapshots recorded during score syncs.")
        .RequireApiAuth();
}

public sealed record MemberRaRankHistoryItemDto(
    DateTimeOffset SyncedAt,
    int? Rank,
    int? TotalRanked,
    int? TotalPoints,
    int? TotalTruePoints,
    int? TotalSoftcorePoints);

public sealed record MemberRaRankHistoryResponse(
    IReadOnlyList<MemberRaRankHistoryItemDto> Items);
