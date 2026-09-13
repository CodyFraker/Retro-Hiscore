using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class GetGamesEndpoint
{
    public static RouteHandlerBuilder MapGetGames(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/games", async (
            HttpRequest httpRequest,
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            CancellationToken ct) =>
        {
            var mediaBaseUrl = raOptions.Value.MediaBaseUrl;
            var requestBase = $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}";

            var games = await db.Games
                .OrderBy(g => g.Title)
                .Select(g => new
                {
                    g.Id,
                    g.RaGameId,
                    g.Title,
                    g.ConsoleId,
                    g.ConsoleName,
                    g.ImageBoxArt,
                    g.ImageIcon,
                    g.ImageTitle,
                    g.ImageIngame,
                    LeaderboardCount = g.Leaderboards.Count,
                    IconFileName = db.Consoles
                        .Where(c => c.RaConsoleId == g.ConsoleId)
                        .Select(c => c.IconFileName)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            var dtos = games.Select(g => new GameDto(
                g.Id,
                g.RaGameId,
                g.Title,
                g.ConsoleName,
                ConsoleIconSyncService.ToAbsoluteUrl(g.ConsoleId, g.IconFileName, requestBase),
                RaMediaUrl.ToAbsolute(g.ImageBoxArt, mediaBaseUrl),
                RaMediaUrl.ToAbsolute(g.ImageIcon, mediaBaseUrl),
                RaMediaUrl.ToAbsolute(g.ImageTitle, mediaBaseUrl),
                RaMediaUrl.ToAbsolute(g.ImageIngame, mediaBaseUrl),
                g.LeaderboardCount)).ToList();

            return Results.Ok(dtos);
        })
        .WithName("GetGames")
        .WithTags("Games")
        .RequireApiAuth();
}

public sealed record GameDto(
    Guid Id,
    int RaGameId,
    string Title,
    string? ConsoleName,
    string? ConsoleIconUrl,
    string? ImageBoxArtUrl,
    string? ImageIconUrl,
    string? ImageTitleUrl,
    string? ImageIngameUrl,
    int LeaderboardCount);
