using System.Security.Claims;

namespace RetroHiscore.Api.Infrastructure;

public static class DiscordUserExtensions
{
    public static string? GetDiscordUserId(ClaimsPrincipal user)
        => user.FindFirst("sub")?.Value
           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
