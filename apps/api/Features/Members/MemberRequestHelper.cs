using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class MemberRequestHelper
{
    public static async Task<Member?> ResolveCurrentMemberAsync(
        ClaimsPrincipal user,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var discordId = DiscordUserExtensions.GetDiscordUserId(user);
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return null;
        }

        return await db.Members.FirstOrDefaultAsync(m => m.DiscordId == discordId, cancellationToken);
    }
}
