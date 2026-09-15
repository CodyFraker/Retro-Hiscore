using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Ra;

namespace RetroHiscore.Api.Features.Sync;

public interface IMemberRaRankSnapshotSync
{
    Task SyncMemberAsync(Member member, DateTimeOffset syncedAt, CancellationToken cancellationToken = default);
}

public sealed class MemberRaRankSnapshotSync(
    AppDbContext db,
    IRaApiClient raApiClient,
    IRaApiKeyPool apiKeyPool) : IMemberRaRankSnapshotSync
{
    public async Task SyncMemberAsync(
        Member member,
        DateTimeOffset syncedAt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(member.RaUsername))
        {
            return;
        }

        var preferredKeys = string.IsNullOrWhiteSpace(member.RaApiKey)
            ? Array.Empty<string>()
            : new[] { member.RaApiKey };

        var target = !string.IsNullOrWhiteSpace(member.RaUlid) ? member.RaUlid! : member.RaUsername!;
        var summary = await apiKeyPool.ExecuteAsync(
            preferredKeys,
            (key, ct) => raApiClient.GetUserSummaryAsync(
                target,
                key,
                recentGamesCount: 3,
                recentAchievementsCount: 0,
                cancellationToken: ct),
            cancellationToken);

        if (summary is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(member.RaUlid) && !string.IsNullOrWhiteSpace(summary.Ulid))
        {
            member.RaUlid = summary.Ulid;
        }

        member.RaStatus = RaUserSummaryFormatting.FormatStatus(summary.Status);
        member.RaPresenceSyncedAt = syncedAt;
        member.RaPresenceRichPresenceAt = RaUserSummaryFormatting.ParseDateTime(summary.RichPresenceMsgDate);
        if (summary.LastGame is not null)
        {
            member.RaPresenceRaGameId = summary.LastGame.Id;
            member.RaPresenceGameTitle = summary.LastGame.Title;
        }
        else
        {
            member.RaPresenceRaGameId = null;
            member.RaPresenceGameTitle = null;
        }

        var hasMetric = summary.Rank is not null
            || summary.TotalPoints is not null
            || summary.TotalTruePoints is not null
            || summary.TotalSoftcorePoints is not null;

        if (hasMetric)
        {
            db.MemberRaRankSnapshots.Add(new MemberRaRankSnapshot
            {
                MemberId = member.Id,
                Rank = summary.Rank,
                TotalRanked = summary.TotalRanked,
                TotalPoints = summary.TotalPoints,
                TotalTruePoints = summary.TotalTruePoints,
                TotalSoftcorePoints = summary.TotalSoftcorePoints,
                SyncedAt = syncedAt
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
