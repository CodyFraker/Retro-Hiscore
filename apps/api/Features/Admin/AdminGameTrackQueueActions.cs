using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;

namespace RetroHiscore.Api.Features.Admin;

public static class AdminGameTrackQueueActions
{
    public static async Task<(AdminGameTrackQueueItemDto? Item, IResult? Error)> ApproveAsync(
        Guid queueId,
        Guid? resolvedByMemberId,
        AppDbContext db,
        IRaApiClient raApiClient,
        IRaApiKeyPool apiKeyPool,
        ILeaderboardSyncService leaderboardSync,
        IConsoleIconSyncService consoleIconSync,
        IOptions<RaOptions> raOptions,
        CancellationToken ct)
    {
        var item = await db.GameTrackQueues.FirstOrDefaultAsync(q => q.Id == queueId, ct);
        if (item is null)
        {
            return (null, Results.NotFound());
        }

        if (item.Status != GameTrackQueueStatus.Pending)
        {
            return (null, Results.Conflict(new { message = "Only pending queue items can be approved." }));
        }

        var now = DateTimeOffset.UtcNow;
        item.Status = GameTrackQueueStatus.Approved;
        item.ResolvedAt = now;
        item.ResolvedByMemberId = resolvedByMemberId;
        await db.SaveChangesAsync(ct);

        var (game, addError) = await AdminGameTracking.AddGameAsync(
            item.RaGameId,
            db,
            raApiClient,
            apiKeyPool,
            leaderboardSync,
            consoleIconSync,
            raOptions,
            ct,
            syncMemberScores: false);

        if (addError is not null)
        {
            var alreadyTracked = await db.Games.AnyAsync(g => g.RaGameId == item.RaGameId, ct);
            if (!alreadyTracked)
            {
                item.Status = GameTrackQueueStatus.Failed;
                item.FailureMessage = "Could not add game from RetroAchievements.";
                await db.SaveChangesAsync(ct);
                return (null, addError);
            }

            game = await db.Games.FirstAsync(g => g.RaGameId == item.RaGameId, ct);
        }

        var memberIds = await db.MemberRecentGamePlays
            .Where(p => p.RaGameId == item.RaGameId)
            .Select(p => p.MemberId)
            .Distinct()
            .ToListAsync(ct);

        await leaderboardSync.SyncGameForMembersAsync(game!, memberIds, ct);

        item.Status = GameTrackQueueStatus.Completed;
        item.FailureMessage = null;
        await db.SaveChangesAsync(ct);

        return (ToDto(item), null);
    }

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
