using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin.DiscordWebhooks;

public static class PutAdminDiscordWebhookEndpoint
{
    public static RouteHandlerBuilder MapPutAdminDiscordWebhook(this IEndpointRouteBuilder routes)
        => routes.MapPut("/api/admin/discord-webhooks/{id:guid}", async (
            Guid id,
            UpsertDiscordWebhookRequest request,
            IDiscordWebhookStore store,
            CancellationToken ct) =>
        {
            try
            {
                var updated = await store.UpdateAsync(id, request, ct);
                return Results.Ok(updated);
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
        .WithName("PutAdminDiscordWebhook")
        .WithTags("Admin")
        .WithSummary("Updates a Discord webhook configuration.")
        .RequireAdmin();
}
