using System.Security.Claims;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class PutGameOfTheWeekVoteEndpoint
{
    public static RouteHandlerBuilder MapPutGameOfTheWeekVote(this IEndpointRouteBuilder routes)
        => routes.MapPut("/api/game-of-the-week/current/vote", async (
            GameOfTheWeekRaGameIdRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            IGameOfTheWeekVoteService voteService,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var (dto, error) = await voteService.CastOrUpdateVoteAsync(request.RaGameId, member.Id, ct);
            return error ?? Results.Ok(dto);
        })
        .WithName("PutGameOfTheWeekVote")
        .WithTags("GameOfTheWeek")
        .WithSummary("Casts or updates the member vote for a game on the current open ballot.")
        .RequireApiAuth();
}
