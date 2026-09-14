using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PostAdminGameTrackQueueRejectEndpoint
{
    public static RouteHandlerBuilder MapPostAdminGameTrackQueueReject(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/game-track-queue/{id:guid}/reject", async (
            Guid id,
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var discordId = DiscordUserExtensions.GetDiscordUserId(user);
            Guid? resolvedBy = null;
            if (!string.IsNullOrWhiteSpace(discordId))
            {
                resolvedBy = await db.Members
                    .Where(m => m.DiscordId == discordId)
                    .Select(m => (Guid?)m.Id)
                    .FirstOrDefaultAsync(ct);
            }

            var (item, error) = await AdminGameTrackQueueActions.RejectAsync(id, resolvedBy, db, ct);
            return error ?? Results.Ok(item);
        })
        .WithName("PostAdminGameTrackQueueReject")
        .WithTags("Admin")
        .WithSummary("Rejects a queued game so it will not be suggested again until rejection is cleared.")
        .RequireAdmin();
}
