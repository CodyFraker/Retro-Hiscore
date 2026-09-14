using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Members;

public static class GetCurrentMemberEndpoint
{
    public static RouteHandlerBuilder MapGetCurrentMember(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/me", async (
            ClaimsPrincipal user,
            AppDbContext db,
            IOptions<AuthOptions> authOptions,
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

            var boardsWithScore = await db.LeaderboardEntries.CountAsync(e => e.MemberId == member.Id, ct);
            var friendRankOnes = await db.LeaderboardEntries.CountAsync(
                e => e.MemberId == member.Id && e.FriendRank == 1,
                ct);

            return Results.Ok(new CurrentMemberDto(
                member.Id,
                member.RaUsername,
                member.RaUlid,
                MemberAuthHelper.DisplayLabel(member),
                member.AvatarUrl,
                boardsWithScore,
                friendRankOnes,
                HasApiKey: !string.IsNullOrWhiteSpace(member.RaApiKey),
                NeedsOnboarding: MemberAuthHelper.NeedsOnboarding(member),
                IsAdmin: MemberAuthHelper.IsAdmin(member, discordId, authOptions.Value)));
        })
        .WithName("GetCurrentMember")
        .WithTags("Members")
        .WithSummary("Returns the tracked member linked to the authenticated Discord user.")
        .RequireApiAuth();
}

public sealed record CurrentMemberDto(
    Guid Id,
    string? RaUsername,
    string? RaUlid,
    string DisplayName,
    string? AvatarUrl,
    int BoardsWithScore,
    int FriendRankOnes,
    bool HasApiKey,
    bool NeedsOnboarding,
    bool IsAdmin);
