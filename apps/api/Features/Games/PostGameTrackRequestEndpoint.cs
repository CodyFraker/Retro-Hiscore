using System.Security.Claims;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.GameOfTheWeek;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class PostGameTrackRequestEndpoint
{
    public static RouteHandlerBuilder MapPostGameTrackRequest(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/games/track-requests", async (
            GameOfTheWeekRaGameIdRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            IGameTrackQueueRequestService trackRequestService,
            CancellationToken ct) =>
        {
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var (_, error, _) = await trackRequestService.SubmitAsync(member.Id, request.RaGameId, ct);
            return error!;
        })
        .WithName("PostGameTrackRequest")
        .WithTags("Games")
        .WithSummary("Submit a RetroAchievements game to the track queue (up to 5 requests per member per rolling 24 hours).")
        .RequireApiAuth();
}
