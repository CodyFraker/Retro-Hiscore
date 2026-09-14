namespace RetroHiscore.Api.Features.Sync;

public interface IRecurringSyncJobRegistrar
{
    Task RegisterAllAsync(CancellationToken cancellationToken = default);
}
