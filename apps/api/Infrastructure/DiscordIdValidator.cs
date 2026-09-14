namespace RetroHiscore.Api.Infrastructure;

public static class DiscordIdValidator
{
    public static bool IsValid(string? discordId)
    {
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return false;
        }

        var trimmed = discordId.Trim();
        if (trimmed.Length > 32)
        {
            return false;
        }

        return trimmed.All(char.IsAsciiDigit);
    }

    public static string Normalize(string discordId) => discordId.Trim();
}
