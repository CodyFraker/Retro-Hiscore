using System.Text.Json;
using System.Text.Json.Serialization;

namespace RetroHiscore.Api.Features.Ra;

public sealed class RaConsoleIdDto
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("IconURL")]
    public string? IconUrl { get; set; }
}

public sealed class RaGameDto
{
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string? ConsoleName { get; set; }

    [JsonPropertyName("ImageIcon")]
    public string? ImageIcon { get; set; }

    [JsonPropertyName("ImageTitle")]
    public string? ImageTitle { get; set; }

    [JsonPropertyName("ImageIngame")]
    public string? ImageIngame { get; set; }

    [JsonPropertyName("ImageBoxArt")]
    public string? ImageBoxArt { get; set; }

    [JsonPropertyName("Publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("Developer")]
    public string? Developer { get; set; }

    [JsonPropertyName("Genre")]
    public string? Genre { get; set; }

    [JsonPropertyName("Released")]
    public string? Released { get; set; }
}

public sealed class RaPagedResponse<T>
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Total")]
    public int Total { get; set; }

    [JsonPropertyName("Results")]
    public List<T> Results { get; set; } = [];
}

public sealed class RaGameLeaderboardDto
{
    [JsonPropertyName("ID")]
    public long Id { get; set; }

    [JsonPropertyName("RankAsc")]
    public bool RankAsc { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Format")]
    public string? Format { get; set; }
}

public sealed class RaUserGameLeaderboardDto
{
    [JsonPropertyName("ID")]
    public long Id { get; set; }

    [JsonPropertyName("RankAsc")]
    public bool RankAsc { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Format")]
    public string? Format { get; set; }

    [JsonPropertyName("UserEntry")]
    public RaUserEntryDto? UserEntry { get; set; }
}

public sealed class RaUserSummaryDto
{
    [JsonPropertyName("User")]
    public string User { get; set; } = string.Empty;

    [JsonPropertyName("ULID")]
    public string? Ulid { get; set; }

    [JsonPropertyName("Motto")]
    public JsonElement Motto { get; set; }

    [JsonPropertyName("UserPic")]
    public string? UserPic { get; set; }

    [JsonPropertyName("MemberSince")]
    public string? MemberSince { get; set; }

    [JsonPropertyName("Status")]
    public JsonElement Status { get; set; }

    [JsonPropertyName("Rank")]
    public int? Rank { get; set; }

    [JsonPropertyName("TotalRanked")]
    public int? TotalRanked { get; set; }

    [JsonPropertyName("TotalPoints")]
    public int? TotalPoints { get; set; }

    [JsonPropertyName("TotalSoftcorePoints")]
    public int? TotalSoftcorePoints { get; set; }

    [JsonPropertyName("TotalTruePoints")]
    public int? TotalTruePoints { get; set; }

    [JsonPropertyName("RichPresenceMsg")]
    public string? RichPresenceMsg { get; set; }

    [JsonPropertyName("RichPresenceMsgDate")]
    public string? RichPresenceMsgDate { get; set; }

    [JsonPropertyName("LastGameID")]
    public int? LastGameId { get; set; }

    [JsonPropertyName("LastGame")]
    public RaUserSummaryGameDto? LastGame { get; set; }

    [JsonPropertyName("RecentlyPlayed")]
    public List<RaUserSummaryRecentGameDto>? RecentlyPlayed { get; set; }

    [JsonPropertyName("Awarded")]
    public Dictionary<string, RaUserSummaryAwardedDto>? Awarded { get; set; }

    [JsonPropertyName("RecentAchievements")]
    public Dictionary<string, Dictionary<string, RaUserSummaryRecentAchievementDto>>? RecentAchievements { get; set; }
}

public sealed class RaUserSummaryGameDto
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string? ConsoleName { get; set; }

    [JsonPropertyName("ImageIcon")]
    public string? ImageIcon { get; set; }

    [JsonPropertyName("ImageTitle")]
    public string? ImageTitle { get; set; }

