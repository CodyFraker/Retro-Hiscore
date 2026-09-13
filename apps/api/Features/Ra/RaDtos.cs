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
