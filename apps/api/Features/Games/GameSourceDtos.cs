using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.Games;

public sealed record GameSourceDto(
    Guid Id,
    string SourceType,
    string Url,
    string? Label,
    int SortOrder,
    string? Note);

public static class GameSourceMapping
{
    public static GameSourceDto ToDto(GameSource source)
        => new(
            source.Id,
            source.SourceType.ToString(),
            source.Url,
            source.Label,
            source.SortOrder,
            source.Note);

    public static IReadOnlyList<GameSourceDto> ToDtos(IEnumerable<GameSource> sources)
        => sources
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .Select(ToDto)
            .ToList();
}
