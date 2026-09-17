using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Games;

public interface IGameTrackQueueCompletionService
{
    Task CompletePendingForGameAsync(int raGameId, Guid? resolvedByMemberId, CancellationToken ct);
}

public sealed class GameTrackQueueCompletionService(AppDbContext db) : IGameTrackQueueCompletionService
{
    public Task CompletePendingForGameAsync(int raGameId, Guid? resolvedByMemberId, CancellationToken ct)
        => CompletePendingForGameAsync(raGameId, resolvedByMemberId, db, ct);

    public static async Task CompletePendingForGameAsync(
        int raGameId,
        Guid? resolvedByMemberId,
        AppDbContext db,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var pending = await db.GameTrackQueues
            .Where(q => q.RaGameId == raGameId && q.Status == GameTrackQueueStatus.Pending)
            .ToListAsync(ct);

        foreach (var item in pending)
        {
            item.Status = GameTrackQueueStatus.Completed;
            item.ResolvedAt = now;
            if (resolvedByMemberId is not null)
            {
                item.ResolvedByMemberId = resolvedByMemberId;
            }
        }

        if (pending.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }
    }
}
