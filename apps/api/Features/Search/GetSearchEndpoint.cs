using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Search;

public static class GetSearchEndpoint
{
    public static RouteHandlerBuilder MapGetSearch(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/search", async (
            AppDbContext db,
            string? q,
            int? limit,
            CancellationToken ct) =>
        {
            var term = q?.Trim();
            if (string.IsNullOrWhiteSpace(term))
            {
                return Results.Ok(new SearchResponse([]));
            }

            PlatformMetrics.RecordSearchExecuted();

            var take = Math.Clamp(limit ?? 20, 1, 50);
            var pattern = $"%{term}%";
            var perKind = Math.Max(take / 3, 5);

            var games = await db.Games
                .AsNoTracking()
                .Where(g =>
                    EF.Functions.ILike(g.Title, pattern)
                    || (g.ConsoleName != null && EF.Functions.ILike(g.ConsoleName, pattern)))
                .OrderBy(g => g.Title)
                .Take(perKind)
                .Select(g => new SearchHitDto(
                    "Game",
                    g.Title,
                    g.ConsoleName,
                    $"/games/{g.RaGameId}",
                    g.RaGameId,
                    null,
                    null))
                .ToListAsync(ct);

            var members = await db.Members
                .AsNoTracking()
                .Where(m => m.RaUsername != null
                    && (EF.Functions.ILike(m.RaUsername!, pattern)
                        || (m.DisplayName != null && EF.Functions.ILike(m.DisplayName, pattern))))
                .OrderBy(m => m.RaUsername)
                .Take(perKind)
                .Select(m => new SearchHitDto(
                    "Member",
                    m.DisplayName ?? m.RaUsername!,
                    m.RaUsername,
                    $"/members/{m.RaUsername}",
                    null,
                    m.RaUsername,
                    null))
                .ToListAsync(ct);

            var leaderboards = await db.Leaderboards
                .AsNoTracking()
                .Include(l => l.Game)
                .Where(l =>
                    EF.Functions.ILike(l.Title, pattern)
                    || EF.Functions.ILike(l.Game.Title, pattern))
                .OrderBy(l => l.Game.Title)
                .ThenBy(l => l.Title)
                .Take(perKind)
                .Select(l => new SearchHitDto(
                    "Leaderboard",
                    l.Title,
                    l.Game.Title,
                    $"/leaderboards/{l.RaLeaderboardId}",
                    l.Game.RaGameId,
                    null,
                    l.RaLeaderboardId))
                .ToListAsync(ct);

            var hits = games
                .Concat(members)
                .Concat(leaderboards)
                .Take(take)
                .ToList();

            return Results.Ok(new SearchResponse(hits));
        })
        .WithName("GetSearch")
        .WithTags("Search")
        .WithSummary("Searches tracked games, members, and leaderboards by title or name.")
        .RequireApiAuth();
}

public sealed record SearchHitDto(
    string Kind,
    string Title,
    string? Subtitle,
    string Href,
    int? RaGameId,
    string? RaUsername,
    long? RaLeaderboardId);

public sealed record SearchResponse(IReadOnlyList<SearchHitDto> Hits);
