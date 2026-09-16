using System.Text.Json;
using System.Text.RegularExpressions;
using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Notifications;

public interface IDiscordWebhookTemplateRenderer
{
    string Render(string payloadTemplateJson, DiscordNotificationEventKind eventKind, string eventPayloadJson);
    IReadOnlyList<string> FindUnknownTokens(string payloadTemplateJson, IReadOnlySet<string> allowedCodes);
}

public sealed class DiscordWebhookTemplateValidationException(string message) : Exception(message);

public sealed partial class DiscordWebhookTemplateRenderer : IDiscordWebhookTemplateRenderer
{
    [GeneratedRegex(@"\{\{([a-z]{2,3})\}\}", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();

    public string Render(string payloadTemplateJson, DiscordNotificationEventKind eventKind, string eventPayloadJson)
    {
        var values = DiscordTokenCatalog.BuildValues(eventKind, eventPayloadJson);
        using var doc = JsonDocument.Parse(payloadTemplateJson);
        var rendered = RenderElement(doc.RootElement, values);
        return JsonSerializer.Serialize(rendered);
    }

    public IReadOnlyList<string> FindUnknownTokens(string payloadTemplateJson, IReadOnlySet<string> allowedCodes)
    {
        var unknown = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in TokenRegex().Matches(payloadTemplateJson))
        {
            var code = match.Groups[1].Value;
            if (!allowedCodes.Contains(code))
            {
                unknown.Add(code);
            }
        }

        return unknown.OrderBy(c => c).ToList();
    }

    private static object? RenderElement(JsonElement element, IReadOnlyDictionary<string, string> values)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => RenderElement(p.Value, values)),
            JsonValueKind.Array => element.EnumerateArray().Select(e => RenderElement(e, values)).ToList(),
            JsonValueKind.String => ReplaceTokens(element.GetString() ?? "", values),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    private static string ReplaceTokens(string input, IReadOnlyDictionary<string, string> values)
        => TokenRegex().Replace(input, match =>
        {
            var code = match.Groups[1].Value;
            return values.TryGetValue(code, out var value) ? value : match.Value;
        });
}
