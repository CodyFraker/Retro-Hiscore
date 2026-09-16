using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin.DiscordWebhooks;

public static class DeleteAdminDiscordWebhookEndpoint
{
    public static RouteHandlerBuilder MapDeleteAdminDiscordWebhook(this IEndpointRouteBuilder routes)
        => routes.MapDelete("/api/admin/discord-webhooks/{id:guid}", async (
            Guid id,
            IDiscordWebhookStore store,
            CancellationToken ct) =>
        {
            await store.DeleteAsync(id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteAdminDiscordWebhook")
        .WithTags("Admin")
        .WithSummary("Deletes a Discord webhook configuration.")
        .RequireAdmin();
}
