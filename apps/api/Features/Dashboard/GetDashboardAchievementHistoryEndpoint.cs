using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetDashboardAchievementHistoryEndpoint
{
    public static RouteHandlerBuilder MapGetDashboardAchievementHistory(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard/achievement-history", HandleAsync)
        .WithName("GetDashboardAchievementHistory")
        .WithTags("Dashboard")
        .WithSummary("Returns cumulative group unlock history for tracked games.")
        .RequireApiAuth();

    private static async Task<IResult> HandleAsync(
        AppDbContext db,
        int? limit,
        CancellationToken ct)
    {
        var take = Math.Clamp(limit ?? 2000, 1, 5000);
        var trackedIds = await db.Games.AsNoTracking().Select(g => g.RaGameId).ToListAsync(ct);

        var unlocks = await db.MemberRaAchievements
            .AsNoTracking()
            .Where(m => m.DateEarned != null && trackedIds.Contains(m.Achievement.RaGameId))
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
        var items = new List<DashboardAchievementHistoryItemDto>(unlocks.Count);

        foreach (var unlock in unlocks)
        {
            cumulativeUnlocks++;
            cumulativePoints += unlock.Points;
            cumulativeTruePoints += unlock.TrueRatio;
            items.Add(new DashboardAchievementHistoryItemDto(
                unlock.DateEarned!.Value,
                cumulativeUnlocks,
                cumulativePoints,
                cumulativeTruePoints));
        }

        return Results.Ok(new DashboardAchievementHistoryResponse(items));
    }
}

public sealed record DashboardAchievementHistoryItemDto(
    DateTimeOffset EarnedAt,
    int CumulativeUnlocks,
    int CumulativePoints,
    int CumulativeTruePoints);

public sealed record DashboardAchievementHistoryResponse(
    IReadOnlyList<DashboardAchievementHistoryItemDto> Items);
