using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public static class PostAdminGameOfTheWeekPollEndpoint
{
    public static RouteHandlerBuilder MapPostAdminGameOfTheWeekPoll(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/game-of-the-week/polls", async (
            PostGameOfTheWeekPollRequest request,
            ClaimsPrincipal user,
            AppDbContext db,
            IGameOfTheWeekPollService pollService,
            CancellationToken ct) =>
        {
            var createdBy = await ResolveMemberIdAsync(user, db, ct);
            var (dto, error) = await pollService.CreatePollAsync(request, createdBy, ct);
            return error ?? Results.Created("/api/admin/game-of-the-week/polls/current", dto);
        })
        .WithName("PostAdminGameOfTheWeekPoll")
        .WithTags("Admin")
        .WithSummary("Starts a new game-of-the-week poll with at least two seeded ballot games.")
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
