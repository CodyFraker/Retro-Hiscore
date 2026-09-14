import type { GameLeaderboardPopulationHistoryResponse } from "@/generated/api-client";

export type TotalPopulationPoint = {
  syncedAt: string;
  totalEntryCount: number;
  boardCount: number;
};

export function toTotalPopulationSeries(
  data: GameLeaderboardPopulationHistoryResponse,
): TotalPopulationPoint[] {
  const bySync = new Map<string, { total: number; boards: number }>();

  for (const board of data.boards) {
    for (const point of board.points) {
      const row = bySync.get(point.syncedAt) ?? { total: 0, boards: 0 };
      row.total += point.entryCount;
      row.boards += 1;
      bySync.set(point.syncedAt, row);
    }
  }

  return [...bySync.entries()]
    .map(([syncedAt, { total, boards }]) => ({
      syncedAt,
      totalEntryCount: total,
      boardCount: boards,
    }))
    .sort((a, b) => new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime());
}
