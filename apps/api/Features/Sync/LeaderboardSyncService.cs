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
    Task<SyncRun> SyncGameWithRunAsync(Game game, SyncTrigger trigger, CancellationToken cancellationToken = default);
    Task<SyncRun> SyncMemberGameWithRunAsync(
        Game game,
        Member member,
        SyncTrigger trigger,
        Guid? existingRunId = null,
        CancellationToken cancellationToken = default);
    Task SyncGameAsync(Game game, CancellationToken cancellationToken = default);
    Task SyncGameForMembersAsync(
        Game game,
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken = default);
    bool IsManualCooldownActive(out DateTimeOffset? availableAt);
    bool IsPerGameRefreshCooldownActive(Guid gameId, Guid memberId, out DateTimeOffset? availableAt);
}

public sealed class LeaderboardSyncService(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool,
    IDiscordNotificationService notificationService,
    IOptions<RaOptions> raOptions,
    IOptions<SyncOptions> syncOptions,
    ILogger<LeaderboardSyncService> logger) : ILeaderboardSyncService
{
    private readonly RaOptions _raOptions = raOptions.Value;
    private readonly SyncOptions _syncOptions = syncOptions.Value;

    public bool IsManualCooldownActive(out DateTimeOffset? availableAt)
    {
        availableAt = null;
        var lastManual = db.SyncRuns
            .Where(r => r.Kind == SyncKind.LeaderboardScores
                && r.Trigger == SyncTrigger.Manual
                && r.MemberId == null)
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

    public bool IsPerGameRefreshCooldownActive(Guid gameId, Guid memberId, out DateTimeOffset? availableAt)
    {
        availableAt = null;
        var lastManual = db.SyncRuns
            .Where(r => r.Kind == SyncKind.LeaderboardScores
                && r.Trigger == SyncTrigger.Manual
                && r.GameId == gameId
                && r.MemberId == memberId)
            .OrderByDescending(r => r.StartedAt)
            .FirstOrDefault();

        if (lastManual is null)
        {
            return false;
        }

        var cooldownEnds = lastManual.StartedAt.AddSeconds(_syncOptions.PerGameRefreshCooldownSeconds);
        if (cooldownEnds <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        availableAt = cooldownEnds;
        return true;
    }

    public async Task<SyncRun> SyncGameWithRunAsync(
        Game game,
        SyncTrigger trigger,
        CancellationToken cancellationToken = default)
    {
        var run = new SyncRun
        {
            Kind = SyncKind.LeaderboardScores,
            Trigger = trigger,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow,
            GameId = game.Id
        };
        db.SyncRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var syncedAt = DateTimeOffset.UtcNow;
        var errors = new List<string>();

        try
        {
            await SyncGameForMembersInternalAsync(game, memberIds: null, syncedAt, errors, cancellationToken);
            run.Status = errors.Count == 0 ? SyncRunStatus.Succeeded : SyncRunStatus.PartialSuccess;
            run.Error = errors.Count == 0 ? null : string.Join("; ", errors);

            if (run.Status is SyncRunStatus.Succeeded or SyncRunStatus.PartialSuccess)
            {
                game.LeaderboardScoresSyncedAt = syncedAt;
            }

            var activity = await ActivityBuilder.BuildForGameAsync(db, game.Id, syncedAt, cancellationToken);
            await notificationService.NotifyActivityAsync(activity, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Game leaderboard sync failed for {RaGameId}", game.RaGameId);
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

    public async Task<SyncRun> SyncMemberGameWithRunAsync(
        Game game,
        Member member,
        SyncTrigger trigger,
        Guid? existingRunId = null,
        CancellationToken cancellationToken = default)
    {
        SyncRun run;
        if (existingRunId is { } runId)
        {
            run = await db.SyncRuns.FirstAsync(r => r.Id == runId, cancellationToken);
        }
        else
        {
            run = new SyncRun
            {
                Kind = SyncKind.LeaderboardScores,
                Trigger = trigger,
                Status = SyncRunStatus.Running,
                StartedAt = DateTimeOffset.UtcNow,
                GameId = game.Id,
                MemberId = member.Id
            };
            db.SyncRuns.Add(run);
            await db.SaveChangesAsync(cancellationToken);
        }

        var syncedAt = DateTimeOffset.UtcNow;
        var errors = new List<string>();

        try
        {
            try
            {
                await SyncGameCatalogAsync(game, cancellationToken);
                await SyncLeaderboardPopulationCountsAsync(game, syncedAt, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync catalog for game {GameId}", game.RaGameId);
                errors.Add($"catalog: {ex.Message}");
            }

            try
            {
                await SyncMemberGameAsync(member, game, syncedAt, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync member {Member} for game {GameId}", member.RaUsername, game.RaGameId);
                errors.Add($"{member.RaUsername}: {ex.Message}");
            }

            await RecomputeFriendRanksAsync(syncedAt, game.Id, cancellationToken);

            run.Status = errors.Count == 0 ? SyncRunStatus.Succeeded : SyncRunStatus.PartialSuccess;
            run.Error = errors.Count == 0 ? null : string.Join("; ", errors);

            var activity = await ActivityBuilder.BuildForGameAsync(db, game.Id, syncedAt, cancellationToken);
            await notificationService.NotifyActivityAsync(activity, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Member game sync failed for {RaGameId}", game.RaGameId);
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

    public Task SyncGameAsync(Game game, CancellationToken cancellationToken = default)
        => SyncGameForMembersInternalAsync(game, memberIds: null, DateTimeOffset.UtcNow, errors: null, cancellationToken);

    public async Task SyncGameForMembersAsync(
        Game game,
        IReadOnlyCollection<Guid> memberIds,
        CancellationToken cancellationToken = default)
    {
        var ids = memberIds.Count == 0 ? null : memberIds;
        await SyncGameForMembersInternalAsync(game, ids, DateTimeOffset.UtcNow, errors: null, cancellationToken);
    }

    private async Task SyncGameForMembersInternalAsync(
        Game game,
        IReadOnlyCollection<Guid>? memberIds,
        DateTimeOffset syncedAt,
        List<string>? errors,
        CancellationToken cancellationToken)
    {
        var membersQuery = db.Members.Where(m => m.RaUsername != null);
        if (memberIds is not null)
        {
            membersQuery = membersQuery.Where(m => memberIds.Contains(m.Id));
        }

        var members = await membersQuery.ToListAsync(cancellationToken);
        if (members.Count == 0 && memberIds is not null)
        {
            members = await db.Members
                .Where(m => m.RaUsername != null)
                .ToListAsync(cancellationToken);
        }

        try
        {
            await SyncGameCatalogAsync(game, cancellationToken);
            await SyncLeaderboardPopulationCountsAsync(game, syncedAt, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync catalog for game {GameId}", game.RaGameId);
            errors?.Add($"Game {game.RaGameId} catalog: {ex.Message}");
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
                errors?.Add($"{member.RaUsername}/{game.RaGameId}: {ex.Message}");
            }
        }

        await RecomputeFriendRanksAsync(syncedAt, game.Id, cancellationToken);
    }

    private async Task SyncLeaderboardPopulationCountsAsync(
        Game game,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken)
    {
        var leaderboards = await db.Leaderboards
            .Where(l => l.GameId == game.Id)
            .ToListAsync(cancellationToken);

        foreach (var leaderboard in leaderboards)
        {
            try
            {
                var count = await apiKeyPool.ExecuteAsync(
                    [_raOptions.ApiKey],
                    (key, ct) => raApiClient.GetLeaderboardEntryCountAsync(leaderboard.RaLeaderboardId, key, ct),
                    cancellationToken);

                if (count is null)
                {
                    continue;
                }

                leaderboard.GlobalEntryCount = count;
                leaderboard.GlobalEntryCountSyncedAt = syncedAt;

                db.LeaderboardPopulationSnapshots.Add(new LeaderboardPopulationSnapshot
                {
                    LeaderboardId = leaderboard.Id,
                    EntryCount = count.Value,
                    SyncedAt = syncedAt
                });
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to sync population count for leaderboard {LeaderboardId}",
                    leaderboard.RaLeaderboardId);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncGameCatalogAsync(Game game, CancellationToken cancellationToken)
    {
        var boards = await apiKeyPool.ExecuteAsync(
            [_raOptions.ApiKey],
            (key, ct) => raApiClient.GetGameLeaderboardsAsync(game.RaGameId, key, ct),
            cancellationToken);

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
        if (string.IsNullOrWhiteSpace(member.RaUsername) || string.IsNullOrWhiteSpace(member.RaApiKey))
        {
            return;
        }

        var identity = !string.IsNullOrWhiteSpace(member.RaUlid) ? member.RaUlid! : member.RaUsername!;
        var boards = await apiKeyPool.ExecuteAsync(
            [member.RaApiKey],
            (key, ct) => raApiClient.GetUserGameLeaderboardsAsync(game.RaGameId, identity, key, ct),
            cancellationToken);

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
            entry.ScoreUpdatedAt = board.UserEntry.DateUpdated?.ToUniversalTime();
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
