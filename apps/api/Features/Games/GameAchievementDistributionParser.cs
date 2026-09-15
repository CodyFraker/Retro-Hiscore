using System.Text.Json;

namespace RetroHiscore.Api.Features.Games;

internal static class GameAchievementDistributionParser
{
    public static IReadOnlyList<AchievementDistributionBucketDto> ParseBuckets(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
            if (raw is null || raw.Count == 0)
            {
                return [];
            }

            return raw
                .Select(kv => new
                {
                    Earned = int.TryParse(kv.Key, out var n) ? n : -1,
                    kv.Value
                })
                .Where(x => x.Earned >= 0)
                .OrderBy(x => x.Earned)
                .Select(x => new AchievementDistributionBucketDto(x.Earned, x.Value))
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
