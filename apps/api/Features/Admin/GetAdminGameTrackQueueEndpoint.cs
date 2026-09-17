using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Admin;

public static class GetAdminGameTrackQueueEndpoint
{
    public static RouteHandlerBuilder MapGetAdminGameTrackQueue(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/admin/game-track-queue", async (AppDbContext db, CancellationToken ct) =>
        {
            var items = await db.GameTrackQueues
                .AsNoTracking()
                .Include(q => q.RequestedByMember)
                .Where(q => q.Status == GameTrackQueueStatus.Pending
                    || q.Status == GameTrackQueueStatus.Failed
                    || (q.Status == GameTrackQueueStatus.Completed
                        && q.ResolvedAt >= DateTimeOffset.UtcNow.AddDays(-7)))
                .OrderByDescending(q => q.EnqueuedAt)
                .ToListAsync(ct);

            var dtos = items.Select(AdminGameTrackQueueActions.ToDto).ToList();
            return Results.Ok(dtos);
        })
        .WithName("GetAdminGameTrackQueue")
        .WithTags("Admin")
        .WithSummary("Lists pending and recently resolved game track queue items.")
        .RequireAdmin();
}