    [JsonPropertyName("ImageIngame")]
    public string? ImageIngame { get; set; }

    [JsonPropertyName("ImageBoxArt")]
    public string? ImageBoxArt { get; set; }
}

public sealed class RaUserSummaryRecentGameDto
{
    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string? ConsoleName { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("ImageIcon")]
    public string? ImageIcon { get; set; }

    [JsonPropertyName("ImageBoxArt")]
    public string? ImageBoxArt { get; set; }

    [JsonPropertyName("LastPlayed")]
    public string? LastPlayed { get; set; }

    [JsonPropertyName("AchievementsTotal")]
    public int? AchievementsTotal { get; set; }
}

public sealed class RaUserRecentlyPlayedGameDto
{
    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string? ConsoleName { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("ImageIcon")]
    public string? ImageIcon { get; set; }

    [JsonPropertyName("ImageBoxArt")]
    public string? ImageBoxArt { get; set; }

    [JsonPropertyName("LastPlayed")]
    public string? LastPlayed { get; set; }

    [JsonPropertyName("NumAchieved")]
    public int? NumAchieved { get; set; }

    [JsonPropertyName("NumPossibleAchievements")]
    public int? NumPossibleAchievements { get; set; }

    [JsonPropertyName("AchievementsTotal")]
    public int? AchievementsTotal { get; set; }
}

public sealed class RaUserSummaryAwardedDto
{
    [JsonPropertyName("NumPossibleAchievements")]
    public int? NumPossibleAchievements { get; set; }

    [JsonPropertyName("PossibleScore")]
    public int? PossibleScore { get; set; }

    [JsonPropertyName("NumAchieved")]
    public int? NumAchieved { get; set; }

    [JsonPropertyName("ScoreAchieved")]
    public int? ScoreAchieved { get; set; }

    [JsonPropertyName("NumAchievedHardcore")]
    public int? NumAchievedHardcore { get; set; }

    [JsonPropertyName("ScoreAchievedHardcore")]
    public int? ScoreAchievedHardcore { get; set; }
}

public sealed class RaUserSummaryRecentAchievementDto
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("GameTitle")]
    public string GameTitle { get; set; } = string.Empty;

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Points")]
    public int Points { get; set; }

    [JsonPropertyName("BadgeName")]
    public string? BadgeName { get; set; }

    [JsonPropertyName("DateAwarded")]
    public string? DateAwarded { get; set; }

    [JsonPropertyName("HardcoreAchieved")]
    public int? HardcoreAchieved { get; set; }
}

public sealed class RaGameInfoAndUserProgressDto
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Achievements")]
    public Dictionary<string, RaGameAchievementProgressDto>? Achievements { get; set; }
}

public sealed class RaGameAchievementProgressDto
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Points")]
    public int Points { get; set; }

    [JsonPropertyName("TrueRatio")]
    public int TrueRatio { get; set; }

    [JsonPropertyName("BadgeName")]
    public string? BadgeName { get; set; }

    [JsonPropertyName("DisplayOrder")]
    public int DisplayOrder { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("DateEarned")]
    public string? DateEarned { get; set; }

    [JsonPropertyName("DateEarnedHardcore")]
    public string? DateEarnedHardcore { get; set; }
}

public sealed class RaLeaderboardEntryDto
{
    [JsonPropertyName("Rank")]
    public int Rank { get; set; }

    [JsonPropertyName("User")]
    public string User { get; set; } = string.Empty;
}

public sealed class RaUserEntryDto
{
    [JsonPropertyName("User")]
    public string User { get; set; } = string.Empty;

    [JsonPropertyName("ULID")]
    public string? Ulid { get; set; }

    [JsonPropertyName("Score")]
    public long Score { get; set; }

    [JsonPropertyName("FormattedScore")]
    public string FormattedScore { get; set; } = string.Empty;

    [JsonPropertyName("Rank")]
    public int Rank { get; set; }

    [JsonPropertyName("DateUpdated")]
    public DateTimeOffset? DateUpdated { get; set; }
}
