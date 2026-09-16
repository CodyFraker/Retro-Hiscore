namespace RetroHiscore.Api.Features.Notifications;

public sealed record EnqueueOutboxOptions(
    Guid? SourceSyncRunId = null,
    DateTimeOffset? ReadyAt = null,
    bool WaitForSync = false);
