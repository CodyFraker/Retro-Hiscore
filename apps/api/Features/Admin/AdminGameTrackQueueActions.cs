using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Admin;

public static class AdminGameTrackQueueActions
{
    public static async Task<(AdminGameTrackQueueItemDto? Item, IResult? Error)> RejectAsync(
        Guid queueId,
        Guid? resolvedByMemberId,
        AppDbContext db,
        CancellationToken ct)
    {
        var item = await db.GameTrackQueues.FirstOrDefaultAsync(q => q.Id == queueId, ct);
        if (item is null)
        {
            return (null, Results.NotFound());
        }

        if (item.Status != GameTrackQueueStatus.Pending)
        {
            return (null, Results.Conflict(new { message = "Only pending queue items can be rejected." }));
        }

        item.Status = GameTrackQueueStatus.Rejected;
        item.ResolvedAt = DateTimeOffset.UtcNow;
        item.ResolvedByMemberId = resolvedByMemberId;
        await db.SaveChangesAsync(ct);

        return (ToDto(item), null);
    }

    public static AdminGameTrackQueueItemDto ToDto(GameTrackQueue item)
        => new(
            item.Id,
            item.RaGameId,
            item.Status,
            item.Title,
            item.ConsoleName,
            item.EnqueuedAt,
            item.ResolvedAt,
            item.FailureMessage);
}
