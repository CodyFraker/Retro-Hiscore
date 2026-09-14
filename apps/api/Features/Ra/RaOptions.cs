namespace RetroHiscore.Api.Features.Ra;

public class RaOptions
{
    public const string SectionName = "RA";

    public string Username { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://retroachievements.org/API";
    public string MediaBaseUrl { get; set; } = "https://media.retroachievements.org";
    public List<int> TrackedGameIds { get; set; } = [];
}
