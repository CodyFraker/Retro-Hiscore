using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PostAdminGameTrackQueueApproveEndpoint
{
    public static RouteHandlerBuilder MapPostAdminGameTrackQueueApprove(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/game-track-queue/{id:guid}/approve", async (
            Guid id,
            ClaimsPrincipal user,
            AppDbContext db,
            IRaApiClient raApiClient,
            IRaApiKeyPool apiKeyPool,
            ILeaderboardSyncService leaderboardSync,
            IConsoleIconSyncService consoleIconSync,
            IOptions<RaOptions> raOptions,
            CancellationToken ct) =>
        {
            var resolvedBy = await ResolveMemberIdAsync(user, db, ct);
            var (item, error) = await AdminGameTrackQueueActions.ApproveAsync(
                id,
                resolvedBy,
                db,
                raApiClient,
                apiKeyPool,
                leaderboardSync,
                consoleIconSync,
                raOptions,
                ct);

            return error ?? Results.Ok(item);
        })
        .WithName("PostAdminGameTrackQueueApprove")
        .WithTags("Admin")
        .WithSummary("Approves a queued game, tracks it on the site, and runs scoped score sync for recent players.")
        .RequireAdmin();

    private static async Task<Guid?> ResolveMemberIdAsync(ClaimsPrincipal user, AppDbContext db, CancellationToken ct)
    {
        var discordId = DiscordUserExtensions.GetDiscordUserId(user);
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return null;
        }

        return await db.Members
            .Where(m => m.DiscordId == discordId)
            .Select(m => (Guid?)m.Id)
            .FirstOrDefaultAsync(ct);
    }
}
