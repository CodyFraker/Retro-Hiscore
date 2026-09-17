using System.Security.Claims;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class PostAdminGameOfTheWeekClosePollEndpoint
{
    public static RouteHandlerBuilder MapPostAdminGameOfTheWeekClosePoll(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/game-of-the-week/polls/current/close", async (
            ClaimsPrincipal user,
            AppDbContext db,
            IGameOfTheWeekCloseService closeService,
            CancellationToken ct) =>
        {
            var (poll, error) = await closeService.CloseOpenBlockingPollAsync(ct);
            if (error is not null)
            {
                return error;
            }

            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            var now = DateTimeOffset.UtcNow;
            var dto = await GameOfTheWeekPollMapper.MapCurrentAsync(db, poll!, now, member?.Id, ct);
            return Results.Ok(dto);
        })
        .WithName("PostAdminGameOfTheWeekClosePoll")
        .WithTags("Admin")
        .WithSummary("Closes the current open game-of-the-week poll and declares the winner.")
        .RequireAdmin();
}
