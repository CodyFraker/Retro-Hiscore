using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Games;

public static class GetGameTrackQueueEndpoint
{
    public static RouteHandlerBuilder MapGetGameTrackQueue(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/games/track-queue", async (
            AppDbContext db,
            int? raGameId,
            CancellationToken ct) =>
        {
            var query = db.GameTrackQueues
                .AsNoTracking()
                .Where(q => q.Status == GameTrackQueueStatus.Pending
                    || q.Status == GameTrackQueueStatus.Failed
                    || (q.Status == GameTrackQueueStatus.Completed
                        && q.ResolvedAt >= DateTimeOffset.UtcNow.AddDays(-7)));

            if (raGameId is { } gameId)
            {
                query = query.Where(q => q.RaGameId == gameId);
            }

            var items = await query
                .OrderByDescending(q => q.EnqueuedAt)
                .Select(q => new GameTrackQueueItemDto(
                    q.RaGameId,
                    q.Title,
                    q.ConsoleName,
                    q.Status.ToString(),
                    q.EnqueuedAt,
                    q.ResolvedAt))
                .ToListAsync(ct);

            return Results.Ok(items);
        })
        .WithName("GetGameTrackQueue")
        .WithTags("Games")
        .WithSummary("Lists pending and recently resolved game track queue items visible to all members.")
        .RequireApiAuth();
}

public sealed record GameTrackQueueItemDto(
    int RaGameId,
    string? Title,
    string? ConsoleName,
    string Status,
    DateTimeOffset EnqueuedAt,
    DateTimeOffset? ResolvedAt);
