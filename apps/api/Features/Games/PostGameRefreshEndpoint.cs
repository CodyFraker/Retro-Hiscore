using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class PostGameRefreshEndpoint
{
    public static RouteHandlerBuilder MapPostGameRefresh(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/games/{raGameId:int}/refresh", async (
            int raGameId,
            ClaimsPrincipal user,
            AppDbContext db,
            ILeaderboardSyncService leaderboardSync,
            ILeaderboardSyncJobEnqueuer jobEnqueuer,
            CancellationToken ct) =>
        {
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

            if (string.IsNullOrWhiteSpace(member.RaApiKey))
            {
                return Results.Json(
                    new { message = "Link a RetroAchievements API key in Settings before refreshing scores." },
                    statusCode: StatusCodes.Status403Forbidden);
            }

            var game = await db.Games.FirstOrDefaultAsync(g => g.RaGameId == raGameId, ct);
            if (game is null)
            {
                return Results.NotFound(new { message = $"Game {raGameId} is not tracked." });
            }

            if (leaderboardSync.IsPerGameRefreshCooldownActive(game.Id, member.Id, out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Per-game refresh cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var run = new SyncRun
            {
                Kind = SyncKind.LeaderboardScores,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Running,
                StartedAt = DateTimeOffset.UtcNow,
                GameId = game.Id,
                MemberId = member.Id
            };
            db.SyncRuns.Add(run);
            await db.SaveChangesAsync(ct);

            jobEnqueuer.EnqueueMemberGameSync(raGameId, member.Id, SyncTrigger.Manual, run.Id);

            return Results.Accepted($"/api/sync/status", new SyncAcceptedResponse(run.Id.ToString()));
        })
        .WithName("PostGameRefresh")
        .WithTags("Games")
        .WithSummary("Queues a leaderboard score refresh for the current member on one tracked game using their API key.")
        .RequireApiAuth();
}
