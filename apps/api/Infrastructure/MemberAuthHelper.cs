using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Infrastructure;

public static class MemberAuthHelper
{
    public static async Task<Member?> FindByDiscordIdAsync(
        AppDbContext db,
        string discordId,
        CancellationToken cancellationToken = default)
        => await db.Members.FirstOrDefaultAsync(m => m.DiscordId == discordId, cancellationToken);

    public static bool IsEnvAdmin(string discordId, AuthOptions authOptions)
        => authOptions.AdminDiscordUserIds.Any(id => string.Equals(id, discordId, StringComparison.Ordinal));

    public static bool IsAdmin(Member member, string discordId, AuthOptions authOptions)
        => member.IsAdmin || IsEnvAdmin(discordId, authOptions);

    public static bool NeedsOnboarding(Member member)
        => string.IsNullOrWhiteSpace(member.RaUsername) || string.IsNullOrWhiteSpace(member.RaApiKey);

    public static string DisplayLabel(Member member)
        => member.DisplayName ?? member.RaUsername ?? "Pending";
}
