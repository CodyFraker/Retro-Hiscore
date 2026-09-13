namespace RetroHiscore.Api.Features.Ra;

public interface IRaApiClient
{
    Task<RaGameDto> GetGameAsync(int gameId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RaConsoleIdDto>> GetConsoleIdsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RaGameLeaderboardDto>> GetGameLeaderboardsAsync(int gameId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RaUserGameLeaderboardDto>> GetUserGameLeaderboardsAsync(int gameId, string usernameOrUlid, CancellationToken cancellationToken = default);
}
