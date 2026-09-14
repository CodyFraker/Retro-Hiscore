using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetCurrentMemberEndpoint
{
    public static RouteHandlerBuilder MapGetCurrentMember(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/me", async (ClaimsPrincipal user, AppDbContext db, CancellationToken ct) =>
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

            var boardsWithScore = await db.LeaderboardEntries.CountAsync(e => e.MemberId == member.Id, ct);
            var friendRankOnes = await db.LeaderboardEntries.CountAsync(
                e => e.MemberId == member.Id && e.FriendRank == 1,
                ct);

            return Results.Ok(new MemberDto(
                member.Id,
                member.RaUsername,
                member.RaUlid,
                member.DisplayName ?? member.RaUsername,
                member.AvatarUrl,
                boardsWithScore,
                friendRankOnes,
                HasApiKey: !string.IsNullOrWhiteSpace(member.RaApiKey)));
        })
        .WithName("GetCurrentMember")
        .WithTags("Members")
        .WithSummary("Returns the tracked member linked to the authenticated Discord user.")
        .RequireApiAuth();
}
