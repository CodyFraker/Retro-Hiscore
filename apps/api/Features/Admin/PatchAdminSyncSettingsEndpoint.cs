using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class PatchAdminSyncSettingsEndpoint
{
    public static RouteHandlerBuilder MapPatchAdminSyncSettings(this IEndpointRouteBuilder routes)
        => routes.MapPatch("/api/admin/sync-settings", async (
            PatchAdminSyncSettingsRequest request,
            ISyncSettingsStore store,
            IRecurringSyncJobRegistrar registrar,
            IHostEnvironment environment,
            CancellationToken ct) =>
        {
            try
            {
                var settings = await store.UpdateAsync(request, ct);
                if (!environment.IsEnvironment("Testing"))
                {
                    await registrar.RegisterAllAsync(ct);
                }

                return Results.Ok(settings);
            }
            catch (SyncSettingsValidationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("PatchAdminSyncSettings")
        .WithTags("Admin")
        .WithSummary("Updates sync schedule settings and reapplies Hangfire recurring jobs.")
        .RequireAdmin();
}
