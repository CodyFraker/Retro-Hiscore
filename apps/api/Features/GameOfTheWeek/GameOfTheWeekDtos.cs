using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public sealed record GameOfTheWeekBallotItemDto(
    int RaGameId,
    string Title,
    string? ConsoleName,
    string? ImageIcon,
    int SortOrder,
    bool IsTracked,
    int VoteCount,
    Guid? AddedByMemberId);

public sealed record GameOfTheWeekCurrentPollDto(
    Guid PollId,
    GameOfTheWeekPhase Phase,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    DateTimeOffset? ClosedAt,
    int? WinnerRaGameId,
    GameOfTheWeekTrackingStatus TrackingStatus,
    int? MyVoteRaGameId,
    IReadOnlyList<GameOfTheWeekBallotItemDto> Ballot,
    int BallotSlotsRemaining);

public sealed record PostGameOfTheWeekPollRequest(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    IReadOnlyList<int> RaGameIds);

public sealed record GameOfTheWeekRaGameIdRequest(int RaGameId);
