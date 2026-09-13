using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Infrastructure;

public sealed class AllowlistedDiscordUserRequirement : IAuthorizationRequirement;

public sealed class AllowlistedDiscordUserAuthorizationHandler(
    IOptions<AuthOptions> authOptions) : AuthorizationHandler<AllowlistedDiscordUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllowlistedDiscordUserRequirement requirement)
    {
        var discordId = context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return Task.CompletedTask;
        }

        var allowed = authOptions.Value.AllowedDiscordUserIds;
        if (allowed.Any(id => string.Equals(id, discordId, StringComparison.Ordinal)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
