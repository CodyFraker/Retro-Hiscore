namespace RetroHiscore.Api.Features.Games;

public sealed record GameTrackRequestSubmitResponse(
    string Code,
    int RaGameId,
    string Title,
    string? Message = null,
    DateTimeOffset? NextSlotAt = null);

public sealed record GameTrackRequestQuotaDto(
    int Limit,
    int Used,
    int Remaining,
    DateTimeOffset? NextSlotAt);
