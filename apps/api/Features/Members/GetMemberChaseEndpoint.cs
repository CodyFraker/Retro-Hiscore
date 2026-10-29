using System.Security.Claims;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMemberChaseEndpoint
{
    public static RouteHandlerBuilder MapGetMemberChase(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/me/chase", async (
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var member = await MemberRequestHelper.ResolveCurrentMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var items = await MemberChaseQuery.LoadAsync(db, member.Id, ct);
            return Results.Ok(new MemberChaseResponse(items));
        })
        .WithName("GetMemberChase")
        .WithTags("Members")
        .WithSummary("Returns leaderboards where the current member holds friend rank 2 and the gap to rank 1.")
        .RequireApiAuth();
}
