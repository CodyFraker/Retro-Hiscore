using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;
using System.Text.Json;

namespace RetroHiscore.Api.Features.Admin.DiscordWebhooks;

public static class PostAdminDiscordWebhookPreviewEndpoint
{
    public static RouteHandlerBuilder MapPostAdminDiscordWebhookPreview(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/admin/discord-webhooks/{id:guid}/preview", async (
            Guid id,
            PreviewDiscordWebhookRequest request,
            AppDbContext db,
            IDiscordWebhookTemplateRenderer renderer,
            IDiscordWebhookPayloadValidator validator,
            CancellationToken ct) =>
        {
            var webhook = await db.DiscordWebhookConfigs
                .AsNoTracking()
                .Include(w => w.EventSubscriptions)
                .FirstOrDefaultAsync(w => w.Id == id, ct);

            if (webhook is null)
            {
                return Results.NotFound();
            }

            var template = string.IsNullOrWhiteSpace(request.PayloadTemplateJson)
                ? webhook.PayloadTemplateJson
                : request.PayloadTemplateJson;

            if (!Enum.TryParse<Domain.DiscordNotificationEventKind>(request.EventKind, true, out var eventKind))
            {
                return Results.BadRequest(new { message = $"Unknown event kind '{request.EventKind}'." });
            }

            var allowed = DiscordTokenCatalog.AllowedCodesForKinds([eventKind]);
            try
            {
                validator.ValidateTemplate(template, allowed);
            }
            catch (DiscordWebhookTemplateValidationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }

            var rendered = renderer.Render(
                template,
                eventKind,
                DiscordWebhookSamplePayloads.BuildJson(eventKind));
            return Results.Ok(new PreviewDiscordWebhookResponse(rendered));
        })
        .WithName("PostAdminDiscordWebhookPreview")
        .WithTags("Admin")
        .WithSummary("Renders a webhook embed template with sample event data.")
        .RequireAdmin();
}
