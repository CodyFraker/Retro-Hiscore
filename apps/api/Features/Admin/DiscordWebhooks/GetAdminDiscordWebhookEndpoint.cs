using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin.DiscordWebhooks;

public static class GetAdminDiscordWebhookEndpoint
{
    public static RouteHandlerBuilder MapGetAdminDiscordWebhook(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/discord-webhooks/{id:guid}", async (
            Guid id,
            IDiscordWebhookStore store,
            CancellationToken ct) =>
        {
            var item = await store.GetAsync(id, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("GetAdminDiscordWebhook")
        .WithTags("Admin")
        .WithSummary("Returns a Discord webhook configuration.")
        .RequireAdmin();
}
