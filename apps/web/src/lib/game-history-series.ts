import type { GameHistoryItemDto } from "@/generated/api-client";
import { computeMemberDeltas, toChartSeries, type MemberSeries } from "@/lib/history-series";

export type MemberLeadPoint = {
  syncedAt: string;
  friendRankOnes: number;
};

export type MemberLeadSeries = {
  memberId: string;
  displayName: string;
  avatarUrl?: string | null;
  points: MemberLeadPoint[];
};

export type BoardRankSeries = {
  raLeaderboardId: number;
  leaderboardTitle: string;
  memberId: string;
  displayName: string;
  avatarUrl?: string | null;
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
  globalRankDelta: number | null;
  globalEntryCount: number | null;
  globalEntryCountDelta: number | null;
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
        avatarUrl: item.avatarUrl,
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
        avatarUrl: item.avatarUrl,
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
          avatarUrl: item.avatarUrl,
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
      if (
        delta.scoreDelta == null &&
        delta.friendRankDelta == null &&
        delta.globalRankDelta == null
      ) {
        continue;
      }

      const populationDelta = computeBoardPopulationDelta(board.raLeaderboardId, items);

      deltas.push({
        memberId: delta.memberId,
        displayName: delta.displayName,
        raLeaderboardId: board.raLeaderboardId,
        leaderboardTitle: board.leaderboardTitle,
        scoreDelta: delta.scoreDelta,
        friendRankDelta: delta.friendRankDelta,
        globalRankDelta: delta.globalRankDelta,
        globalEntryCount: populationDelta?.currentCount ?? null,
        globalEntryCountDelta: populationDelta?.delta ?? null,
      });
    }
  }

  return deltas.sort(
    (a, b) =>
      a.leaderboardTitle.localeCompare(b.leaderboardTitle) ||
      a.displayName.localeCompare(b.displayName),
  );
}

function computeBoardPopulationDelta(
  raLeaderboardId: number,
  items: GameHistoryItemDto[],
): { currentCount: number; delta: number } | null {
  const boardItems = items.filter((item) => item.raLeaderboardId === raLeaderboardId);
  const bySync = new Map<string, number>();

  for (const item of boardItems) {
    if (item.globalEntryCount != null && !bySync.has(item.syncedAt)) {
      bySync.set(item.syncedAt, item.globalEntryCount);
    }
  }

  const stamps = [...bySync.keys()].sort(
    (a, b) => new Date(a).getTime() - new Date(b).getTime(),
  );

  if (stamps.length === 0) {
    return null;
  }

  const currentCount = bySync.get(stamps[stamps.length - 1])!;
  if (stamps.length < 2) {
    return { currentCount, delta: 0 };
  }

  const previousCount = bySync.get(stamps[stamps.length - 2])!;
  return { currentCount, delta: currentCount - previousCount };
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

export type GameMemberBoardRankSeries = {
  raLeaderboardId: number;
  leaderboardTitle: string;
  points: { syncedAt: string; friendRank: number | null }[];
};

export function toGameMemberBoardRankSeries(
  memberId: string,
  items: GameHistoryItemDto[],
): GameMemberBoardRankSeries[] {
  const byBoard = new Map<number, GameMemberBoardRankSeries>();

  for (const item of items) {
    if (item.memberId !== memberId) {
      continue;
    }

    let series = byBoard.get(item.raLeaderboardId);
    if (!series) {
      series = {
        raLeaderboardId: item.raLeaderboardId,
        leaderboardTitle: item.leaderboardTitle,
        points: [],
      };
      byBoard.set(item.raLeaderboardId, series);
    }

    series.points.push({
      syncedAt: item.syncedAt,
      friendRank: item.friendRank ?? null,
    });
  }

  return [...byBoard.values()]
    .map((series) => ({
      ...series,
      points: dedupeBoardRankPoints(series.points),
    }))
    .filter((series) => series.points.some((point) => point.friendRank != null))
    .sort((a, b) => a.leaderboardTitle.localeCompare(b.leaderboardTitle));
}

export function countDistinctSyncTimestampsForGameMemberBoards(
  series: GameMemberBoardRankSeries[],
): number {
  const stamps = new Set<string>();
  for (const board of series) {
    for (const point of board.points) {
      stamps.add(point.syncedAt);
    }
  }
  return stamps.size;
}

export function memberLeadSeriesHasVariation(series: MemberLeadSeries[]): boolean {
  const values = series.flatMap((member) => member.points.map((point) => point.friendRankOnes));
  return new Set(values).size > 1;
}

export function gameMemberBoardRankSeriesHasVariation(series: GameMemberBoardRankSeries[]): boolean {
  for (const board of series) {
    const ranks = board.points
      .map((point) => point.friendRank)
      .filter((rank): rank is number => rank != null);
    if (new Set(ranks).size > 1) {
      return true;
    }
  }
  return false;
}

export function gameDeltaMovementScore(delta: GameDelta): number {
  const magnitudes = [
    delta.friendRankDelta,
    delta.globalRankDelta,
    delta.scoreDelta,
    delta.globalEntryCountDelta,
  ].filter((value): value is number => value != null);

  if (magnitudes.length === 0) {
    return 0;
  }

  return Math.max(...magnitudes.map((value) => Math.abs(value)));
}

export function isGameDeltaMover(delta: GameDelta): boolean {
  return gameDeltaMovementScore(delta) > 0;
}

export function sortGameDeltasByMovement(deltas: GameDelta[]): GameDelta[] {
  return [...deltas].sort((a, b) => {
    const byMovement = gameDeltaMovementScore(b) - gameDeltaMovementScore(a);
    if (byMovement !== 0) {
      return byMovement;
    }
    return (
      a.leaderboardTitle.localeCompare(b.leaderboardTitle) ||
      a.displayName.localeCompare(b.displayName)
    );
  });
}

export function indexGameDeltasByBoardMember(deltas: GameDelta[]): Map<string, GameDelta> {
  return new Map(deltas.map((delta) => [`${delta.raLeaderboardId}:${delta.memberId}`, delta]));
}
