using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class DeleteAdminMemberInviteEndpoint
{
    public static RouteHandlerBuilder MapDeleteAdminMemberInvite(this IEndpointRouteBuilder routes)
        => routes.MapDelete("/api/admin/member-invites/{discordId}", async (
            string discordId,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (!DiscordIdValidator.IsValid(discordId))
            {
                return Results.NotFound();
            }

            var normalized = DiscordIdValidator.Normalize(discordId);
            var member = await db.Members.FirstOrDefaultAsync(m => m.DiscordId == normalized, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(member.RaUsername))
            {
                return Results.Conflict(new { message = "Cannot remove a member who has configured a RetroAchievements account." });
            }

            db.Members.Remove(member);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteAdminMemberInvite")
        .WithTags("Admin")
        .WithSummary("Removes a pending invite (members without a configured RA username only).")
        .RequireAdmin();
}
