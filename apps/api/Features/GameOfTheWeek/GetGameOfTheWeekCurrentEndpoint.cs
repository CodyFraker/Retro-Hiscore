using System.Security.Claims;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class GetGameOfTheWeekCurrentEndpoint
{
    public static RouteHandlerBuilder MapGetGameOfTheWeekCurrent(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/game-of-the-week/current", async (
            ClaimsPrincipal user,
            AppDbContext db,
            IGameOfTheWeekPollService pollService,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var dto = await pollService.GetCurrentPollDtoAsync(member.Id, ct);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        })
        .WithName("GetGameOfTheWeekCurrent")
        .WithTags("GameOfTheWeek")
        .WithSummary("Returns the current game-of-the-week poll, ballot, and vote tallies.")
        .RequireApiAuth();
}
