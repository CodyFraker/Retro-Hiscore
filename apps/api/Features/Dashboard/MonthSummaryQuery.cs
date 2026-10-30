using Microsoft.EntityFrameworkCore;
using RetroHiscore.Api.Data;
using RetroHiscore.Api.Infrastructure;

namespace RetroHiscore.Api.Features.Dashboard;

public static class MonthSummaryQuery
{
    public static async Task<MonthSummaryResponse> LoadAsync(
        AppDbContext db,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var start = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddMonths(1);

        var members = await db.Members.AsNoTracking().OrderBy(m => m.DisplayName).ToListAsync(cancellationToken);
        var memberStats = members.ToDictionary(
            m => m.Id,
            _ => new MemberMonthAccumulator());

        var trackedRaGameIds = await db.Games.AsNoTracking().Select(g => g.RaGameId).ToHashSetAsync(cancellationToken);
        var gameTitles = await db.Games.AsNoTracking().ToDictionaryAsync(g => g.RaGameId, g => g.Title, cancellationToken);
        var gameActivity = new Dictionary<int, GameMonthAccumulator>();
        var groupGameIds = new HashSet<int>();

        await ApplyAchievementActivityAsync(db, start, end, memberStats, gameActivity, groupGameIds, cancellationToken);
        await ApplyRecentPlayActivityAsync(db, start, end, memberStats, gameActivity, groupGameIds, gameTitles, cancellationToken);
        await ApplyLeaderboardSnapshotActivityAsync(db, start, end, memberStats, gameActivity, groupGameIds, cancellationToken);

        var groupTotals = AggregateGroupTotals(memberStats.Values, groupGameIds.Count);

        var memberRows = members
            .Select(m =>
            {
                var stats = memberStats[m.Id];
                return new MonthSummaryMemberRowDto(
                    m.Id,
                    m.RaUsername ?? string.Empty,
                    MemberAuthHelper.DisplayLabel(m),
                    m.AvatarUrl,
                    stats.DistinctGameCount,
                    stats.AchievementsUnlocked,
                    stats.HardcoreUnlocks,
                    stats.HardcorePoints,
                    stats.TruePoints,
                    stats.BoardsTaken,
                    stats.ScoresImproved);
            })
            .OrderByDescending(r => r.AchievementsUnlocked)
            .ThenByDescending(r => r.BoardsTaken)
            .ThenByDescending(r => r.ScoresImproved)
            .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var mostActiveGames = gameActivity.Values
            .Select(g =>
            {
                gameTitles.TryGetValue(g.RaGameId, out var trackedTitle);
                var title = trackedTitle ?? g.FallbackTitle ?? $"Game #{g.RaGameId}";
                return new MonthSummaryGameRowDto(
                    g.RaGameId,
                    title,
                    trackedRaGameIds.Contains(g.RaGameId),
                    g.AchievementsUnlocked,
                    g.ScoreImprovements,
                    g.MemberIds.Count,
                    g.AchievementsUnlocked + g.ScoreImprovements);
            })
            .Where(g => g.ActivityScore > 0)
            .OrderByDescending(g => g.ActivityScore)
            .ThenByDescending(g => g.MemberCount)
            .ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase)
            .Take(15)
            .ToList();

        var availableMonths = await BuildAvailableMonthsAsync(db, cancellationToken);

