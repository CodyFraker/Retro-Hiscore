using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class GetGameLeaderboardsEndpoint
{
    public static RouteHandlerBuilder MapGetGameLeaderboards(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/games/{raGameId:int}/leaderboards", async (
            int raGameId,
            HttpRequest httpRequest,
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            CancellationToken ct) =>
        {
            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
            if (game is null)
            {
                return Results.NotFound();
            }

            var memberRows = await db.Members
                .OrderBy(m => m.RaUsername)
                .Select(m => new { m.Id, m.RaUsername, DisplayName = m.DisplayName ?? m.RaUsername, m.AvatarUrl })
                .ToListAsync(ct);

            var members = memberRows
                .Select(m => new StandingMemberDto(m.Id, m.RaUsername, m.DisplayName, m.AvatarUrl))
                .ToList();

            var leaderboards = await db.Leaderboards
                .Include(l => l.Entries)
                .Where(l => l.GameId == game.Id)
                .OrderBy(l => l.Title)
                .ToListAsync(ct);

            var response = leaderboards.Select(l =>
            {
                var standings = members.Select(m =>
                {
                    var entry = l.Entries.FirstOrDefault(e => e.MemberId == m.Id);
                    return new FriendStandingDto(
                        m.Id,
                        m.RaUsername,
                        m.DisplayName,
                        m.AvatarUrl,
                        entry?.Score,
                        entry?.FormattedScore,
                        entry?.GlobalRank,
                        entry?.FriendRank,
                        entry?.ScoreUpdatedAt);
                }).ToList();

                return new GameLeaderboardDto(
                    l.Id,
                    l.RaLeaderboardId,
                    l.Title,
                    l.Description,
                    l.Format,
                    l.RankAsc,
                    standings);
            }).ToList();

            var images = RaMediaUrl.FromGame(game, raOptions.Value.MediaBaseUrl);
            var console = game.ConsoleId is null
                ? null
                : await db.Consoles.AsNoTracking().FirstOrDefaultAsync(c => c.RaConsoleId == game.ConsoleId, ct);
            var requestBase = $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}";

            return Results.Ok(new GameLeaderboardsResponse(
                game.Id,
                game.RaGameId,
                game.Title,
                game.ConsoleName,
                ConsoleIconSyncService.ToAbsoluteUrl(game.ConsoleId, console?.IconFileName, requestBase),
                images.ImageBoxArtUrl,
                images.ImageIconUrl,
                images.ImageTitleUrl,
                images.ImageIngameUrl,
                game.Publisher,
                game.Developer,
                game.Genre,
                game.ReleasedAt,
                game.MetadataSyncedAt,
                members,
                response));
        })
        .WithName("GetGameLeaderboards")
        .WithTags("Games")
        .RequireApiAuth();
}

public sealed record StandingMemberDto(Guid Id, string RaUsername, string DisplayName, string? AvatarUrl);

public sealed record FriendStandingDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    long? Score,
    string? FormattedScore,
    int? GlobalRank,
    int? FriendRank,
    DateTimeOffset? ScoreUpdatedAt);

public sealed record GameLeaderboardDto(
    Guid Id,
    long RaLeaderboardId,
    string Title,
    string? Description,
    string? Format,
    bool RankAsc,
    IReadOnlyList<FriendStandingDto> Standings);

public sealed record GameLeaderboardsResponse(
    Guid GameId,
    int RaGameId,
    string Title,
    string? ConsoleName,
    string? ConsoleIconUrl,
    string? ImageBoxArtUrl,
    string? ImageIconUrl,
    string? ImageTitleUrl,
    string? ImageIngameUrl,
    string? Publisher,
    string? Developer,
    string? Genre,
    DateTimeOffset? ReleasedAt,
    DateTimeOffset? MetadataSyncedAt,
    IReadOnlyList<StandingMemberDto> Members,
    IReadOnlyList<GameLeaderboardDto> Leaderboards);
