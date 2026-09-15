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

public static class TriggerDueDispatchSyncEndpoint
{
    public static RouteHandlerBuilder MapTriggerDueDispatchSync(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/sync/dispatch", async (
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

            var enqueued = await dispatcher.DispatchDueGamesAsync(SyncTrigger.Manual, forceAll: false, ct);
            return Results.Accepted(
                "/api/sync/status",
                new SyncAcceptedResponse($"enqueued-{enqueued}"));
        })
        .WithName("TriggerDueDispatchSync")
        .WithTags("Sync")
        .WithSummary("Admin: enqueue leaderboard sync jobs only for games that are due.")
        .RequireAdmin();
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
        .WithSummary("Admin: refresh game metadata and art for all tracked games.")
        .RequireAdmin();
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
        .WithSummary("Returns the most recent game metadata sync run.")
        .RequireAdmin();
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
        .WithSummary("Admin: sync system icons for consoles used by tracked games. Pass force=true to re-download all needed icons.")
        .RequireAdmin();
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
        .RequireAdmin();
}

public static class TriggerMemberActivitySyncEndpoint
{
    public static RouteHandlerBuilder MapTriggerMemberActivitySync(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/sync/member-activity", async (
            IMemberActivitySyncService syncService,
            CancellationToken ct) =>
        {
            if (syncService.IsManualCooldownActive(out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Manual member activity sync cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var run = await syncService.SyncAsync(SyncTrigger.Manual, ct);
            return Results.Accepted("/api/sync/member-activity/status", new SyncAcceptedResponse(run.Id.ToString()));
        })
        .WithName("TriggerMemberActivitySync")
        .WithTags("Sync")
        .WithSummary("Admin: sync recently played games for all members and refresh the game track queue.")
        .RequireAdmin();
}

public static class GetMemberActivitySyncStatusEndpoint
{
    public static RouteHandlerBuilder MapGetMemberActivitySyncStatus(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/sync/member-activity/status", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await SyncStatusQueries.GetLatestAsync(db, SyncKind.MemberActivity, ct)))
        .WithName("GetMemberActivitySyncStatus")
        .WithTags("Sync")
        .WithSummary("Returns the most recent member activity sync run.")
        .RequireAdmin();
}

public static class TriggerMemberRankSyncEndpoint
{
    public static RouteHandlerBuilder MapTriggerMemberRankSync(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/sync/member-rank", async (
            IMemberRankSyncService syncService,
            CancellationToken ct) =>
        {
            if (syncService.IsManualCooldownActive(out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Manual member rank sync cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var run = await syncService.SyncAsync(SyncTrigger.Manual, ct);
            return Results.Accepted("/api/sync/member-rank/status", new SyncAcceptedResponse(run.Id.ToString()));
        })
        .WithName("TriggerMemberRankSync")
        .WithTags("Sync")
        .WithSummary("Admin: sync RetroAchievements site rank snapshots for all members.")
        .RequireAdmin();
}

public static class GetMemberRankSyncStatusEndpoint
{
    public static RouteHandlerBuilder MapGetMemberRankSyncStatus(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/sync/member-rank/status", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await SyncStatusQueries.GetLatestAsync(db, SyncKind.MemberRank, ct)))
        .WithName("GetMemberRankSyncStatus")
        .WithTags("Sync")
        .WithSummary("Returns the most recent member rank sync run.")
        .RequireAdmin();
}

public static class TriggerMemberAchievementSyncEndpoint
{
    public static RouteHandlerBuilder MapTriggerMemberAchievementSync(this IEndpointRouteBuilder routes)
        => routes.MapPost("/api/sync/member-achievements", async (
            IMemberAchievementSyncService syncService,
            CancellationToken ct) =>
        {
            if (syncService.IsManualCooldownActive(out var availableAt))
            {
                return Results.Json(
                    new SyncCooldownResponse("Manual member achievement sync cooldown is active", availableAt),
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var run = await syncService.SyncAsync(SyncTrigger.Manual, ct);
            return Results.Accepted("/api/sync/member-achievements/status", new SyncAcceptedResponse(run.Id.ToString()));
        })
        .WithName("TriggerMemberAchievementSync")
        .WithTags("Sync")
        .WithSummary("Admin: sync achievement progress for all members on all tracked games.")
        .RequireAdmin();
}

public static class GetMemberAchievementSyncStatusEndpoint
{
    public static RouteHandlerBuilder MapGetMemberAchievementSyncStatus(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/sync/member-achievements/status", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await SyncStatusQueries.GetLatestAsync(db, SyncKind.MemberAchievements, ct)))
        .WithName("GetMemberAchievementSyncStatus")
        .WithTags("Sync")
        .WithSummary("Returns the most recent member achievement sync run.")
        .RequireAdmin();
}

internal static class SyncStatusQueries
{
    public static async Task<SyncStatusDto> GetLatestAsync(
        AppDbContext db,
        SyncKind kind,
        CancellationToken cancellationToken)
    {
        var last = await db.SyncRuns
            .Where(r => r.Kind == kind)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (last is null)
        {
            return new SyncStatusDto(null, null, null, null, null, null);
        }

        return new SyncStatusDto(
            last.Id,
            last.Trigger.ToString(),
            last.Status.ToString(),
            last.StartedAt,
            last.FinishedAt,
            last.Error);
    }
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
