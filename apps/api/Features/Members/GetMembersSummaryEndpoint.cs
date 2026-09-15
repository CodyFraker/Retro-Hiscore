using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Features.Dashboard;
using RetroHiscore.Api.Features.Ra;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Members;

public static class GetMembersSummaryEndpoint
{
    public static RouteHandlerBuilder MapGetMembersSummary(this IEndpointRouteBuilder routes)
        => routes.MapGet("/api/members/summary", async (AppDbContext db, CancellationToken ct) =>
        {
            var memberCount = await db.Members.CountAsync(m => m.RaUsername != null, ct);

            var entries = await db.LeaderboardEntries
                .AsNoTracking()
                .Include(e => e.Member)
                .ToListAsync(ct);

            var championshipRow = entries
                .GroupBy(e => e.MemberId)
                .Select(g =>
                {
                    var member = g.First().Member;
                    return new
                    {
                        DisplayName = MemberAuthHelper.DisplayLabel(member),
                        member.RaUsername,
                        FriendRankOnes = g.Count(e => e.FriendRank == 1)
                    };
                })
                .OrderByDescending(c => c.FriendRankOnes)
                .ThenBy(c => c.DisplayName)
                .FirstOrDefault();

            string? leaderDisplayName = championshipRow?.DisplayName;
            string? leaderRaUsername = championshipRow?.RaUsername;
            int? leaderFriendRankOnes = championshipRow?.FriendRankOnes;

            var achievementSummary = await DashboardAchievementSummaryQuery.LoadAsync(db, ct);

            var memberStatuses = await db.Members
                .AsNoTracking()
                .Where(m => m.RaUsername != null && m.RaStatus != null)
                .Select(m => m.RaStatus!)
                .ToListAsync(ct);
            var playingNowCount = memberStatuses.Count(RaUserSummaryFormatting.IsOnlineOrPlaying);

            var raMetricsFreshnessAt = await db.MemberRaRankSnapshots
                .AsNoTracking()
                .MaxAsync(s => (DateTimeOffset?)s.SyncedAt, ct);

            return Results.Ok(new MembersSummaryDto(
                memberCount,
                leaderDisplayName,
                leaderRaUsername,
                leaderFriendRankOnes,
                achievementSummary.UnlocksLast7Days,
                achievementSummary.UnlocksLast30Days,
                achievementSummary.ActiveMembersLast7Days,
                achievementSummary.LastUnlockAt,
                achievementSummary.TopGameLast7Days,
                playingNowCount,
                raMetricsFreshnessAt));
        })
        .WithName("GetMembersSummary")
        .WithTags("Members")
        .WithSummary("Returns aggregate stats for the members roster page.")
        .RequireApiAuth();
}

public sealed record MembersSummaryDto(
    int MemberCount,
    string? ChampionshipLeaderDisplayName,
    string? ChampionshipLeaderRaUsername,
    int? ChampionshipLeaderFriendRankOnes,
    int UnlocksLast7Days,
    int UnlocksLast30Days,
    int ActiveMembersLast7Days,
    DateTimeOffset? LastUnlockAt,
    DashboardAchievementTopGameDto? TopGameLast7Days,
    int PlayingNowCount,
    DateTimeOffset? RaMetricsFreshnessAt);
