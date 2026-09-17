using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Admin;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public interface IGameOfTheWeekWinnerTrackingService
{
    Task ProcessPendingWinnersAsync(CancellationToken ct);
}

public sealed class GameOfTheWeekWinnerTrackingService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    ILeaderboardSyncService leaderboardSync,
    ILeaderboardSyncJobEnqueuer jobEnqueuer,
    ISyncSettingsStore syncSettingsStore,
    IConsoleIconSyncService consoleIconSync,
    IOptions<RaOptions> raOptions,
    ILogger<GameOfTheWeekWinnerTrackingService> logger) : IGameOfTheWeekWinnerTrackingService
{
    public async Task ProcessPendingWinnersAsync(CancellationToken ct)
    {
        var pending = await db.GameOfTheWeekPolls
            .Where(p =>
                p.ClosedAt != null
                && p.TrackingStatus == GameOfTheWeekTrackingStatus.Pending
                && p.WinnerRaGameId != null)
            .OrderBy(p => p.ClosedAt)
            .ToListAsync(ct);

        foreach (var poll in pending)
        {
            var winnerId = poll.WinnerRaGameId!.Value;
            if (await db.Games.AnyAsync(g => g.RaGameId == winnerId, ct))
            {
                poll.TrackingStatus = GameOfTheWeekTrackingStatus.Completed;
                await db.SaveChangesAsync(ct);
                continue;
            }

            var (game, error) = await AdminGameTracking.AddGameAsync(
                winnerId,
                db,
                raApiClient,
                apiKeyPool,
                leaderboardSync,
                jobEnqueuer,
                syncSettingsStore,
                consoleIconSync,
                raOptions,
                ct);

            if (error is not null)
            {
                logger.LogWarning(
                    "Game-of-the-week winner tracking failed for poll {PollId} RaGameId {RaGameId}",
                    poll.Id,
                    winnerId);
                continue;
            }

            poll.TrackingStatus = GameOfTheWeekTrackingStatus.Completed;
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Tracked game-of-the-week winner RaGameId {RaGameId} for poll {PollId}",
                winnerId,
                poll.Id);
        }
    }
}
