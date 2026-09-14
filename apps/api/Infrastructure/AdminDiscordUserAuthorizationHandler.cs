using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Infrastructure;

public sealed class AdminDiscordUserRequirement : IAuthorizationRequirement;

public sealed class AdminDiscordUserAuthorizationHandler(
    IOptions<AuthOptions> authOptions) : AuthorizationHandler<AdminDiscordUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminDiscordUserRequirement requirement)
    {
        var discordId = context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return Task.CompletedTask;
        }

        var admins = authOptions.Value.AdminDiscordUserIds;
        if (admins.Any(id => string.Equals(id, discordId, StringComparison.Ordinal)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
