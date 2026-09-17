namespace RetroHiscore.Api.Domain;

public class GameTrackQueueRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public int RaGameId { get; set; }
    public Guid? GameTrackQueueId { get; set; }
    public GameTrackQueue? GameTrackQueue { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
