using System.Security.Claims;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class GetAdminGameOfTheWeekCurrentPollEndpoint
{
    public static RouteHandlerBuilder MapGetAdminGameOfTheWeekCurrentPoll(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/game-of-the-week/polls/current", async (
            ClaimsPrincipal user,
            AppDbContext db,
            IGameOfTheWeekPollService pollService,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            var dto = await pollService.GetCurrentPollDtoAsync(member?.Id, ct);
            return dto is null ? Results.NotFound() : Results.Ok(dto);
        })
        .WithName("GetAdminGameOfTheWeekCurrentPoll")
        .WithTags("Admin")
        .WithSummary("Returns the current game-of-the-week poll for admin monitoring.")
        .RequireAdmin();
}
