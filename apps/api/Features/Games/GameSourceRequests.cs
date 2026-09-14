namespace RetroHiscore.Api.Features.Games;

public sealed record UpsertGameSourceRequest(
    string SourceType,
    string Url,
    string? Label,
    int SortOrder,
    string? Note);
