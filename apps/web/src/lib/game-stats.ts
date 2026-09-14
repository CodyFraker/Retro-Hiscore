import type { GameLeaderboardsResponse } from "@/generated/api-client";
import { summarizeBoardWins } from "@/lib/board-wins";

export type GameStats = {
  leaderboardCount: number;
  membersWithScores: number;
  totalScores: number;
  totalRankedEntriesAcrossBoards: number;
  busiestBoard: {
    raLeaderboardId: number;
    title: string;
    globalEntryCount: number;
  } | null;
  lastActivityAt: string | null;
  currentLeader: {
    memberId: string;
    displayName: string;
    friendRankOnes: number;
  } | null;
};

export function summarizeGameStats(data: GameLeaderboardsResponse): GameStats {
  const winRows = summarizeBoardWins(data.leaderboards, data.members);
  const scoredMemberIds = new Set<string>();

  let totalScores = 0;
  let lastActivityAt: string | null = null;

  for (const board of data.leaderboards) {
    for (const standing of board.standings) {
      if (standing.score != null) {
        totalScores += 1;
        scoredMemberIds.add(standing.memberId);
      }
      if (standing.scoreUpdatedAt) {
        if (
          lastActivityAt == null ||
          new Date(standing.scoreUpdatedAt).getTime() > new Date(lastActivityAt).getTime()
        ) {
          lastActivityAt = standing.scoreUpdatedAt;
        }
      }
    }
  }

  const topWinner = winRows.find((row) => row.friendRankOnes > 0);

  let totalRankedEntriesAcrossBoards = 0;
  let busiestBoard: GameStats["busiestBoard"] = null;

  for (const board of data.leaderboards) {
    if (board.globalEntryCount != null) {
      totalRankedEntriesAcrossBoards += board.globalEntryCount;
      if (
        busiestBoard == null ||
        board.globalEntryCount > busiestBoard.globalEntryCount
      ) {
        busiestBoard = {
          raLeaderboardId: board.raLeaderboardId,
          title: board.title,
          globalEntryCount: board.globalEntryCount,
        };
      }
    }
  }

  return {
    leaderboardCount: data.leaderboards.length,
    membersWithScores: scoredMemberIds.size,
    totalScores,
    totalRankedEntriesAcrossBoards,
    busiestBoard,
    lastActivityAt,
    currentLeader: topWinner
      ? {
          memberId: topWinner.memberId,
          displayName: topWinner.displayName,
          friendRankOnes: topWinner.friendRankOnes,
        }
      : null,
  };
}
