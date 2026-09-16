using System.Security.Claims;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class PostGameOfTheWeekBallotEndpoint
{
    public static RouteHandlerBuilder MapPostGameOfTheWeekBallot(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/game-of-the-week/current/ballot", async (
            GameOfTheWeekRaGameIdRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            IGameOfTheWeekBallotService ballotService,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var (dto, error) = await ballotService.AddBallotGameAsync(request.RaGameId, member.Id, ct);
            return error ?? Results.Ok(dto);
        })
        .WithName("PostGameOfTheWeekBallot")
        .WithTags("GameOfTheWeek")
        .WithSummary("Adds a RetroAchievements game to the open poll ballot when slots remain.")
        .RequireApiAuth();
}
