using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;

namespace RetroHiscore.Api.Features.Dashboard;

internal static class DashboardAchievementSummaryQuery
{
    public static async Task<DashboardAchievementSummaryResponse> LoadAsync(AppDbContext db, CancellationToken ct)
    {
        var trackedIds = await db.Games.AsNoTracking().Select(g => g.RaGameId).ToListAsync(ct);
        var gameTitles = await db.Games.AsNoTracking().ToDictionaryAsync(g => g.RaGameId, g => g.Title, ct);

        var now = DateTimeOffset.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);
        var thirtyDaysAgo = now.AddDays(-30);

        var baseQuery = GetDashboardAchievementActivityEndpoint.FilteredQuery(db, trackedIds, null, null);

        var unlocksLast7Days = await baseQuery.CountAsync(m => m.DateEarned >= sevenDaysAgo, ct);
        var unlocksLast30Days = await baseQuery.CountAsync(m => m.DateEarned >= thirtyDaysAgo, ct);
        var activeMembersLast7Days = await baseQuery
            .Where(m => m.DateEarned >= sevenDaysAgo)
            .Select(m => m.MemberId)
            .Distinct()
            .CountAsync(ct);
        var lastUnlockAt = await baseQuery.MaxAsync(m => m.DateEarned, ct);

        DashboardAchievementTopGameDto? topGameLast7Days = null;
        var topGameRow = await baseQuery
            .Where(m => m.DateEarned >= sevenDaysAgo)
            .GroupBy(m => m.Achievement.RaGameId)
            .Select(g => new { RaGameId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.RaGameId)
            .FirstOrDefaultAsync(ct);

        if (topGameRow is not null)
        {
            topGameLast7Days = new DashboardAchievementTopGameDto(
                topGameRow.RaGameId,
                gameTitles.GetValueOrDefault(topGameRow.RaGameId) ?? $"Game #{topGameRow.RaGameId}",
                topGameRow.Count);
        }

        var achievementsSyncedAt = await db.Games
            .AsNoTracking()
            .Where(g => trackedIds.Contains(g.RaGameId))
            .MaxAsync(g => g.AchievementProgressSyncedAt, ct);

        return new DashboardAchievementSummaryResponse(
            unlocksLast7Days,
            unlocksLast30Days,
            activeMembersLast7Days,
            lastUnlockAt,
            topGameLast7Days,
            achievementsSyncedAt);
    }
}
