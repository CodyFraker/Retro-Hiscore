using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class PutMemberRaAccountEndpoint
{
    public static RouteHandlerBuilder MapPutMemberRaAccount(this IEndpointRouteBuilder routes)
        => routes.MapPut("/api/members/me/ra-account", async (
            PutMemberRaAccountRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            IRaApiClient raApiClient,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.RaUsername))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.RaUsername)] = ["raUsername is required."]
                });
            }

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

            var member = await db.Members.FirstOrDefaultAsync(m => m.DiscordId == discordId, ct);
            if (member is null)
            {
                return Results.NotFound();
            }

            var raUsername = request.RaUsername.Trim();
            var raApiKey = request.RaApiKey.Trim();

            var taken = await db.Members.AnyAsync(
                m => m.Id != member.Id
                    && m.RaUsername != null
                    && EF.Functions.ILike(m.RaUsername, raUsername),
                ct);
            if (taken)
            {
                return Results.Conflict(new { message = "That RetroAchievements username is already linked to another member." });
            }

            var summary = await raApiClient.GetUserSummaryAsync(
                raUsername,
                raApiKey,
                recentGamesCount: 0,
                recentAchievementsCount: 0,
                cancellationToken: ct);
            if (summary is null || string.IsNullOrWhiteSpace(summary.User))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.RaApiKey)] = ["Could not verify this API key for the given RetroAchievements username."]
                });
            }

            if (!string.Equals(summary.User, raUsername, StringComparison.OrdinalIgnoreCase))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.RaUsername)] = ["API key does not belong to this RetroAchievements username."]
                });
            }

            member.RaUsername = summary.User;
            member.RaApiKey = raApiKey;
            if (!string.IsNullOrWhiteSpace(summary.Ulid))
            {
                member.RaUlid = summary.Ulid;
            }

            member.DisplayName ??= summary.User;
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("PutMemberRaAccount")
        .WithTags("Members")
        .WithSummary("Links the authenticated member to a RetroAchievements account using username and API key.")
        .RequireApiAuth();
}

public sealed record PutMemberRaAccountRequest(string RaUsername, string RaApiKey);
