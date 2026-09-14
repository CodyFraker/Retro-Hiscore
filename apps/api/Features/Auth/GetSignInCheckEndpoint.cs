using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Auth;

public static class GetSignInCheckEndpoint
{
    public static RouteHandlerBuilder MapGetSignInCheck(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/auth/sign-in-check", async (
            string discordId,
            HttpRequest request,
            AppDbContext db,
            IOptions<AuthOptions> authOptions,
            CancellationToken ct) =>
        {
            if (!AuthExtensions.IsSignInServiceAuthorized(request, authOptions.Value))
            {
                return Results.Unauthorized();
            }

            if (!DiscordIdValidator.IsValid(discordId))
            {
                return Results.NotFound();
            }

            var normalized = DiscordIdValidator.Normalize(discordId);
            var exists = await db.Members.AnyAsync(m => m.DiscordId == normalized, ct);
            return exists ? Results.NoContent() : Results.NotFound();
        })
        .WithName("GetSignInCheck")
        .WithTags("Auth")
        .WithSummary("Checks whether a Discord user ID is invited to sign in (server-to-server only).");
}
