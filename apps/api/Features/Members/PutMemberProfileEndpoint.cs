using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class PutMemberProfileEndpoint
{
    public static RouteHandlerBuilder MapPutMemberProfile(this IEndpointRouteBuilder routes)
        => routes.MapPut("/api/members/me/profile", async (
            PutMemberProfileRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var discordId = DiscordUserExtensions.GetDiscordUserId(user);
            if (string.IsNullOrWhiteSpace(discordId))
            {
                return Results.Unauthorized();
            }

            var member = await db.Members
                .FirstOrDefaultAsync(m => m.DiscordId == discordId, ct);

            if (member is null)
            {
                return Results.NotFound();
            }

            member.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl)
                ? null
                : request.AvatarUrl.Trim();
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("PutMemberProfile")
        .WithTags("Members")
        .WithSummary("Updates the Discord avatar URL for the authenticated user's linked member.")
        .RequireApiAuth();
}

public sealed record PutMemberProfileRequest(string? AvatarUrl);
