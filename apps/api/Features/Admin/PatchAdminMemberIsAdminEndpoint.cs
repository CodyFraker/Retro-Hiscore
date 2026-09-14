using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Admin;

public static class PatchAdminMemberIsAdminEndpoint
{
    public static RouteHandlerBuilder MapPatchAdminMemberIsAdmin(this IEndpointRouteBuilder routes)
        => routes.MapPatch("/api/admin/member-invites/{discordId}/is-admin", async (
            string discordId,
            PatchAdminMemberIsAdminRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            IOptions<AuthOptions> authOptions,
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

            var callerDiscordId = DiscordUserExtensions.GetDiscordUserId(user);
            if (string.IsNullOrWhiteSpace(callerDiscordId))
            {
                return Results.Unauthorized();
            }

            if (MemberAuthHelper.IsEnvAdmin(normalized, authOptions.Value)
                && !request.IsAdmin)
            {
                return Results.Conflict(new { message = "Cannot remove admin status from bootstrap env administrators." });
            }

            member.IsAdmin = request.IsAdmin;
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("PatchAdminMemberIsAdmin")
        .WithTags("Admin")
        .WithSummary("Sets whether a member has database admin privileges.")
        .RequireAdmin();
}

public sealed record PatchAdminMemberIsAdminRequest(bool IsAdmin);
