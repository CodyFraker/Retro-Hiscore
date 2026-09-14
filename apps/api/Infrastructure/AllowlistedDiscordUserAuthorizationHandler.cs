using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;

namespace RetroHiscore.Api.Infrastructure;

public sealed class AllowlistedDiscordUserRequirement : IAuthorizationRequirement;

public sealed class AllowlistedDiscordUserAuthorizationHandler(
    IServiceScopeFactory scopeFactory) : AuthorizationHandler<AllowlistedDiscordUserRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AllowlistedDiscordUserRequirement requirement)
    {
        var discordId = DiscordUserExtensions.GetDiscordUserId(context.User);
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var exists = await db.Members.AnyAsync(m => m.DiscordId == discordId);
        if (exists)
        {
            context.Succeed(requirement);
        }
    }
}
