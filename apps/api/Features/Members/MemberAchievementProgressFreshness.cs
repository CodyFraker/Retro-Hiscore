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
        var runAtTask = db.SyncRuns
            .AsNoTracking()
            .Where(r => r.MemberId == memberId && r.Kind == SyncKind.MemberAchievements)
            .MaxAsync(r => (DateTimeOffset?)(r.FinishedAt ?? r.StartedAt), cancellationToken);

        var dataAtTask = db.MemberRaAchievements
            .AsNoTracking()
            .Where(a => a.MemberId == memberId)
            .MaxAsync(a => (DateTimeOffset?)a.FirstDetectedAt, cancellationToken);

        await Task.WhenAll(runAtTask, dataAtTask);

        return MaxDateTime(await dataAtTask, await runAtTask);
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
