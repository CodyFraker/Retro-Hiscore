import type { GameHistoryItemDto } from "@/generated/api-client";
import { computeMemberDeltas, toChartSeries, type MemberSeries } from "@/lib/history-series";

export type MemberLeadPoint = {
  syncedAt: string;
  friendRankOnes: number;
};

export type MemberLeadSeries = {
  memberId: string;
  displayName: string;
  points: MemberLeadPoint[];
};

export type BoardRankSeries = {
  raLeaderboardId: number;
  leaderboardTitle: string;
  memberId: string;
  displayName: string;
  points: { syncedAt: string; friendRank: number | null }[];
};

export type BoardScoreSeries = {
  raLeaderboardId: number;
  leaderboardTitle: string;
  series: MemberSeries[];
};

export type GameDelta = {
  memberId: string;
  displayName: string;
  raLeaderboardId: number;
  leaderboardTitle: string;
  scoreDelta: number | null;
  friendRankDelta: number | null;
};

export function toMemberLeadSeries(items: GameHistoryItemDto[]): MemberLeadSeries[] {
  const byMember = new Map<string, MemberLeadSeries>();
  const syncStamps = [...new Set(items.map((item) => item.syncedAt))].sort(
    (a, b) => new Date(a).getTime() - new Date(b).getTime(),
  );

  for (const item of items) {
    let series = byMember.get(item.memberId);
    if (!series) {
      series = {
        memberId: item.memberId,
        displayName: item.displayName,
        points: [],
      };
      byMember.set(item.memberId, series);
    }
  }

  for (const syncedAt of syncStamps) {
    const atSync = items.filter((item) => item.syncedAt === syncedAt);
    const leadsByMember = new Map<string, number>();

    for (const item of atSync) {
      const current = leadsByMember.get(item.memberId) ?? 0;
      if (item.friendRank === 1) {
        leadsByMember.set(item.memberId, current + 1);
      } else {
        leadsByMember.set(item.memberId, current);
      }
    }

    for (const series of byMember.values()) {
      series.points.push({
        syncedAt,
        friendRankOnes: leadsByMember.get(series.memberId) ?? 0,
      });
    }
  }

  return [...byMember.values()].sort((a, b) => a.displayName.localeCompare(b.displayName));
}

export function toBoardRankSeries(items: GameHistoryItemDto[]): BoardRankSeries[] {
  const byBoardMember = new Map<string, BoardRankSeries>();

  for (const item of items) {
    const key = `${item.raLeaderboardId}:${item.memberId}`;
    let series = byBoardMember.get(key);
    if (!series) {
      series = {
        raLeaderboardId: item.raLeaderboardId,
        leaderboardTitle: item.leaderboardTitle,
        memberId: item.memberId,
        displayName: item.displayName,
        points: [],
      };
      byBoardMember.set(key, series);
    }

    series.points.push({
      syncedAt: item.syncedAt,
      friendRank: item.friendRank ?? null,
    });
  }

  return [...byBoardMember.values()]
    .map((series) => ({
      ...series,
      points: dedupeBoardRankPoints(series.points),
    }))
    .filter((series) => series.points.some((point) => point.friendRank != null))
    .sort(
      (a, b) =>
        a.leaderboardTitle.localeCompare(b.leaderboardTitle) ||
        a.displayName.localeCompare(b.displayName),
    );
}

function dedupeBoardRankPoints(
  points: { syncedAt: string; friendRank: number | null }[],
): { syncedAt: string; friendRank: number | null }[] {
  const bySync = new Map<string, { syncedAt: string; friendRank: number | null }>();
  for (const point of points) {
    bySync.set(point.syncedAt, point);
  }
  return [...bySync.values()].sort(
    (a, b) => new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime(),
  );
}

export function toBoardScoreSeries(items: GameHistoryItemDto[]): BoardScoreSeries[] {
  const byBoard = new Map<number, GameHistoryItemDto[]>();

  for (const item of items) {
    const boardItems = byBoard.get(item.raLeaderboardId) ?? [];
    boardItems.push(item);
    byBoard.set(item.raLeaderboardId, boardItems);
  }

  return [...byBoard.entries()]
    .map(([raLeaderboardId, boardItems]) => ({
      raLeaderboardId,
      leaderboardTitle: boardItems[0]?.leaderboardTitle ?? "",
      series: toChartSeries(
        boardItems.map((item) => ({
          id: item.id,
          memberId: item.memberId,
          raUsername: item.raUsername,
          displayName: item.displayName,
          score: item.score,
          formattedScore: item.formattedScore,
          globalRank: item.globalRank,
          friendRank: item.friendRank,
          syncedAt: item.syncedAt,
        })),
      ),
    }))
    .sort((a, b) => a.leaderboardTitle.localeCompare(b.leaderboardTitle));
}

export function toGameDeltas(items: GameHistoryItemDto[]): GameDelta[] {
  const boardSeries = toBoardScoreSeries(items);
  const deltas: GameDelta[] = [];

  for (const board of boardSeries) {
    const memberDeltas = computeMemberDeltas(board.series);
    for (const delta of memberDeltas) {
      if (delta.scoreDelta == null && delta.friendRankDelta == null) {
        continue;
      }

      deltas.push({
        memberId: delta.memberId,
        displayName: delta.displayName,
        raLeaderboardId: board.raLeaderboardId,
        leaderboardTitle: board.leaderboardTitle,
        scoreDelta: delta.scoreDelta,
        friendRankDelta: delta.friendRankDelta,
      });
    }
  }

  return deltas.sort(
    (a, b) =>
      a.leaderboardTitle.localeCompare(b.leaderboardTitle) ||
      a.displayName.localeCompare(b.displayName),
  );
}

export function countDistinctSyncTimestampsForMemberLeads(series: MemberLeadSeries[]): number {
  const stamps = new Set<string>();
  for (const member of series) {
    for (const point of member.points) {
      stamps.add(point.syncedAt);
    }
  }
  return stamps.size;
}

export function countDistinctSyncTimestampsForBoardRanks(series: BoardRankSeries[]): number {
  const stamps = new Set<string>();
  for (const board of series) {
    for (const point of board.points) {
      stamps.add(point.syncedAt);
    }
  }
  return stamps.size;
}
