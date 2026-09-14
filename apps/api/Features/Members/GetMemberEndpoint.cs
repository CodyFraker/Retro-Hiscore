using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberEndpoint
{
    public static RouteHandlerBuilder MapGetMember(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/{raUsername}", async (string raUsername, AppDbContext db, CancellationToken ct) =>
        {
            var member = await db.Members
                .FirstOrDefaultAsync(m => EF.Functions.ILike(m.RaUsername, raUsername), ct);

            if (member is null)
            {
                return Results.NotFound();
            }

            var entries = await db.LeaderboardEntries
                .Include(e => e.Leaderboard)
                .ThenInclude(l => l.Game)
                .Where(e => e.MemberId == member.Id)
                .OrderBy(e => e.Leaderboard.Game.Title)
                .ThenBy(e => e.Leaderboard.Title)
                .ToListAsync(ct);

            var standings = entries.Select(e => new MemberStandingDto(
                e.Leaderboard.Game.RaGameId,
                e.Leaderboard.Game.Title,
                e.Leaderboard.RaLeaderboardId,
                e.Leaderboard.Title,
                e.Leaderboard.Format,
                e.FriendRank,
                e.Score,
                e.FormattedScore)).ToList();

            var boardsWithScore = standings.Count;
            var friendRankOnes = standings.Count(s => s.FriendRank == 1);

            return Results.Ok(new MemberDetailDto(
                member.Id,
                member.RaUsername,
                member.RaUlid,
                member.DisplayName ?? member.RaUsername,
                member.AvatarUrl,
                boardsWithScore,
                friendRankOnes,
                standings));
        })
        .WithName("GetMember")
        .WithTags("Members")
        .RequireApiAuth();
}

public sealed record MemberStandingDto(
    int RaGameId,
    string GameTitle,
    long RaLeaderboardId,
    string LeaderboardTitle,
    string? Format,
    int? FriendRank,
    long Score,
    string FormattedScore);

public sealed record MemberDetailDto(
    Guid Id,
    string RaUsername,
    string? RaUlid,
    string DisplayName,
    string? AvatarUrl,
    int BoardsWithScore,
    int FriendRankOnes,
    IReadOnlyList<MemberStandingDto> Standings);
