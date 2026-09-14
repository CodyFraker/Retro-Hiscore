namespace RetroHiscore.Api.Domain;

public class MemberRaRankSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int? Rank { get; set; }
    public int? TotalRanked { get; set; }
    public int? TotalPoints { get; set; }
    public int? TotalTruePoints { get; set; }
    public int? TotalSoftcorePoints { get; set; }
    public DateTimeOffset SyncedAt { get; set; }
}
