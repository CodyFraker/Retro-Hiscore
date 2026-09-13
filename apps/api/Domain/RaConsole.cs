namespace RetroHiscore.Api.Domain;

public class RaConsole
{
    public int RaConsoleId { get; set; }
    public required string Name { get; set; }
    public string? IconFileName { get; set; }
    public DateTimeOffset? IconSyncedAt { get; set; }
}
