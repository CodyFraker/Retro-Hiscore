namespace RetroHiscore.Api.Features.GameOfTheWeek;

public sealed class GameOfTheWeekPollJob(
    IGameOfTheWeekCloseService closeService,
    IGameOfTheWeekWinnerTrackingService winnerTrackingService)
{
    public async Task RunScheduledAsync(CancellationToken cancellationToken = default)
    {
        await closeService.CloseDuePollsAsync(cancellationToken);
        await winnerTrackingService.ProcessPendingWinnersAsync(cancellationToken);
    }
}
