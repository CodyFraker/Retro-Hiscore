import type { GameLeaderboardDto } from "@/generated/api-client";

export function filterLeaderboardsByQuery(
  boards: GameLeaderboardDto[],
  query: string,
): GameLeaderboardDto[] {
  const trimmed = query.trim().toLowerCase();
  if (!trimmed) {
    return boards;
  }

  return boards.filter((board) => {
    const title = board.title.toLowerCase();
    const description = board.description?.toLowerCase() ?? "";
    return title.includes(trimmed) || description.includes(trimmed);
  });
}

export function filterLeaderboardsWithFriendScores(
  boards: GameLeaderboardDto[],
  memberIds: string[],
): GameLeaderboardDto[] {
  if (memberIds.length === 0) {
    return boards;
  }

  const idSet = new Set(memberIds);
  return boards.filter((board) =>
    board.standings.some(
      (standing) => idSet.has(standing.memberId) && standing.score != null,
    ),
  );
}

export function applyLeaderboardFilters(
  boards: GameLeaderboardDto[],
  options: {
    query: string;
    friendScoresOnly: boolean;
    memberIds: string[];
  },
): GameLeaderboardDto[] {
  let result = boards;
  if (options.friendScoresOnly) {
    result = filterLeaderboardsWithFriendScores(result, options.memberIds);
  }
  result = filterLeaderboardsByQuery(result, options.query);
  return result;
}
