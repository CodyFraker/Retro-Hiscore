using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Members;

internal static class MemberAchievementProgressFreshness
{
    public static async Task<DateTimeOffset?> GetProgressSyncedAtAsync(
        AppDbContext db,
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var runAt = await db.SyncRuns
            .AsNoTracking()
            .Where(r => r.MemberId == memberId && r.Kind == SyncKind.MemberAchievements)
            .MaxAsync(r => (DateTimeOffset?)(r.FinishedAt ?? r.StartedAt), cancellationToken);

        var dataAt = await db.MemberRaAchievements
            .AsNoTracking()
            .Where(a => a.MemberId == memberId)
            .MaxAsync(a => (DateTimeOffset?)a.FirstDetectedAt, cancellationToken);

        return MaxDateTime(dataAt, runAt);
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
