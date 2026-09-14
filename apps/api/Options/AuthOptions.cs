namespace RetroHiscore.Api.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public string? JwtSigningKey { get; set; }

    public string WebOrigin { get; set; } = "http://localhost:18321";

    public List<string> AllowedDiscordUserIds { get; set; } = [];

    public List<string> AdminDiscordUserIds { get; set; } = [];
}
