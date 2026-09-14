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
    DateTimeOffset? MetadataSyncedAt);

public sealed record PostAdminGameRequest(int RaGameId);
