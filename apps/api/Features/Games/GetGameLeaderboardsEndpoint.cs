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
            AppDbContext db,
            IOptions<RaOptions> raOptions,
            ISyncSettingsStore syncSettingsStore,
            CancellationToken ct) =>
        {
            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
            if (game is null)
            {
                return Results.NotFound();
            }

            var engagedRows = await GameEngagedMembersQuery.GetAsync(db, game.Id, raGameId, ct);
            var members = engagedRows
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
                    l.GlobalEntryCount,
                    l.GlobalEntryCountSyncedAt,
                    standings);
            }).ToList();

            var images = RaMediaUrl.FromGame(game, raOptions.Value.MediaBaseUrl);
            var console = game.ConsoleId is null
                ? null
                : await db.Consoles.AsNoTracking().FirstOrDefaultAsync(c => c.RaConsoleId == game.ConsoleId, ct);

            var syncStatusByGameId = await GameLeaderboardSyncStatusQuery.GetForGamesAsync(
                db,
                syncSettingsStore,
                [new GameLeaderboardSyncStatusQuery.GameSyncInput(
                    game.Id,
                    game.RaGameId,
                    game.LeaderboardScoresSyncedAt,
                    game.ForceColdLeaderboardSync)],
                ct);
            var leaderboardSyncStatus = syncStatusByGameId[game.Id];

            return Results.Ok(new GameLeaderboardsResponse(
                game.Id,
                game.RaGameId,
                game.Title,
                game.ConsoleName,
                ConsoleIconSyncService.ToDataUrl(console?.IconData, console?.IconContentType),
                images.ImageBoxArtUrl,
                images.ImageIconUrl,
                images.ImageTitleUrl,
                images.ImageIngameUrl,
                game.Publisher,
                game.Developer,
                game.Genre,
                game.ReleasedAt,
                game.MetadataSyncedAt,
                game.LeaderboardScoresSyncedAt,
                leaderboardSyncStatus,
                members,
                response));
        })
        .WithName("GetGameLeaderboards")
        .WithTags("Games")
        .WithSummary("Returns friend standings for every tracked leaderboard on a game (engaged members only), including hot/cold leaderboard sync schedule.")
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
    int? GlobalEntryCount,
    DateTimeOffset? GlobalEntryCountSyncedAt,
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
    DateTimeOffset? LeaderboardScoresSyncedAt,
    GameLeaderboardSyncStatusDto LeaderboardSyncStatus,
    IReadOnlyList<StandingMemberDto> Members,
    IReadOnlyList<GameLeaderboardDto> Leaderboards);
