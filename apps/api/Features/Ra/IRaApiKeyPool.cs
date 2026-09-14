namespace RetroHiscore.Api.Features.Ra;

public interface IRaApiKeyPool
{
    Task<IReadOnlyList<string>> GetMemberKeysAsync(CancellationToken cancellationToken = default);

    Task<T> ExecuteAsync<T>(
        IEnumerable<string> preferredKeys,
        Func<string, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}
