using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberAchievementsEndpoint
{
    public static RouteHandlerBuilder MapGetMemberAchievements(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/{raUsername}/achievements", HandleAsync)
        .WithName("GetMemberAchievements")
        .WithTags("Members")
        .WithSummary("Returns paginated achievement unlocks for a member.")
        .RequireApiAuth();

    private static async Task<IResult> HandleAsync(
        string raUsername,
        AppDbContext db,
        IOptions<RaOptions> raOptions,
        int? limit,
        int? offset,
        bool? trackedOnly,
        CancellationToken ct)
    {
        var member = await db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.RaUsername != null && EF.Functions.ILike(m.RaUsername, raUsername),
                ct);

        if (member is null)
        {
            return Results.NotFound();
        }

        var take = Math.Clamp(limit ?? 50, 1, 200);
        var skip = Math.Max(offset ?? 0, 0);
        var mediaBaseUrl = raOptions.Value.MediaBaseUrl;
        var trackedSet = await db.Games.AsNoTracking().Select(g => g.RaGameId).ToListAsync(ct);
        var trackedHash = trackedSet.ToHashSet();

        var query = db.MemberRaAchievements
            .AsNoTracking()
            .Where(m => m.MemberId == member.Id && m.DateEarned != null);

        if (trackedOnly == true)
        {
            query = query.Where(m => trackedHash.Contains(m.Achievement.RaGameId));
        }

        var total = await query.CountAsync(ct);

        var gameTitles = await db.Games.AsNoTracking().ToDictionaryAsync(g => g.RaGameId, g => g.Title, ct);

        var rows = await query
            .OrderByDescending(m => m.DateEarned)
            .ThenByDescending(m => m.RaAchievementId)
            .Skip(skip)
            .Take(take)
            .Select(m => new
            {
                m.RaAchievementId,
                m.DateEarned,
                m.DateEarnedHardcore,
                m.Achievement.RaGameId,
                m.Achievement.Title,
                m.Achievement.Description,
                m.Achievement.Points,
                m.Achievement.BadgeData,
                m.Achievement.BadgeContentType,
                m.Achievement.BadgeName
            })
            .ToListAsync(ct);

        var items = rows.Select(r => new MemberAchievementListItemDto(
            r.RaAchievementId,
            r.RaGameId,
            gameTitles.GetValueOrDefault(r.RaGameId) ?? $"Game #{r.RaGameId}",
            trackedHash.Contains(r.RaGameId),
            r.Title,
            r.Description,
            r.Points,
            ConsoleIconSyncService.ToDataUrl(r.BadgeData, r.BadgeContentType)
                ?? RaMediaUrl.ToBadgeUrl(r.BadgeName, mediaBaseUrl),
            r.DateEarned,
            r.DateEarnedHardcore.HasValue)).ToList();

        var achievementsProgressSyncedAt =
            await MemberAchievementProgressFreshness.GetProgressSyncedAtAsync(db, member.Id, ct);

        return Results.Ok(new MemberAchievementsListResponse(
            total,
            skip,
            take,
            items,
            achievementsProgressSyncedAt));
    }
}

public sealed record MemberAchievementListItemDto(
    int RaAchievementId,
    int RaGameId,
    string GameTitle,
    bool IsTracked,
    string Title,
    string? Description,
    int Points,
    string? BadgeUrl,
    DateTimeOffset? DateEarned,
    bool HardcoreEarned);

public sealed record MemberAchievementsListResponse(
    int Total,
    int Offset,
    int Limit,
    IReadOnlyList<MemberAchievementListItemDto> Items,
    DateTimeOffset? AchievementsProgressSyncedAt = null);
