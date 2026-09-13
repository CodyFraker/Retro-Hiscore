namespace RetroHiscore.Api.Features.Sync;

public static class FriendRankCalculator
{
    public sealed record RankableScore(Guid MemberId, long Score);

    public static IReadOnlyDictionary<Guid, int> Calculate(IEnumerable<RankableScore> scores, bool rankAsc)
    {
        var ordered = rankAsc
            ? scores.OrderBy(s => s.Score).ThenBy(s => s.MemberId)
            : scores.OrderByDescending(s => s.Score).ThenBy(s => s.MemberId);

        var ranks = new Dictionary<Guid, int>();
        var rank = 0;
        long? previousScore = null;
        var index = 0;

        foreach (var score in ordered)
        {
            index++;
            if (previousScore is null || score.Score != previousScore)
            {
                rank = index;
                previousScore = score.Score;
            }

            ranks[score.MemberId] = rank;
        }

        return ranks;
    }
}
