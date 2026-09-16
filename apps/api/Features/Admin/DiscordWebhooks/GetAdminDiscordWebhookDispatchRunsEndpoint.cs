using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin.DiscordWebhooks;

public static class GetAdminDiscordWebhookDispatchRunsEndpoint
{
    public static RouteHandlerBuilder MapGetAdminDiscordWebhookDispatchRuns(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/discord-webhooks/dispatch-runs", async (
            int? limit,
            IDiscordWebhookStore store,
            CancellationToken ct) =>
            Results.Ok(await store.ListDispatchRunsAsync(limit ?? 15, ct)))
        .WithName("GetAdminDiscordWebhookDispatchRuns")
        .WithTags("Admin")
        .WithSummary("Lists recent Discord notification dispatch runs.")
        .RequireAdmin();
}
