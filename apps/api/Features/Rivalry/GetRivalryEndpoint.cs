using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Rivalry;

public static class GetRivalryEndpoint
{
    public static RouteHandlerBuilder MapGetRivalry(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/rivalry/{usernameA}/{usernameB}", async (
            string usernameA,
            string usernameB,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var memberA = await db.Members
                .FirstOrDefaultAsync(
                    m => m.RaUsername != null && EF.Functions.ILike(m.RaUsername, usernameA),
                    ct);
            var memberB = await db.Members
                .FirstOrDefaultAsync(
                    m => m.RaUsername != null && EF.Functions.ILike(m.RaUsername, usernameB),
                    ct);

            if (memberA is null || memberB is null)
            {
                return Results.NotFound();
            }

            var entries = await db.LeaderboardEntries
                .Include(e => e.Leaderboard)
                .ThenInclude(l => l.Game)
                .Include(e => e.Member)
                .Where(e => e.MemberId == memberA.Id || e.MemberId == memberB.Id)
                .ToListAsync(ct);

            var boardIds = entries.Select(e => e.LeaderboardId).Distinct().ToList();
            var boards = await db.Leaderboards
                .Include(l => l.Game)
                .Include(l => l.Entries)
                .ThenInclude(e => e.Member)
                .Where(l => boardIds.Contains(l.Id))
                .OrderBy(l => l.Game.Title)
                .ThenBy(l => l.Title)
                .ToListAsync(ct);

            var memberALeads = 0;
            var memberBLeads = 0;
            var tiedBoards = 0;
            var games = new List<RivalryGameDto>();

            foreach (var boardGroup in boards.GroupBy(b => b.GameId))
            {
                var game = boardGroup.First().Game;
                var boardRows = new List<RivalryBoardDto>();

                foreach (var board in boardGroup)
                {
                    var entryA = board.Entries.FirstOrDefault(e => e.MemberId == memberA.Id);
                    var entryB = board.Entries.FirstOrDefault(e => e.MemberId == memberB.Id);

                    string? leaderUsername = null;
                    if (entryA?.FriendRank == 1 && entryB?.FriendRank == 1)
                    {
                        tiedBoards++;
                        leaderUsername = null;
                    }
                    else if (entryA?.FriendRank == 1)
                    {
                        memberALeads++;
                        leaderUsername = memberA.RaUsername;
                    }
                    else if (entryB?.FriendRank == 1)
                    {
                        memberBLeads++;
                        leaderUsername = memberB.RaUsername;
                    }
                    else if (entryA?.Score != null && entryB?.Score != null && entryA.Score == entryB.Score)
                    {
                        tiedBoards++;
                    }

                    boardRows.Add(new RivalryBoardDto(
                        board.RaLeaderboardId,
                        board.Title,
                        game.RaGameId,
                        game.Title,
                        entryA?.Score,
                        entryA?.FormattedScore,
                        entryA?.FriendRank,
                        entryA?.GlobalRank,
                        entryB?.Score,
                        entryB?.FormattedScore,
                        entryB?.FriendRank,
                        entryB?.GlobalRank,
                        board.GlobalEntryCount,
                        leaderUsername));
                }

                games.Add(new RivalryGameDto(game.RaGameId, game.Title, boardRows));
            }

            var summaryA = new MemberSummaryDto(
                memberA.Id,
                memberA.RaUsername ?? string.Empty,
                memberA.DisplayName ?? memberA.RaUsername ?? string.Empty,
                memberA.AvatarUrl,
                entries.Count(e => e.MemberId == memberA.Id),
                entries.Count(e => e.MemberId == memberA.Id && e.FriendRank == 1));

            var summaryB = new MemberSummaryDto(
                memberB.Id,
                memberB.RaUsername ?? string.Empty,
                memberB.DisplayName ?? memberB.RaUsername ?? string.Empty,
                memberB.AvatarUrl,
                entries.Count(e => e.MemberId == memberB.Id),
                entries.Count(e => e.MemberId == memberB.Id && e.FriendRank == 1));

            return Results.Ok(new RivalryResponse(
                summaryA,
                summaryB,
                memberALeads,
                memberBLeads,
                tiedBoards,
                games));
        })
        .WithName("GetRivalry")
        .WithTags("Rivalry")
        .WithSummary("Returns head-to-head standings between two tracked members across all games.")
        .RequireApiAuth();
}

public sealed record MemberSummaryDto(
    Guid Id,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    int BoardsWithScore,
    int FriendRankOnes);

public sealed record RivalryBoardDto(
    long RaLeaderboardId,
    string Title,
    int RaGameId,
    string GameTitle,
    long? MemberAScore,
    string? MemberAFormattedScore,
    int? MemberAFriendRank,
    int? MemberAGlobalRank,
    long? MemberBScore,
    string? MemberBFormattedScore,
    int? MemberBFriendRank,
    int? MemberBGlobalRank,
    int? GlobalEntryCount,
    string? LeaderUsername);

public sealed record RivalryGameDto(
    int RaGameId,
    string GameTitle,
    IReadOnlyList<RivalryBoardDto> Boards);

public sealed record RivalryResponse(
    MemberSummaryDto MemberA,
    MemberSummaryDto MemberB,
    int MemberALeads,
    int MemberBLeads,
    int TiedBoards,
    IReadOnlyList<RivalryGameDto> Games);
