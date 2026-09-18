using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class PutMemberUiThemeEndpoint
{
    public static RouteHandlerBuilder MapPutMemberUiTheme(this IEndpointRouteBuilder routes)
        => routes.MapPut("/api/members/me/ui-theme", async (
            PutMemberUiThemeRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (!UiThemes.IsValid(request.UiTheme))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [nameof(request.UiTheme)] = ["uiTheme must be a supported site theme."]
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

            member.UiTheme = request.UiTheme.Trim();
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("PutMemberUiTheme")
        .WithTags("Members")
        .WithSummary("Updates the site UI theme preference for the authenticated user's linked member.")
        .RequireApiAuth();
}

public sealed record PutMemberUiThemeRequest(string UiTheme);
