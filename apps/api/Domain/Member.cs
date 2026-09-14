namespace RetroHiscore.Api.Domain;

public class Member
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? RaUsername { get; set; }
    public bool IsAdmin { get; set; }
    public string? RaUlid { get; set; }
    public string? DisplayName { get; set; }
    public string? DiscordId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? RaApiKey { get; set; }
    public ICollection<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
}