        return new MonthSummaryResponse(
            year,
            month,
            groupTotals,
            memberRows,
            mostActiveGames,
            availableMonths);
    }

    private static MonthSummaryGroupTotalsDto AggregateGroupTotals(
        IEnumerable<MemberMonthAccumulator> stats,
        int distinctGroupGames)
    {
        var list = stats.ToList();
        return new MonthSummaryGroupTotalsDto(
            distinctGroupGames,
            list.Sum(s => s.AchievementsUnlocked),
            list.Sum(s => s.HardcoreUnlocks),
            list.Sum(s => s.HardcorePoints),
            list.Sum(s => s.TruePoints),
            list.Sum(s => s.BoardsTaken),
            list.Sum(s => s.ScoresImproved));
    }

    private static async Task ApplyAchievementActivityAsync(
        AppDbContext db,
        DateTimeOffset start,
        DateTimeOffset end,
        Dictionary<Guid, MemberMonthAccumulator> memberStats,
        Dictionary<int, GameMonthAccumulator> gameActivity,
        HashSet<int> groupGameIds,
        CancellationToken cancellationToken)
    {
        var unlocks = await db.MemberRaAchievements
            .AsNoTracking()
            .Include(m => m.Achievement)
            .Where(m => m.DateEarned != null && m.DateEarned >= start && m.DateEarned < end)
            .ToListAsync(cancellationToken);

        foreach (var unlock in unlocks)
        {
            if (!memberStats.TryGetValue(unlock.MemberId, out var memberAcc))
            {
                continue;
            }

            var raGameId = unlock.Achievement.RaGameId;
            memberAcc.AddGame(raGameId);
            groupGameIds.Add(raGameId);
            memberAcc.AchievementsUnlocked++;

            var gameAcc = GetOrCreateGame(gameActivity, raGameId, unlock.Achievement.Title);
            gameAcc.AchievementsUnlocked++;
            gameAcc.MemberIds.Add(unlock.MemberId);

            if (unlock.DateEarnedHardcore is { } hardcoreAt && hardcoreAt >= start && hardcoreAt < end)
            {
                memberAcc.HardcoreUnlocks++;
                memberAcc.HardcorePoints += unlock.Achievement.Points;
                memberAcc.TruePoints += unlock.Achievement.TrueRatio;
            }
        }
    }

    private static async Task ApplyRecentPlayActivityAsync(
        AppDbContext db,
        DateTimeOffset start,
        DateTimeOffset end,
        Dictionary<Guid, MemberMonthAccumulator> memberStats,
        Dictionary<int, GameMonthAccumulator> gameActivity,
        HashSet<int> groupGameIds,
        Dictionary<int, string> gameTitles,
        CancellationToken cancellationToken)
    {
        var plays = await db.MemberRecentGamePlays
            .AsNoTracking()
            .Where(p => p.LastPlayedAt >= start && p.LastPlayedAt < end)
            .ToListAsync(cancellationToken);

        foreach (var play in plays)
        {
            if (!memberStats.TryGetValue(play.MemberId, out var memberAcc))
            {
                continue;
            }

            memberAcc.AddGame(play.RaGameId);
            groupGameIds.Add(play.RaGameId);
            var gameAcc = GetOrCreateGame(gameActivity, play.RaGameId, play.Title);
            gameAcc.FallbackTitle ??= play.Title;
            gameAcc.MemberIds.Add(play.MemberId);
            if (gameTitles.ContainsKey(play.RaGameId))
            {
                gameAcc.FallbackTitle = gameTitles[play.RaGameId];
            }
        }
    }

    private static async Task ApplyLeaderboardSnapshotActivityAsync(
        AppDbContext db,
        DateTimeOffset start,
        DateTimeOffset end,
        Dictionary<Guid, MemberMonthAccumulator> memberStats,
        Dictionary<int, GameMonthAccumulator> gameActivity,
        HashSet<int> groupGameIds,
        CancellationToken cancellationToken)
    {
        var boards = await db.Leaderboards
            .AsNoTracking()
            .Include(l => l.Game)
            .ToListAsync(cancellationToken);

        foreach (var board in boards)
        {
            var snapshots = await db.LeaderboardEntrySnapshots
                .AsNoTracking()
                .Where(s => s.LeaderboardId == board.Id && s.SyncedAt < end)
                .OrderBy(s => s.SyncedAt)
                .ThenBy(s => s.MemberId)
                .ToListAsync(cancellationToken);

            foreach (var memberGroup in snapshots.GroupBy(s => s.MemberId))
            {
                if (!memberStats.ContainsKey(memberGroup.Key))
                {
                    continue;
                }

                var points = memberGroup.OrderBy(s => s.SyncedAt).ToList();
                for (var i = 1; i < points.Count; i++)
                {
                    var previous = points[i - 1];
                    var current = points[i];
                    if (current.SyncedAt < start || current.SyncedAt >= end)
                    {
                        continue;
                    }

                    var memberAcc = memberStats[memberGroup.Key];
                    var raGameId = board.Game.RaGameId;
                    var gameAcc = GetOrCreateGame(gameActivity, raGameId, board.Game.Title);

                    if (current.FriendRank == 1 && previous.FriendRank is not 1)
                    {
                        memberAcc.BoardsTaken++;
                        memberAcc.AddGame(raGameId);
                        groupGameIds.Add(raGameId);
                        gameAcc.MemberIds.Add(memberGroup.Key);
                    }

                    if (ScoreImproved(previous.Score, current.Score, board.RankAsc))
                    {
                        memberAcc.ScoresImproved++;
                        memberAcc.AddGame(raGameId);
                        groupGameIds.Add(raGameId);
                        gameAcc.ScoreImprovements++;
                        gameAcc.MemberIds.Add(memberGroup.Key);
                    }
                }
            }
        }
    }

    private static GameMonthAccumulator GetOrCreateGame(
        Dictionary<int, GameMonthAccumulator> gameActivity,
        int raGameId,
        string? titleHint)
    {
        if (!gameActivity.TryGetValue(raGameId, out var acc))
        {
            acc = new GameMonthAccumulator { RaGameId = raGameId, FallbackTitle = titleHint };
            gameActivity[raGameId] = acc;
        }

        return acc;
    }

    internal static bool ScoreImproved(long previousScore, long currentScore, bool rankAsc)
        => rankAsc ? currentScore < previousScore : currentScore > previousScore;

    private static async Task<IReadOnlyList<SeasonMonthDto>> BuildAvailableMonthsAsync(
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var candidates = new List<DateTimeOffset>();

        var achievementMin = await db.MemberRaAchievements
            .AsNoTracking()
            .Where(m => m.DateEarned != null)
            .MinAsync(m => (DateTimeOffset?)m.DateEarned, cancellationToken);
        if (achievementMin is not null)
        {
            candidates.Add(achievementMin.Value);
        }

        var playMin = await db.MemberRecentGamePlays
            .AsNoTracking()
            .MinAsync(p => (DateTimeOffset?)p.LastPlayedAt, cancellationToken);
        if (playMin is not null)
        {
            candidates.Add(playMin.Value);
        }

        var snapshotMin = await db.LeaderboardEntrySnapshots
            .AsNoTracking()
            .MinAsync(s => (DateTimeOffset?)s.SyncedAt, cancellationToken);
        if (snapshotMin is not null)
        {
            candidates.Add(snapshotMin.Value);
        }

        var now = DateTimeOffset.UtcNow;
        if (candidates.Count == 0)
        {
            return [new SeasonMonthDto(now.Year, now.Month)];
        }

        var earliest = candidates.Min();
        var cursor = new DateTimeOffset(earliest.Year, earliest.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var endCursor = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var months = new List<SeasonMonthDto>();

        while (cursor <= endCursor)
        {
            months.Add(new SeasonMonthDto(cursor.Year, cursor.Month));
            cursor = cursor.AddMonths(1);
        }

        return months
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ToList();
    }

    private sealed class MemberMonthAccumulator
    {
        private readonly HashSet<int> _gameIds = [];

        public int AchievementsUnlocked { get; set; }
        public int HardcoreUnlocks { get; set; }
        public int HardcorePoints { get; set; }
        public int TruePoints { get; set; }
        public int BoardsTaken { get; set; }
        public int ScoresImproved { get; set; }

        public int DistinctGameCount => _gameIds.Count;

        public void AddGame(int raGameId) => _gameIds.Add(raGameId);
    }

    private sealed class GameMonthAccumulator
    {
        public int RaGameId { get; init; }
        public string? FallbackTitle { get; set; }
        public int AchievementsUnlocked { get; set; }
        public int ScoreImprovements { get; set; }
        public HashSet<Guid> MemberIds { get; } = [];
    }
}

public sealed record MonthSummaryGroupTotalsDto(
    int GamesPlayed,
    int AchievementsUnlocked,
    int HardcoreUnlocks,
    int HardcorePoints,
    int TruePoints,
    int BoardsTaken,
    int ScoresImproved);

public sealed record MonthSummaryMemberRowDto(
    Guid MemberId,
    string RaUsername,
    string DisplayName,
    string? AvatarUrl,
    int GamesPlayed,
    int AchievementsUnlocked,
    int HardcoreUnlocks,
    int HardcorePoints,
    int TruePoints,
    int BoardsTaken,
    int ScoresImproved);

public sealed record MonthSummaryGameRowDto(
    int RaGameId,
    string Title,
    bool IsTracked,
    int AchievementsUnlocked,
    int ScoreImprovements,
    int MemberCount,
    int ActivityScore);

public sealed record MonthSummaryResponse(
    int Year,
    int Month,
    MonthSummaryGroupTotalsDto GroupTotals,
    IReadOnlyList<MonthSummaryMemberRowDto> MemberRows,
    IReadOnlyList<MonthSummaryGameRowDto> MostActiveGames,
    IReadOnlyList<SeasonMonthDto> AvailableMonths);
