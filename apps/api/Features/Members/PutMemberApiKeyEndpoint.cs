using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class PutMemberApiKeyEndpoint
{
    public static RouteHandlerBuilder MapPutMemberApiKey(this IEndpointRouteBuilder routes)
        => routes.MapPut("/api/members/me/api-key", async (
            PutMemberApiKeyRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.RaApiKey))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.RaApiKey)] = ["raApiKey is required."]
                });
            }

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

            member.RaApiKey = request.RaApiKey.Trim();
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("PutMemberApiKey")
        .WithTags("Members")
        .WithSummary("Stores the RetroAchievements API key for the authenticated user's linked member.")
        .RequireApiAuth();
}

public sealed record PutMemberApiKeyRequest(string RaApiKey);
