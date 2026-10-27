namespace RetroHiscore.Api.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public string? JwtSigningKey { get; set; }

    public string JwtIssuer { get; set; } = "retro-hiscore-web";

    public string JwtAudience { get; set; } = "retro-hiscore-api";

    public string WebOrigin { get; set; } = "http://localhost:18321";

    public string? SignInServiceKey { get; set; }

    public List<string> AdminDiscordUserIds { get; set; } = [];
}
