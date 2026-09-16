using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;
using System.Text.Json;

namespace RetroHiscore.Api.Features.Admin.DiscordWebhooks;

public static class PostAdminDiscordWebhookTestEndpoint
{
    private static readonly Dictionary<Guid, DateTimeOffset> LastTestSent = new();

    public static RouteHandlerBuilder MapPostAdminDiscordWebhookTest(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/discord-webhooks/{id:guid}/test", async (
            Guid id,
            TestDiscordWebhookRequest request,
            AppDbContext db,
            IDiscordWebhookTemplateRenderer renderer,
            IDiscordWebhookClient webhookClient,
            IWebhookUrlProtector urlProtector,
            CancellationToken ct) =>
        {
            if (LastTestSent.TryGetValue(id, out var last) && last.AddMinutes(1) > DateTimeOffset.UtcNow)
            {
                return Results.BadRequest(new { message = "Test webhooks are rate limited to once per minute." });
            }

            var webhook = await db.DiscordWebhookConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, ct);

            if (webhook is null)
            {
                return Results.NotFound();
            }

            if (!Enum.TryParse<Domain.DiscordNotificationEventKind>(request.EventKind, true, out var eventKind))
            {
                return Results.BadRequest(new { message = $"Unknown event kind '{request.EventKind}'." });
            }

            var payloadJson = DiscordWebhookSamplePayloads.BuildJson(eventKind);
            var rendered = renderer.Render(webhook.PayloadTemplateJson, eventKind, payloadJson);
            var url = urlProtector.Unprotect(webhook.WebhookUrlProtected);
            var success = await webhookClient.PostAsync(url, rendered, ct);
            if (!success)
            {
                return Results.BadRequest(new { message = "Discord rejected the test webhook." });
            }

            LastTestSent[id] = DateTimeOffset.UtcNow;
            return Results.NoContent();
        })
        .WithName("PostAdminDiscordWebhookTest")
        .WithTags("Admin")
        .WithSummary("Sends a test Discord webhook using sample event data.")
        .RequireAdmin();
}
