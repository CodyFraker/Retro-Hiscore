using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class PostMemberAlertReadEndpoint
{
    public static RouteHandlerBuilder MapPostMemberAlertRead(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/members/me/alerts/{id:guid}/read", async (
            Guid id,
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var member = await MemberRequestHelper.ResolveCurrentMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var alert = await db.MemberAlerts
                .FirstOrDefaultAsync(a => a.Id == id && a.RecipientMemberId == member.Id, ct);

            if (alert is null)
            {
                return Results.NotFound();
            }

            if (alert.ReadAt is null)
            {
                alert.ReadAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);
            }

            return Results.NoContent();
        })
        .WithName("PostMemberAlertRead")
        .WithTags("Members")
        .WithSummary("Marks a personal alert as read for the current member.")
        .RequireApiAuth();
}
