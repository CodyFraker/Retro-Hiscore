using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class GetGameAchievementsEndpoint
{
    public static RouteHandlerBuilder MapGetGameAchievements(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/games/{raGameId:int}/achievements", HandleAsync)
        .WithName("GetGameAchievements")
        .WithTags("Games")
        .WithSummary("Returns achievement catalog, friend unlocks, and group completion stats for a tracked game.")
        .RequireApiAuth();

    private static async Task<IResult> HandleAsync(
        int raGameId,
        AppDbContext db,
        IOptions<RaOptions> raOptions,
        CancellationToken ct)
    {
        var game = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
        if (game is null)
        {
            return Results.NotFound();
        }

        var mediaBaseUrl = raOptions.Value.MediaBaseUrl;

        var memberRows = await db.Members
            .AsNoTracking()
            .Where(m => m.RaUsername != null)
            .OrderBy(m => m.RaUsername)
            .Select(m => new GameAchievementMemberDto(
                m.Id,
                m.RaUsername!,
                m.DisplayName ?? m.RaUsername!,
                m.AvatarUrl))
            .ToListAsync(ct);

        var catalog = await db.RaAchievements
            .AsNoTracking()
            .Where(a => a.RaGameId == raGameId)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.RaAchievementId)
            .Select(a => new
            {
                a.RaAchievementId,
                a.Title,
                a.Description,
                a.Points,
                a.TrueRatio,
                a.BadgeData,
                a.BadgeContentType,
                a.BadgeName,
                a.DisplayOrder
            })
            .ToListAsync(ct);

        var catalogDtos = catalog
            .Select(a => new GameAchievementCatalogItemDto(
                a.RaAchievementId,
                a.Title,
                a.Description,
                a.Points,
                a.TrueRatio,
                ConsoleIconSyncService.ToDataUrl(a.BadgeData, a.BadgeContentType)
                    ?? RaMediaUrl.ToBadgeUrl(a.BadgeName, mediaBaseUrl),
                a.DisplayOrder))
            .ToList();

        var totalAchievements = catalogDtos.Count;
        var totalPoints = catalogDtos.Sum(a => a.Points);

        var memberIds = memberRows.Select(m => m.MemberId).ToList();
        var unlockRows = await db.MemberRaAchievements
            .AsNoTracking()
            .Where(m => memberIds.Contains(m.MemberId) && m.Achievement.RaGameId == raGameId)
            .Select(m => new
            {
                m.MemberId,
                m.RaAchievementId,
                m.DateEarned,
                m.DateEarnedHardcore
            })
            .ToListAsync(ct);

        var unlockDtos = unlockRows
            .Select(u => new GameAchievementMemberUnlockDto(
                u.MemberId,
                u.RaAchievementId,
                u.DateEarned,
                u.DateEarnedHardcore.HasValue))
            .ToList();

        var unlocksByMember = unlockRows
            .GroupBy(u => u.MemberId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var summaries = new List<GameAchievementMemberSummaryDto>();
        GameAchievementMemberSummaryDto? leader = null;

        foreach (var member in memberRows)
        {
            unlocksByMember.TryGetValue(member.MemberId, out var memberUnlocks);
            var earned = memberUnlocks?.Count ?? 0;
            var earnedIds = memberUnlocks is null
                ? new HashSet<int>()
                : memberUnlocks.Select(x => x.RaAchievementId).ToHashSet();
            var pointsEarned = catalogDtos.Where(a => earnedIds.Contains(a.RaAchievementId)).Sum(a => a.Points);
            DateTimeOffset? lastUnlock = null;
            if (memberUnlocks is not null)
            {
                lastUnlock = memberUnlocks
                    .Where(x => x.DateEarned != null)
                    .MaxBy(x => x.DateEarned)?
                    .DateEarned;
            }

            var summary = new GameAchievementMemberSummaryDto(
                member.MemberId,
                earned,
                totalAchievements,
                pointsEarned,
                totalPoints,
                lastUnlock);

            summaries.Add(summary);

            if (leader is null
                || summary.AchievementsEarned > leader.AchievementsEarned
                || (summary.AchievementsEarned == leader.AchievementsEarned
                    && summary.PointsEarned > leader.PointsEarned))
            {
                leader = summary;
            }
        }

        var membersMastered = totalAchievements > 0
            ? summaries.Count(s => s.AchievementsEarned >= totalAchievements)
            : 0;

        var catalogLookup = catalog.ToDictionary(a => a.RaAchievementId);
        var recentUnlocks = unlockRows
            .Where(u => u.DateEarned != null)
            .OrderByDescending(u => u.DateEarned)
            .Take(15)
            .Select(u =>
            {
                var member = memberRows.First(m => m.MemberId == u.MemberId);
                catalogLookup.TryGetValue(u.RaAchievementId, out var ach);
                return new GameAchievementRecentUnlockDto(
                    u.MemberId,
                    member.RaUsername,
                    member.DisplayName,
                    member.AvatarUrl,
                    u.RaAchievementId,
                    ach?.Title ?? $"#{u.RaAchievementId}",
                    ach?.Points ?? 0,
                    ach is null
                        ? null
                        : ConsoleIconSyncService.ToDataUrl(ach.BadgeData, ach.BadgeContentType)
                            ?? RaMediaUrl.ToBadgeUrl(ach.BadgeName, mediaBaseUrl),
                    u.DateEarned,
                    u.DateEarnedHardcore.HasValue);
            })
            .ToList();

        return Results.Ok(new GameAchievementsResponse(
            raGameId,
            game.Title,
            game.AchievementProgressSyncedAt,
            catalogDtos,
            memberRows,
            unlockDtos,
            summaries,
            leader,
            membersMastered,
            recentUnlocks));
    }
}
