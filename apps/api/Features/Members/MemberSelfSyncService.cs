using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Sync;
using RetroHiscore.Api.Options;

namespace RetroHiscore.Api.Features.Members;

public interface IMemberSelfSyncService
{
    Task<MemberSelfSyncLeaderboardsResult> QueueLeaderboardsAsync(
        Member member,
        CancellationToken cancellationToken = default);

    Task<SyncRun> SyncProfileAsync(Member member, CancellationToken cancellationToken = default);

    Task<SyncRun> SyncAchievementsAsync(Member member, CancellationToken cancellationToken = default);

    Task<MemberSelfSyncStatusDto> GetStatusAsync(Member member, CancellationToken cancellationToken = default);

    bool IsLeaderboardsCooldownActive(Guid memberId, out DateTimeOffset? availableAt);

    bool IsProfileCooldownActive(Guid memberId, out DateTimeOffset? availableAt);

    bool IsAchievementsCooldownActive(Guid memberId, out DateTimeOffset? availableAt);
}

public sealed record MemberSelfSyncLeaderboardsResult(int Queued, int SkippedCooldown);

public sealed record MemberSelfSyncScopeStatusDto(
    DateTimeOffset? LastSyncedAt,
    DateTimeOffset? CooldownUntil,
    DateTimeOffset? GroupScoresLastSyncedAt = null);

public sealed record MemberSelfSyncStatusDto(
    MemberSelfSyncScopeStatusDto Leaderboards,
    MemberSelfSyncScopeStatusDto Profile,
    MemberSelfSyncScopeStatusDto Achievements);

