using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin.DiscordWebhooks;

public static class PostAdminDiscordWebhookEndpoint
{
    public static RouteHandlerBuilder MapPostAdminDiscordWebhook(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/discord-webhooks", async (
            UpsertDiscordWebhookRequest request,
            IDiscordWebhookStore store,
            CancellationToken ct) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.WebhookUrl))
                {
                    return Results.BadRequest(new { message = "Webhook URL is required when creating a webhook." });
                }

                var created = await store.CreateAsync(request, ct);
                return Results.Created($"/api/admin/discord-webhooks/{created.Id}", created);
            }
            catch (DiscordWebhookValidationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (DiscordWebhookTemplateValidationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("PostAdminDiscordWebhook")
        .WithTags("Admin")
        .WithSummary("Creates a Discord webhook configuration.")
        .RequireAdmin();
}
