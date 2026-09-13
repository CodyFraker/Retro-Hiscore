using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Dashboard;
using RetroHiscore.Api.Features.Notifications;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Sync;

public interface ILeaderboardSyncService
{
    Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default);
    Task SyncGameAsync(Game game, CancellationToken cancellationToken = default);
    bool IsManualCooldownActive(out DateTimeOffset? availableAt);
}

public sealed class LeaderboardSyncService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IDiscordNotificationService notificationService,
    IOptions<SyncOptions> syncOptions,
    ILogger<LeaderboardSyncService> logger) : ILeaderboardSyncService
{
    private readonly SyncOptions _syncOptions = syncOptions.Value;

    public bool IsManualCooldownActive(out DateTimeOffset? availableAt)
    {
        availableAt = null;
        var lastManual = db.SyncRuns
            .Where(r => r.Kind == SyncKind.LeaderboardScores && r.Trigger == SyncTrigger.Manual)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefault();

        if (lastManual is null)
        {
            return false;
        }

        var cooldownEnds = lastManual.StartedAt.AddSeconds(_syncOptions.ManualCooldownSeconds);
        if (cooldownEnds <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        availableAt = cooldownEnds;
        return true;
    }

    public async Task<SyncRun> SyncAsync(SyncTrigger trigger, CancellationToken cancellationToken = default)
    {
        var run = new SyncRun
        {
            Kind = SyncKind.LeaderboardScores,
            Trigger = trigger,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow
        };
        db.SyncRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var errors = new List<string>();
        var syncedAt = DateTimeOffset.UtcNow;

        try
        {
            var games = await db.Games.ToListAsync(cancellationToken);
            var members = await db.Members.ToListAsync(cancellationToken);

            foreach (var game in games)
            {
                try
                {
                    await SyncGameCatalogAsync(game, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to sync catalog for game {GameId}", game.RaGameId);
                    errors.Add($"Game {game.RaGameId} catalog: {ex.Message}");
                }
            }

            foreach (var member in members)
            {
                foreach (var game in games)
                {
                    try
                    {
                        await SyncMemberGameAsync(member, game, syncedAt, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to sync member {Member} for game {GameId}", member.RaUsername, game.RaGameId);
                        errors.Add($"{member.RaUsername}/{game.RaGameId}: {ex.Message}");
                    }
                }
            }

            await RecomputeFriendRanksAsync(syncedAt, gameId: null, cancellationToken);

            var activity = await ActivityBuilder.BuildAsync(db, cancellationToken);
            await notificationService.NotifyActivityAsync(activity, cancellationToken);

            run.Status = errors.Count == 0
                ? SyncRunStatus.Succeeded
                : SyncRunStatus.PartialSuccess;
            run.Error = errors.Count == 0 ? null : string.Join("; ", errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sync run failed");
            run.Status = SyncRunStatus.Failed;
            run.Error = ex.Message;
        }
        finally
        {
            run.FinishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        return run;
    }

    public async Task SyncGameAsync(Game game, CancellationToken cancellationToken = default)
    {
        var syncedAt = DateTimeOffset.UtcNow;
        var members = await db.Members.ToListAsync(cancellationToken);

        try
        {
            await SyncGameCatalogAsync(game, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync catalog for game {GameId}", game.RaGameId);
        }

        foreach (var member in members)
        {
            try
            {
                await SyncMemberGameAsync(member, game, syncedAt, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync member {Member} for game {GameId}", member.RaUsername, game.RaGameId);
            }
        }

        await RecomputeFriendRanksAsync(syncedAt, game.Id, cancellationToken);
    }

    private async Task SyncGameCatalogAsync(Game game, CancellationToken cancellationToken)
    {
        var boards = await raApiClient.GetGameLeaderboardsAsync(game.RaGameId, cancellationToken);

        foreach (var board in boards)
        {
            var existing = await db.Leaderboards
                .FirstOrDefaultAsync(l => l.RaLeaderboardId == board.Id, cancellationToken);

            if (existing is null)
            {
                db.Leaderboards.Add(new Leaderboard
                {
                    RaLeaderboardId = board.Id,
                    GameId = game.Id,
                    Title = board.Title,
                    Description = board.Description,
                    Format = board.Format,
                    RankAsc = board.RankAsc
                });
            }
            else
            {
                existing.Title = board.Title;
                existing.Description = board.Description;
                existing.Format = board.Format;
                existing.RankAsc = board.RankAsc;
                existing.GameId = game.Id;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncMemberGameAsync(Member member, Game game, DateTimeOffset syncedAt, CancellationToken cancellationToken)
    {
        var identity = !string.IsNullOrWhiteSpace(member.RaUlid) ? member.RaUlid! : member.RaUsername;
        var boards = await raApiClient.GetUserGameLeaderboardsAsync(game.RaGameId, identity, cancellationToken);

        if (boards.Count == 0)
        {
            return;
        }

        foreach (var board in boards)
        {
            if (board.UserEntry is null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(board.UserEntry.Ulid) && member.RaUlid != board.UserEntry.Ulid)
            {
                member.RaUlid = board.UserEntry.Ulid;
            }

            if (!string.IsNullOrWhiteSpace(board.UserEntry.User) &&
                !string.Equals(member.RaUsername, board.UserEntry.User, StringComparison.Ordinal))
            {
                member.RaUsername = board.UserEntry.User;
                member.DisplayName ??= board.UserEntry.User;
            }

            var leaderboard = await db.Leaderboards
                .FirstOrDefaultAsync(l => l.RaLeaderboardId == board.Id, cancellationToken);

            if (leaderboard is null)
            {
                leaderboard = new Leaderboard
                {
                    RaLeaderboardId = board.Id,
                    GameId = game.Id,
                    Title = board.Title,
                    Description = board.Description,
                    Format = board.Format,
                    RankAsc = board.RankAsc
                };
                db.Leaderboards.Add(leaderboard);
                await db.SaveChangesAsync(cancellationToken);
            }

            var entry = await db.LeaderboardEntries
                .FirstOrDefaultAsync(e => e.LeaderboardId == leaderboard.Id && e.MemberId == member.Id, cancellationToken);

            if (entry is null)
            {
                entry = new LeaderboardEntry
                {
                    LeaderboardId = leaderboard.Id,
                    MemberId = member.Id
                };
                db.LeaderboardEntries.Add(entry);
            }

            entry.Score = board.UserEntry.Score;
            entry.FormattedScore = board.UserEntry.FormattedScore;
            entry.GlobalRank = board.UserEntry.Rank;
            entry.ScoreUpdatedAt = board.UserEntry.DateUpdated;
            entry.SyncedAt = syncedAt;

            db.LeaderboardEntrySnapshots.Add(new LeaderboardEntrySnapshot
            {
                LeaderboardId = leaderboard.Id,
                MemberId = member.Id,
                Score = entry.Score,
                FormattedScore = entry.FormattedScore,
                GlobalRank = entry.GlobalRank,
                FriendRank = entry.FriendRank,
                SyncedAt = syncedAt
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task RecomputeFriendRanksAsync(
        DateTimeOffset syncedAt,
        Guid? gameId,
        CancellationToken cancellationToken)
    {
        var query = db.Leaderboards.Include(l => l.Entries).AsQueryable();
        if (gameId is not null)
        {
            query = query.Where(l => l.GameId == gameId);
        }

        var leaderboards = await query.ToListAsync(cancellationToken);

        foreach (var leaderboard in leaderboards)
        {
            var ranks = FriendRankCalculator.Calculate(
                leaderboard.Entries.Select(e => new FriendRankCalculator.RankableScore(e.MemberId, e.Score)),
                leaderboard.RankAsc);

            foreach (var entry in leaderboard.Entries)
            {
                entry.FriendRank = ranks.TryGetValue(entry.MemberId, out var rank) ? rank : null;
            }

            var snapshots = await db.LeaderboardEntrySnapshots
                .Where(s => s.LeaderboardId == leaderboard.Id && s.SyncedAt == syncedAt)
                .ToListAsync(cancellationToken);

            foreach (var snapshot in snapshots)
            {
                snapshot.FriendRank = ranks.TryGetValue(snapshot.MemberId, out var rank) ? rank : null;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class SyncJob(ILeaderboardSyncService syncService)
{
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
    [AutomaticRetry(Attempts = 0)]
    public Task RunScheduledAsync(CancellationToken cancellationToken = default)
        => syncService.SyncAsync(SyncTrigger.Scheduled, cancellationToken);

    [DisableConcurrentExecution(timeoutInSeconds: 60 * 30)]
    [AutomaticRetry(Attempts = 0)]
    public Task RunManualAsync(CancellationToken cancellationToken = default)
        => syncService.SyncAsync(SyncTrigger.Manual, cancellationToken);
}
