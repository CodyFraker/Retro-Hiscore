namespace RetroHiscore.Api.Domain;

public class Member
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string RaUsername { get; set; }
    public string? RaUlid { get; set; }
    public string? DisplayName { get; set; }
    public ICollection<LeaderboardEntry> Entries { get; set; } = new List<LeaderboardEntry>();
}
