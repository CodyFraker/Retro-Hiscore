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
}
