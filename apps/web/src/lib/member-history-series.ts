import type { MemberHistoryItemDto } from "@/generated/api-client";

export type AggregatePoint = {
  syncedAt: string;
  friendRankOnes: number;
  boardsWithScore: number;
};

export type BoardSeries = {
  raLeaderboardId: number;
  leaderboardTitle: string;
  gameTitle: string;
  points: { syncedAt: string; friendRank: number | null }[];
};

export function toAggregateSeries(items: MemberHistoryItemDto[]): AggregatePoint[] {
  const bySync = new Map<string, AggregatePoint>();

  for (const item of items) {
    let point = bySync.get(item.syncedAt);
    if (!point) {
      point = { syncedAt: item.syncedAt, friendRankOnes: 0, boardsWithScore: 0 };
      bySync.set(item.syncedAt, point);
    }

    point.boardsWithScore += 1;
    if (item.friendRank === 1) {
      point.friendRankOnes += 1;
    }
  }

  return [...bySync.values()].sort(
    (a, b) => new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime(),
  );
}

export function toBoardRankSeries(items: MemberHistoryItemDto[]): BoardSeries[] {
  const byBoard = new Map<number, BoardSeries>();

  for (const item of items) {
    let series = byBoard.get(item.raLeaderboardId);
    if (!series) {
      series = {
        raLeaderboardId: item.raLeaderboardId,
        leaderboardTitle: item.leaderboardTitle,
        gameTitle: item.gameTitle,
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
      points: [...series.points].sort(
        (a, b) => new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime(),
      ),
    }))
    .filter((series) => series.points.some((p) => p.friendRank != null))
    .sort((a, b) =>
      a.gameTitle.localeCompare(b.gameTitle) || a.leaderboardTitle.localeCompare(b.leaderboardTitle),
    );
}

export function countDistinctSyncTimestampsForAggregate(series: AggregatePoint[]): number {
  return series.length;
}

export function countDistinctSyncTimestampsForBoards(series: BoardSeries[]): number {
  const stamps = new Set<string>();
  for (const board of series) {
    for (const point of board.points) {
      stamps.add(point.syncedAt);
    }
  }
  return stamps.size;
}
