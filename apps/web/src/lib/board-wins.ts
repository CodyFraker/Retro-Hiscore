import type { FriendStandingDto, GameLeaderboardDto, StandingMemberDto } from "@/generated/api-client";

export type BoardWinSummaryRow = {
  memberId: string;
  raUsername: string;
  displayName: string;
  avatarUrl?: string | null;
  friendRankOnes: number;
  boardsWithScore: number;
};

export function summarizeBoardWins(
  leaderboards: GameLeaderboardDto[],
  members: StandingMemberDto[],
): BoardWinSummaryRow[] {
  return members
    .map((member) => {
      let friendRankOnes = 0;
      let boardsWithScore = 0;

      for (const board of leaderboards) {
        const standing = board.standings.find((s) => s.memberId === member.id) as
          | FriendStandingDto
          | undefined;
        if (standing?.score != null) {
          boardsWithScore += 1;
        }
        if (standing?.friendRank === 1) {
          friendRankOnes += 1;
        }
      }

      return {
        memberId: member.id,
        raUsername: member.raUsername,
        displayName: member.displayName,
        avatarUrl: member.avatarUrl,
        friendRankOnes,
        boardsWithScore,
      };
    })
    .sort((a, b) => b.friendRankOnes - a.friendRankOnes || a.displayName.localeCompare(b.displayName));
}

export type RecentlyUpdatedBoard = {
  raLeaderboardId: number;
  title: string;
  latestScoreUpdatedAt: string;
  globalEntryCount: number | null;
};

export function recentlyUpdatedBoards(
  leaderboards: GameLeaderboardDto[],
  limit = 3,
): RecentlyUpdatedBoard[] {
  return leaderboards
    .map((board) => {
      const stamps = board.standings
        .map((s) => s.scoreUpdatedAt)
        .filter((value): value is string => Boolean(value))
        .sort((a, b) => new Date(b).getTime() - new Date(a).getTime());

      return {
        raLeaderboardId: board.raLeaderboardId,
        title: board.title,
        latestScoreUpdatedAt: stamps[0] ?? "",
        globalEntryCount: board.globalEntryCount ?? null,
      };
    })
    .filter((board) => board.latestScoreUpdatedAt)
    .sort(
      (a, b) =>
        new Date(b.latestScoreUpdatedAt).getTime() - new Date(a.latestScoreUpdatedAt).getTime(),
    )
    .slice(0, limit);
}
