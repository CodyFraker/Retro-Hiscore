using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class PostMemberAlertsReadAllEndpoint
{
    public static RouteHandlerBuilder MapPostMemberAlertsReadAll(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/members/me/alerts/read-all", async (
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var member = await MemberRequestHelper.ResolveCurrentMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var now = DateTimeOffset.UtcNow;
            await db.MemberAlerts
                .Where(a => a.RecipientMemberId == member.Id && a.ReadAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.ReadAt, now), ct);

            return Results.NoContent();
        })
        .WithName("PostMemberAlertsReadAll")
        .WithTags("Members")
        .WithSummary("Marks all personal alerts as read for the current member.")
        .RequireApiAuth();
}
