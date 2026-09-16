using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Members;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Sync;

public static class GetSyncHealthEndpoint
{
    public static RouteHandlerBuilder MapGetSyncHealth(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/sync/health", async (
            ClaimsPrincipal user,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var latestLeaderboard = await db.SyncRuns
                .AsNoTracking()
                .Where(r => r.Kind == SyncKind.LeaderboardScores)
                .OrderByDescending(r => r.StartedAt)
                .FirstOrDefaultAsync(ct);

            var lastSuccessfulLeaderboard = await db.SyncRuns
                .AsNoTracking()
                .Where(r => r.Kind == SyncKind.LeaderboardScores && r.Status == SyncRunStatus.Succeeded)
                .OrderByDescending(r => r.FinishedAt)
                .FirstOrDefaultAsync(ct);

            var leaderboardInProgress = await db.SyncRuns
                .AsNoTracking()
                .AnyAsync(
                    r => r.Kind == SyncKind.LeaderboardScores
                        && r.Status == SyncRunStatus.Running
                        && r.FinishedAt == null,
                    ct);

            var groupStatus = SyncGroupHealthEvaluator.DeriveGroupStatus(latestLeaderboard);
            var groupMessage = groupStatus switch
            {
                SyncGroupHealthEvaluator.StatusRateLimited =>
                    "RetroAchievements rate limits are slowing leaderboard sync. Scores will catch up automatically.",
                SyncGroupHealthEvaluator.StatusDegraded =>
                    "Leaderboard sync is behind. Recent scores may be stale until the next run succeeds.",
                _ => null
            };

            IReadOnlyList<MemberSelfSyncIssueDto> memberIssues = [];
            var member = await MemberSelfSyncEndpointHelpers.ResolveMemberAsync(user, db, ct);
            if (member is not null)
            {
                memberIssues = await BuildMemberIssuesAsync(db, member.Id, ct);
            }

            return Results.Ok(new SyncHealthResponse(
                groupStatus,
                groupMessage,
                leaderboardInProgress,
                lastSuccessfulLeaderboard?.FinishedAt,
                memberIssues));
        })
        .WithName("GetSyncHealth")
        .WithTags("Sync")
        .WithSummary("Returns group leaderboard sync health and recent self-sync issues for the current member.")
        .RequireApiAuth();

    private static async Task<IReadOnlyList<MemberSelfSyncIssueDto>> BuildMemberIssuesAsync(
        AppDbContext db,
        Guid memberId,
        CancellationToken ct)
    {
        var kinds = new[]
        {
            SyncKind.LeaderboardScores,
            SyncKind.MemberRank,
            SyncKind.MemberActivity,
            SyncKind.MemberAchievements
        };

        var issues = new List<MemberSelfSyncIssueDto>();
        foreach (var kind in kinds)
        {
            var run = await db.SyncRuns
                .AsNoTracking()
                .Where(r => r.MemberId == memberId && r.Kind == kind && r.Trigger == SyncTrigger.Manual)
                .OrderByDescending(r => r.StartedAt)
                .FirstOrDefaultAsync(ct);

            if (run is null || run.Status is SyncRunStatus.Succeeded or SyncRunStatus.Running)
            {
                continue;
            }

            var message = SyncGroupHealthEvaluator.IsRateLimitedError(run.Error)
                ? "Rate limited by RetroAchievements. Try again in a few minutes."
                : "Your last sync did not finish. Try again from Settings.";

            issues.Add(new MemberSelfSyncIssueDto(kind.ToString(), message, run.FinishedAt ?? run.StartedAt));
        }

        return issues;
    }
}

public sealed record MemberSelfSyncIssueDto(string Scope, string Message, DateTimeOffset OccurredAt);

public sealed record SyncHealthResponse(
    string GroupStatus,
    string? GroupMessage,
    bool LeaderboardSyncInProgress,
    DateTimeOffset? LeaderboardScoresLastSuccessAt,
    IReadOnlyList<MemberSelfSyncIssueDto> MemberIssues);
