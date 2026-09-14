using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Admin;

public sealed record AdminGameTrackQueueItemDto(
    Guid Id,
    int RaGameId,
    GameTrackQueueStatus Status,
    string Title,
    string? ConsoleName,
    DateTimeOffset EnqueuedAt,
    DateTimeOffset? ResolvedAt,
    string? FailureMessage);
