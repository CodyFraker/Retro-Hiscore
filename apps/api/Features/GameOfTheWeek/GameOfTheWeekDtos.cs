using RetroHiscore.Api.Domain;

namespace RetroHiscore.Api.Features.GameOfTheWeek;

public sealed record GameOfTheWeekVoteCastDto(
    Guid MemberId,
    string DisplayName,
    DateTimeOffset CastAt);

public sealed record GameOfTheWeekBallotItemDto(
    int RaGameId,
    string Title,
    string? ConsoleName,
    string? ImageIcon,
    int SortOrder,
    bool IsTracked,
    int VoteCount,
    Guid? AddedByMemberId,
    string? AddedByDisplayName,
    IReadOnlyList<GameOfTheWeekVoteCastDto> Voters);

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
    int BallotSlotsRemaining,
    int EligibleVoterCount,
    int VotesCastCount,
    bool AllEligibleVotesCast);

public sealed record PostGameOfTheWeekPollRequest(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    IReadOnlyList<int> RaGameIds);

public sealed record GameOfTheWeekRaGameIdRequest(int RaGameId);

public sealed record GameOfTheWeekHistoryItemDto(
    Guid PollId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    DateTimeOffset? ClosedAt,
    int? WinnerRaGameId,
    string? WinnerTitle,
    string? WinnerConsoleName,
    string? WinnerImageIcon,
    bool IsTracked,
    int TotalVotes);

public sealed record GameOfTheWeekHistoryResponse(
    int Total,
    int Offset,
    int Limit,
    IReadOnlyList<GameOfTheWeekHistoryItemDto> Items);
