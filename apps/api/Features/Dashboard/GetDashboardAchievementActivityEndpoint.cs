using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Games;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class GetDashboardAchievementActivityEndpoint
{
    public static RouteHandlerBuilder MapGetDashboardAchievementActivity(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/dashboard/achievement-activity", HandleAsync)
        .WithName("GetDashboardAchievementActivity")
        .WithTags("Dashboard")
        .WithSummary("Returns paginated recent achievement unlocks across tracked games for all members.")
        .RequireApiAuth();

    private static async Task<IResult> HandleAsync(
        AppDbContext db,
        IOptions<RaOptions> raOptions,
        int? limit,
        int? offset,
        int? raGameId,
        string? raUsername,
        CancellationToken ct)
    {
        var take = Math.Clamp(limit ?? 25, 1, 100);
        var skip = Math.Max(offset ?? 0, 0);
        var mediaBaseUrl = raOptions.Value.MediaBaseUrl;
        var trackedIds = await db.Games.AsNoTracking().Select(g => g.RaGameId).ToListAsync(ct);
        var gameTitles = await db.Games.AsNoTracking().ToDictionaryAsync(g => g.RaGameId, g => g.Title, ct);

        var query = FilteredQuery(db, trackedIds, raGameId, raUsername);

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(m => m.DateEarned)
            .Skip(skip)
            .Take(take)
            .Select(m => new
            {
                m.RaAchievementId,
                m.DateEarned,
                m.DateEarnedHardcore,
                m.Achievement.RaGameId,
                m.Achievement.Title,
                m.Achievement.Points,
                m.Achievement.BadgeData,
                m.Achievement.BadgeContentType,
                m.Achievement.BadgeName,
                m.MemberId,
                m.Member.RaUsername,
                DisplayName = m.Member.DisplayName ?? m.Member.RaUsername!,
                m.Member.AvatarUrl
            })
            .ToListAsync(ct);

        var items = rows.Select(r => new DashboardAchievementActivityItemDto(
            r.MemberId,
            r.RaUsername ?? string.Empty,
            r.DisplayName,
            r.AvatarUrl,
            r.RaGameId,
            gameTitles.GetValueOrDefault(r.RaGameId) ?? $"Game #{r.RaGameId}",
            r.RaAchievementId,
            r.Title,
            r.Points,
            ConsoleIconSyncService.ToDataUrl(r.BadgeData, r.BadgeContentType)
                ?? RaMediaUrl.ToBadgeUrl(r.BadgeName, mediaBaseUrl),
            r.DateEarned,
            r.DateEarnedHardcore.HasValue)).ToList();

        return Results.Ok(new DashboardAchievementActivityResponse(total, skip, take, items));
    }

    internal static IQueryable<MemberRaAchievement> FilteredQuery(
        AppDbContext db,
        IReadOnlyList<int> trackedIds,
        int? raGameId,
        string? raUsername)
    {
        IQueryable<MemberRaAchievement> query = db.MemberRaAchievements
            .AsNoTracking()
            .Where(m => m.DateEarned != null && trackedIds.Contains(m.Achievement.RaGameId));

        if (raGameId is { } gameId)
        {
            query = query.Where(m => m.Achievement.RaGameId == gameId);
        }

        if (!string.IsNullOrWhiteSpace(raUsername))
        {
            query = query.Where(m =>
                m.Member.RaUsername != null
                && EF.Functions.ILike(m.Member.RaUsername, raUsername));
        }

        return query;
    }
}

public sealed record DashboardAchievementActivityItemDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    int RaGameId,
    string GameTitle,
    int RaAchievementId,
    string Title,
    int Points,
    string? BadgeUrl,
    DateTimeOffset? DateEarned,
    bool HardcoreEarned);

public sealed record DashboardAchievementActivityResponse(
    int Total,
    int Offset,
    int Limit,
    IReadOnlyList<DashboardAchievementActivityItemDto> Items);
