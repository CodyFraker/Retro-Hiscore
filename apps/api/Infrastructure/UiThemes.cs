namespace RetroHiscore.Api.Infrastructure;

public static class UiThemes
{
    public const string Steam = "steam";
    public const string FrutigerAero = "frutiger-aero";

    private static readonly HashSet<string> Known = new(StringComparer.Ordinal)
    {
        Steam,
        FrutigerAero,
    };

    public static bool IsValid(string? value)
        => !string.IsNullOrWhiteSpace(value) && Known.Contains(value.Trim());

    public static string Normalize(string? value)
        => IsValid(value) ? value!.Trim() : Steam;
}
