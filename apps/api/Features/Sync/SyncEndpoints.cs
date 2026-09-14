using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Sync;

public static class TriggerSyncEndpoint
{
    public static RouteHandlerBuilder MapTriggerSync(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/sync", async (
            ILeaderboardSyncService syncService,
            ILeaderboardSyncDispatcher dispatcher,
            CancellationToken ct) =>
        {
            if (syncService.IsManualCooldownActive(out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Manual sync cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var enqueued = await dispatcher.DispatchDueGamesAsync(SyncTrigger.Manual, forceAll: true, ct);
            return Results.Accepted(
                "/api/sync/status",
                new SyncAcceptedResponse($"enqueued-{enqueued}"));
        })
        .WithName("TriggerSync")
        .WithTags("Sync")
        .WithSummary("Admin: enqueue leaderboard sync jobs for all tracked games.")
        .RequireAdmin();
}

public static class GetSyncStatusEndpoint
{
    public static RouteHandlerBuilder MapGetSyncStatus(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/sync/status", async (AppDbContext db, CancellationToken ct) =>
        {
            var last = await db.SyncRuns
                .Where(r => r.Kind == SyncKind.LeaderboardScores)
                .OrderByDescending(r => r.StartedAt)
                .FirstOrDefaultAsync(ct);

            if (last is null)
            {
                return Results.Ok(new SyncStatusDto(null, null, null, null, null, null));
            }

            return Results.Ok(new SyncStatusDto(
                last.Id,
                last.Trigger.ToString(),
                last.Status.ToString(),
                last.StartedAt,
                last.FinishedAt,
                last.Error));
        })
        .WithName("GetSyncStatus")
        .WithTags("Sync")
        .RequireApiAuth();
}

public static class TriggerMetadataSyncEndpoint
{
    public static RouteHandlerBuilder MapTriggerMetadataSync(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/sync/metadata", async (IGameMetadataSyncService syncService, CancellationToken ct) =>
        {
            if (syncService.IsManualCooldownActive(out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Manual metadata sync cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var run = await syncService.SyncAsync(SyncTrigger.Manual, ct);
            return Results.Accepted("/api/sync/metadata/status", new SyncAcceptedResponse(run.Id.ToString()));
        })
        .WithName("TriggerMetadataSync")
        .WithTags("Sync")
        .RequireApiAuth();
}

public static class GetMetadataSyncStatusEndpoint
{
    public static RouteHandlerBuilder MapGetMetadataSyncStatus(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/sync/metadata/status", async (AppDbContext db, CancellationToken ct) =>
        {
            var last = await db.SyncRuns
                .Where(r => r.Kind == SyncKind.GameMetadata)
                .OrderByDescending(r => r.StartedAt)
                .FirstOrDefaultAsync(ct);

            if (last is null)
            {
                return Results.Ok(new SyncStatusDto(null, null, null, null, null, null));
            }

            return Results.Ok(new SyncStatusDto(
                last.Id,
                last.Trigger.ToString(),
                last.Status.ToString(),
                last.StartedAt,
                last.FinishedAt,
                last.Error));
        })
        .WithName("GetMetadataSyncStatus")
        .WithTags("Sync")
        .RequireApiAuth();
}

public static class TriggerConsoleIconSyncEndpoint
{
    public static RouteHandlerBuilder MapTriggerConsoleIconSync(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/sync/console-icons", async (
            bool? force,
            IConsoleIconSyncService syncService,
            CancellationToken ct) =>
        {
            if (syncService.IsManualCooldownActive(out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Manual console icon sync cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var run = await syncService.SyncAsync(SyncTrigger.Manual, force ?? false, ct);
            return Results.Accepted("/api/sync/console-icons/status", new SyncAcceptedResponse(run.Id.ToString()));
        })
        .WithName("TriggerConsoleIconSync")
        .WithTags("Sync")
        .WithSummary("Sync system icons for consoles used by tracked games. Pass force=true to re-download all needed icons.")
        .RequireApiAuth();
}

public static class GetConsoleIconSyncStatusEndpoint
{
    public static RouteHandlerBuilder MapGetConsoleIconSyncStatus(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/sync/console-icons/status", async (AppDbContext db, CancellationToken ct) =>
        {
            var last = await db.SyncRuns
                .Where(r => r.Kind == SyncKind.ConsoleIcons)
                .OrderByDescending(r => r.StartedAt)
                .FirstOrDefaultAsync(ct);

            if (last is null)
            {
                return Results.Ok(new SyncStatusDto(null, null, null, null, null, null));
            }

            return Results.Ok(new SyncStatusDto(
                last.Id,
                last.Trigger.ToString(),
                last.Status.ToString(),
                last.StartedAt,
                last.FinishedAt,
                last.Error));
        })
        .WithName("GetConsoleIconSyncStatus")
        .WithTags("Sync")
        .WithSummary("Returns the most recent console icon sync run.")
        .RequireApiAuth();
}

public sealed record SyncAcceptedResponse(string JobId);
public sealed record SyncCooldownResponse(string Message, DateTimeOffset? AvailableAt);
public sealed record SyncStatusDto(
    Guid? Id,
    string? Trigger,
    string? Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    string? Error);
