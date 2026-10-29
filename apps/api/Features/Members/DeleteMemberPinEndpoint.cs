using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class DeleteMemberPinEndpoint
{
    public static RouteHandlerBuilder MapDeleteMemberPin(this IEndpointRouteBuilder routes)
        => routes.MapDelete("/api/members/me/pins/{raGameId:int}", async (
            int raGameId,
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var member = await MemberRequestHelper.ResolveCurrentMemberAsync(user, db, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var game = await db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
            if (game is null)
            {
                return Results.NotFound();
            }

            var pin = await db.MemberGamePins
                .FirstOrDefaultAsync(p => p.MemberId == member.Id && p.GameId == game.Id, ct);

            if (pin is null)
            {
                return Results.NoContent();
            }

            db.MemberGamePins.Remove(pin);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        })
        .WithName("DeleteMemberPin")
        .WithTags("Members")
        .WithSummary("Removes a pinned game for the current member.")
        .RequireApiAuth();
}
