namespace RetroHiscore.Api.Domain;

public class MemberRaAchievement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int RaAchievementId { get; set; }
    public RaAchievement Achievement { get; set; } = null!;
    public DateTimeOffset? DateEarned { get; set; }
    public DateTimeOffset? DateEarnedHardcore { get; set; }
    public DateTimeOffset FirstDetectedAt { get; set; }
}
