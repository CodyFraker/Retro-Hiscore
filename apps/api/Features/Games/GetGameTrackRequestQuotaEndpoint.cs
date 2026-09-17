using System.Security.Claims;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class GetGameTrackRequestQuotaEndpoint
{
    public static RouteHandlerBuilder MapGetGameTrackRequestQuota(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/games/track-requests/quota", async (
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

            var quota = await trackRequestService.GetQuotaAsync(member.Id, ct);
            return Results.Ok(quota);
        })
        .WithName("GetGameTrackRequestQuota")
        .WithTags("Games")
        .WithSummary("Returns the member track-request quota for the rolling 24-hour window.")
        .RequireApiAuth();
}
