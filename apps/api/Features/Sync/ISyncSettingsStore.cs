using RetroHiscore.Api.Domain;
using RetroHiscore.Api.Features.Admin;

namespace RetroHiscore.Api.Features.Sync;

public interface ISyncSettingsStore
{
    Task<LeaderboardSyncPolicy> GetLeaderboardPolicyAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SyncRecurringJob>> GetRecurringJobsAsync(CancellationToken cancellationToken = default);
    Task<AdminSyncSettingsDto> GetAdminSettingsAsync(CancellationToken cancellationToken = default);
    Task<AdminSyncSettingsDto> UpdateAsync(PatchAdminSyncSettingsRequest patch, CancellationToken cancellationToken = default);
}
