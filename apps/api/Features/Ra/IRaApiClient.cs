namespace RetroHiscore.Api.Features.Ra;

public interface IRaApiClient
{
    Task<RaGameDto> GetGameAsync(int gameId, string? apiKey = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RaConsoleIdDto>> GetConsoleIdsAsync(string? apiKey = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RaGameLeaderboardDto>> GetGameLeaderboardsAsync(int gameId, string? apiKey = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RaUserGameLeaderboardDto>> GetUserGameLeaderboardsAsync(
        int gameId,
        string usernameOrUlid,
        string? apiKey = null,
        CancellationToken cancellationToken = default);

    Task<int?> GetLeaderboardEntryCountAsync(
        long leaderboardId,
        string? apiKey = null,
        CancellationToken cancellationToken = default);

    Task<RaUserSummaryDto?> GetUserSummaryAsync(
        string username,
        string apiKey,
        int recentGamesCount = 3,
        int recentAchievementsCount = 8,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RaUserRecentlyPlayedGameDto>> GetUserRecentlyPlayedGamesAsync(
        string usernameOrUlid,
        int count = 15,
        int offset = 0,
        string? apiKey = null,
        CancellationToken cancellationToken = default);

    Task<RaGameInfoAndUserProgressDto?> GetGameInfoAndUserProgressAsync(
        int gameId,
        string usernameOrUlid,
        string apiKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, int>> GetAchievementDistributionAsync(
        int gameId,
        bool hardcore,
        string? apiKey = null,
        CancellationToken cancellationToken = default);
}
