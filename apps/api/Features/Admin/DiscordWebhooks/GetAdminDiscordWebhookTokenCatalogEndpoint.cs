using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin.DiscordWebhooks;

public static class GetAdminDiscordWebhookTokenCatalogEndpoint
{
    public static RouteHandlerBuilder MapGetAdminDiscordWebhookTokenCatalog(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/discord-webhooks/token-catalog", () =>
        {
            var events = Enum.GetValues<DiscordNotificationEventKind>()
                .Select(kind => new DiscordTokenCatalogEntryDto(
                    kind.ToString(),
                    DiscordTokenCatalog.ForKind(kind)
                        .Select(t => new DiscordTokenCatalogTokenDto(t.Code, t.Label))
                        .ToList()))
                .ToList();

            return Results.Ok(new DiscordTokenCatalogDto(events));
        })
        .WithName("GetAdminDiscordWebhookTokenCatalog")
        .WithTags("Admin")
        .WithSummary("Lists injectable tokens per Discord notification event kind.")
        .RequireAdmin();
}
