namespace RetroHiscore.Api.Domain;

public class MemberGamePin
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;
    public DateTimeOffset PinnedAt { get; set; }
}
