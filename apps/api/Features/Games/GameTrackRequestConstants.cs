namespace RetroHiscore.Api.Features.Games;

public static class GameTrackRequestConstants
{
    public const int MaxPerRollingWindow = 5;
    public static readonly TimeSpan RollingWindow = TimeSpan.FromHours(24);
}
