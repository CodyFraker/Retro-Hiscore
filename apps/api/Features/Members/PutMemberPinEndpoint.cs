using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class PutMemberPinEndpoint
{
    public static RouteHandlerBuilder MapPutMemberPin(this IEndpointRouteBuilder routes)
        => routes.MapPut("/api/members/me/pins/{raGameId:int}", async (
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

            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
            if (game is null)
            {
                return Results.NotFound();
            }

            var existing = await db.MemberGamePins
                .FirstOrDefaultAsync(p => p.MemberId == member.Id && p.GameId == game.Id, ct);

            if (existing is not null)
            {
                return Results.NoContent();
            }

            var pinCount = await db.MemberGamePins.CountAsync(p => p.MemberId == member.Id, ct);
            if (pinCount >= MemberPins.MaxPinsPerMember)
            {
                return Results.Conflict(new { message = $"You can pin at most {MemberPins.MaxPinsPerMember} games." });
            }

            db.MemberGamePins.Add(new MemberGamePin
            {
                MemberId = member.Id,
                GameId = game.Id,
                PinnedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        })
        .WithName("PutMemberPin")
        .WithTags("Members")
        .WithSummary("Pins a tracked game for the current member.")
        .RequireApiAuth();
}
