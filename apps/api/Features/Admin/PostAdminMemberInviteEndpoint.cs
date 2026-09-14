using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PostAdminMemberInviteEndpoint
{
    public static RouteHandlerBuilder MapPostAdminMemberInvite(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/member-invites", async (
            PostAdminMemberInviteRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (!DiscordIdValidator.IsValid(request.DiscordId))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.DiscordId)] = ["discordId must be a numeric Discord user ID."]
                });
            }

            var discordId = DiscordIdValidator.Normalize(request.DiscordId);
            if (await db.Members.AnyAsync(m => m.DiscordId == discordId, ct))
            {
                return Results.Conflict(new { message = "A member with this Discord ID already exists." });
            }

            db.Members.Add(new Member { DiscordId = discordId });
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/admin/member-invites/{discordId}", new AdminMemberInviteDto(
                discordId,
                null,
                false,
                false,
                false));
        })
        .WithName("PostAdminMemberInvite")
        .WithTags("Admin")
        .WithSummary("Invites a Discord user by creating a member row they can use to sign in.")
        .RequireAdmin();
}

public sealed record PostAdminMemberInviteRequest(string DiscordId);
