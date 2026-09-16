using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin.DiscordWebhooks;

public static class GetAdminDiscordWebhooksEndpoint
{
    public static RouteHandlerBuilder MapGetAdminDiscordWebhooks(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/discord-webhooks", async (IDiscordWebhookStore store, CancellationToken ct) =>
            Results.Ok(await store.ListAsync(ct)))
        .WithName("GetAdminDiscordWebhooks")
        .WithTags("Admin")
        .WithSummary("Lists configured Discord webhooks.")
        .RequireAdmin();
}
