using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberRaAchievementHistoryEndpoint
{
    public static RouteHandlerBuilder MapGetMemberRaAchievementHistory(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/{raUsername}/ra-achievement-history", async (
            string raUsername,
            AppDbContext db,
            int? limit,
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

            var take = Math.Clamp(limit ?? 500, 1, 2000);

            var unlocks = await db.MemberRaAchievements
                .AsNoTracking()
                .Where(m => m.MemberId == member.Id && m.DateEarned != null)
                .OrderBy(m => m.DateEarned)
                .ThenBy(m => m.RaAchievementId)
                .Take(take)
                .Select(m => new
                {
                    m.DateEarned,
                    m.Achievement.Points,
                    m.Achievement.TrueRatio
                })
                .ToListAsync(ct);

            var cumulativeUnlocks = 0;
            var cumulativePoints = 0;
            var cumulativeTruePoints = 0;
            var items = new List<MemberRaAchievementHistoryItemDto>(unlocks.Count);

            foreach (var unlock in unlocks)
            {
                cumulativeUnlocks++;
                cumulativePoints += unlock.Points;
                cumulativeTruePoints += unlock.TrueRatio;
                items.Add(new MemberRaAchievementHistoryItemDto(
                    unlock.DateEarned!.Value,
                    cumulativeUnlocks,
                    cumulativePoints,
                    cumulativeTruePoints));
            }

            return Results.Ok(new MemberRaAchievementHistoryResponse(items));
        })
        .WithName("GetMemberRaAchievementHistory")
        .WithTags("Members")
        .WithSummary("Returns cumulative RetroAchievements unlock history for charting, ordered by unlock time.")
        .RequireApiAuth();
}

public sealed record MemberRaAchievementHistoryItemDto(
    DateTimeOffset EarnedAt,
    int CumulativeUnlocks,
    int CumulativePoints,
    int CumulativeTruePoints);

public sealed record MemberRaAchievementHistoryResponse(
    IReadOnlyList<MemberRaAchievementHistoryItemDto> Items);
