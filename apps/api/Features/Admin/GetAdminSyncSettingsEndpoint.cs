using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class GetAdminSyncSettingsEndpoint
{
    public static RouteHandlerBuilder MapGetAdminSyncSettings(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/sync-settings", async (ISyncSettingsStore store, CancellationToken ct) =>
        {
            var settings = await store.GetAdminSettingsAsync(ct);
            return Results.Ok(settings);
        })
        .WithName("GetAdminSyncSettings")
        .WithTags("Admin")
        .WithSummary("Returns editable sync schedule settings stored in the database.")
        .RequireAdmin();
}
