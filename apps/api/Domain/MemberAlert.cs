namespace RetroHiscore.Api.Domain;

public class MemberAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipientMemberId { get; set; }
    public Member RecipientMember { get; set; } = null!;
    public MemberAlertKind Kind { get; set; }
    public Guid? ActorMemberId { get; set; }
    public Member? ActorMember { get; set; }
    public int? RaGameId { get; set; }
    public string? GameTitle { get; set; }
    public long? RaLeaderboardId { get; set; }
    public string? LeaderboardTitle { get; set; }
    public string? AchievementTitle { get; set; }
    public int? AchievementPoints { get; set; }
    public bool? Hardcore { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
