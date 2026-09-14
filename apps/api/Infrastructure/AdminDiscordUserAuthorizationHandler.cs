using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Infrastructure;

public sealed class AdminDiscordUserRequirement : IAuthorizationRequirement;

public sealed class AdminDiscordUserAuthorizationHandler(
    IServiceScopeFactory scopeFactory,
    IOptions<AuthOptions> authOptions) : AuthorizationHandler<AdminDiscordUserRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminDiscordUserRequirement requirement)
    {
        var discordId = DiscordUserExtensions.GetDiscordUserId(context.User);
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return;
        }

        if (MemberAuthHelper.IsEnvAdmin(discordId, authOptions.Value))
        {
            context.Succeed(requirement);
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var member = await db.Members.FirstOrDefaultAsync(m => m.DiscordId == discordId);
        if (member is { IsAdmin: true })
        {
            context.Succeed(requirement);
        }
    }
}
