using RetroHiscore.Api.Features.Sync;

namespace RetroHiscore.Api.Features.Admin;

public sealed record AdminGameDto(
    Guid Id,
    int RaGameId,
    string Title,
    string? ConsoleName,
    string? ConsoleIconUrl,
    string? ImageBoxArtUrl,
    string? ImageIconUrl,
    string? ImageTitleUrl,
    string? ImageIngameUrl,
    int LeaderboardCount,
    int SourceCount,
    DateTimeOffset? MetadataSyncedAt,
    bool ForceColdLeaderboardSync,
    GameLeaderboardSyncStatusDto LeaderboardSyncStatus);

public sealed record PostAdminGameRequest(int RaGameId);

public sealed record PatchAdminGameLeaderboardSyncRequest(bool ForceColdLeaderboardSync);
