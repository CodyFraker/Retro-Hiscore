using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Admin;
using RetroHiscore.Api.Features.Sync;

namespace RetroHiscore.Api.Tests;

internal sealed class FixedSyncSettingsStore(LeaderboardSyncPolicy policy) : ISyncSettingsStore
{
    public Task<LeaderboardSyncPolicy> GetLeaderboardPolicyAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(policy);

    public Task<IReadOnlyList<SyncRecurringJob>> GetRecurringJobsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SyncRecurringJob>>([]);

    public Task<AdminSyncSettingsDto> GetAdminSettingsAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<AdminSyncSettingsDto> UpdateAsync(PatchAdminSyncSettingsRequest patch, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