public sealed class MemberSelfSyncService(
    AppDbContext db,
    ILeaderboardSyncService leaderboardSync,
    ILeaderboardSyncJobEnqueuer jobEnqueuer,
    ISyncSettingsStore syncSettingsStore,
    IMemberRaRankSnapshotSync memberRaRankSnapshotSync,
    IMemberRecentGamesSyncService memberRecentGamesSync,
    IMemberRaGameProgressSyncService memberRaGameProgressSync,
    IOptions<SyncOptions> syncOptions,
    ILogger<MemberSelfSyncService> logger) : IMemberSelfSyncService
{
    private readonly SyncOptions _syncOptions = syncOptions.Value;

    public bool IsLeaderboardsCooldownActive(Guid memberId, out DateTimeOffset? availableAt)
        => ManualSyncCooldown.IsActive(
            db,
            SyncKind.LeaderboardScores,
            _syncOptions.ManualCooldownSeconds,
            memberId,
            out availableAt);

    public bool IsProfileCooldownActive(Guid memberId, out DateTimeOffset? availableAt)
    {
        if (ManualSyncCooldown.IsActive(db, SyncKind.MemberRank, _syncOptions.ManualCooldownSeconds, memberId, out availableAt))
        {
            return true;
        }

        return ManualSyncCooldown.IsActive(
            db,
            SyncKind.MemberActivity,
            _syncOptions.ManualCooldownSeconds,
            memberId,
            out availableAt);
    }

    public bool IsAchievementsCooldownActive(Guid memberId, out DateTimeOffset? availableAt)
        => ManualSyncCooldown.IsActive(
            db,
            SyncKind.MemberAchievements,
            _syncOptions.ManualCooldownSeconds,
            memberId,
            out availableAt);

    public async Task<MemberSelfSyncLeaderboardsResult> QueueLeaderboardsAsync(
        Member member,
        CancellationToken cancellationToken = default)
    {
        if (IsLeaderboardsCooldownActive(member.Id, out _))
        {
            throw new InvalidOperationException("Leaderboard sync cooldown is active.");
        }

        var policy = await syncSettingsStore.GetLeaderboardPolicyAsync(cancellationToken);
        var trackedRaGameIds = await db.Games.AsNoTracking().Select(g => g.RaGameId).ToListAsync(cancellationToken);
        var eligibleRaGameIds = await LeaderboardSyncMemberEligibility.GetEligibleTrackedRaGameIdsForMemberAsync(
            db,
            member.Id,
            trackedRaGameIds.ToHashSet(),
            DateTimeOffset.UtcNow,
            policy,
            cancellationToken);
        var games = await db.Games
            .AsNoTracking()
            .Where(g => eligibleRaGameIds.Contains(g.RaGameId))
            .ToListAsync(cancellationToken);
        var queued = 0;
        var skipped = 0;

        foreach (var game in games)
        {
            if (leaderboardSync.IsPerGameRefreshCooldownActive(game.Id, member.Id, out _))
            {
                skipped++;
                continue;
            }

            var run = new SyncRun
            {
                Kind = SyncKind.LeaderboardScores,
                Trigger = SyncTrigger.Manual,
                Status = SyncRunStatus.Running,
                StartedAt = DateTimeOffset.UtcNow,
                GameId = game.Id,
                MemberId = member.Id
            };
            db.SyncRuns.Add(run);
            await db.SaveChangesAsync(cancellationToken);

            jobEnqueuer.EnqueueMemberGameSync(game.RaGameId, member.Id, SyncTrigger.Manual, run.Id);
            queued++;
        }

        return new MemberSelfSyncLeaderboardsResult(queued, skipped);
    }

    public async Task<SyncRun> SyncProfileAsync(Member member, CancellationToken cancellationToken = default)
    {
        if (IsProfileCooldownActive(member.Id, out _))
        {
            throw new InvalidOperationException("Profile sync cooldown is active.");
        }

        var rankRun = new SyncRun
        {
            Kind = SyncKind.MemberRank,
            Trigger = SyncTrigger.Manual,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow,
            MemberId = member.Id
        };
        db.SyncRuns.Add(rankRun);
        await db.SaveChangesAsync(cancellationToken);

        var syncedAt = DateTimeOffset.UtcNow;
        var errors = new List<string>();

        try
        {
            await memberRaRankSnapshotSync.SyncMemberAsync(member, syncedAt, cancellationToken);
            rankRun.Status = SyncRunStatus.Succeeded;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Profile rank sync failed for member {Member}", member.RaUsername);
            rankRun.Status = SyncRunStatus.Failed;
            rankRun.Error = ex.Message;
            errors.Add(ex.Message);
        }
        finally
        {
            rankRun.FinishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        var activityRun = new SyncRun
        {
            Kind = SyncKind.MemberActivity,
            Trigger = SyncTrigger.Manual,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow,
            MemberId = member.Id
        };
        db.SyncRuns.Add(activityRun);
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            await memberRecentGamesSync.SyncMemberAsync(member, DateTimeOffset.UtcNow, cancellationToken);
            activityRun.Status = SyncRunStatus.Succeeded;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Profile recent games sync failed for member {Member}", member.RaUsername);
            activityRun.Status = SyncRunStatus.Failed;
            activityRun.Error = ex.Message;
            errors.Add(ex.Message);
        }
        finally
        {
            activityRun.FinishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        if (errors.Count > 0 && rankRun.Status != SyncRunStatus.Succeeded)
        {
            return rankRun;
        }

        return activityRun.Status == SyncRunStatus.Succeeded ? activityRun : rankRun;
    }

    public async Task<SyncRun> SyncAchievementsAsync(Member member, CancellationToken cancellationToken = default)
    {
        if (IsAchievementsCooldownActive(member.Id, out _))
        {
            throw new InvalidOperationException("Achievement sync cooldown is active.");
        }

        var run = new SyncRun
        {
            Kind = SyncKind.MemberAchievements,
            Trigger = SyncTrigger.Manual,
            Status = SyncRunStatus.Running,
            StartedAt = DateTimeOffset.UtcNow,
            MemberId = member.Id
        };
        db.SyncRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var syncedAt = DateTimeOffset.UtcNow;

        try
        {
            await memberRaGameProgressSync.SyncMemberTrackedGamesAsync(member, syncedAt, cancellationToken);
            run.Status = SyncRunStatus.Succeeded;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Achievement sync failed for member {Member}", member.RaUsername);
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

    public async Task<MemberSelfSyncStatusDto> GetStatusAsync(
        Member member,
        CancellationToken cancellationToken = default)
    {
        var leaderboardsAtValue = await db.SyncRuns
            .AsNoTracking()
            .Where(r => r.MemberId == member.Id && r.Kind == SyncKind.LeaderboardScores)
            .MaxAsync(r => (DateTimeOffset?)(r.FinishedAt ?? r.StartedAt), cancellationToken);

        var groupLeaderboardsAt = await db.SyncRuns
            .AsNoTracking()
            .Where(r =>
                r.MemberId == null
                && r.Kind == SyncKind.LeaderboardScores
                && r.Status == SyncRunStatus.Succeeded
                && r.FinishedAt != null)
            .MaxAsync(r => r.FinishedAt, cancellationToken);

        var rankAt = await db.MemberRaRankSnapshots
            .AsNoTracking()
            .Where(s => s.MemberId == member.Id)
            .MaxAsync(s => (DateTimeOffset?)s.SyncedAt, cancellationToken);

        var presenceAt = member.RaPresenceSyncedAt;
        var recentAt = await db.MemberRecentGamePlays
            .AsNoTracking()
            .Where(p => p.MemberId == member.Id)
            .MaxAsync(p => (DateTimeOffset?)p.SyncedAt, cancellationToken);

        var profileAt = MaxDateTime(rankAt, presenceAt, recentAt);

        var achievementsAt = await MemberAchievementProgressFreshness.GetProgressSyncedAtAsync(
            db,
            member.Id,
            cancellationToken);

        IsLeaderboardsCooldownActive(member.Id, out var lbCooldown);
        IsProfileCooldownActive(member.Id, out var profileCooldown);
        IsAchievementsCooldownActive(member.Id, out var achCooldown);

        return new MemberSelfSyncStatusDto(
            new MemberSelfSyncScopeStatusDto(leaderboardsAtValue, lbCooldown, groupLeaderboardsAt),
            new MemberSelfSyncScopeStatusDto(profileAt, profileCooldown),
            new MemberSelfSyncScopeStatusDto(achievementsAt, achCooldown));
    }

    private static DateTimeOffset? MaxDateTime(DateTimeOffset? a, DateTimeOffset? b, DateTimeOffset? c)
    {
        var values = new[] { a, b, c }.Where(v => v is not null).Select(v => v!.Value).ToList();
        return values.Count == 0 ? null : values.Max();
    }

    private static DateTimeOffset? MaxDateTime(DateTimeOffset? a, DateTimeOffset? b)
    {
        if (a is null)
        {
            return b;
        }

        if (b is null)
        {
            return a;
        }

        return a > b ? a : b;
    }
}
