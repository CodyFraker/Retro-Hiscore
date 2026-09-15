import type {
  GameLeaderboardPopulationHistoryResponse,
  LeaderboardPopulationPointDto,
} from "@/generated/api-client";

export type TotalPopulationPoint = {
  syncedAt: string;
  totalEntryCount: number;
  boardCount: number;
};

export type BoardPopulationRow = {
  raLeaderboardId: number;
  title: string;
  latestEntryCount: number;
  entryCountDelta: number | null;
  points: LeaderboardPopulationPointDto[];
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

export function toBoardPopulationRows(
  data: GameLeaderboardPopulationHistoryResponse,
): BoardPopulationRow[] {
  return data.boards
    .map((board) => {
      const points = [...board.points].sort(
        (a, b) => new Date(a.syncedAt).getTime() - new Date(b.syncedAt).getTime(),
      );
      const latest = points.at(-1);
      const previous = points.length >= 2 ? points.at(-2) : undefined;

      return {
        raLeaderboardId: board.raLeaderboardId,
        title: board.title,
        latestEntryCount: latest?.entryCount ?? 0,
        entryCountDelta:
          latest != null && previous != null ? latest.entryCount - previous.entryCount : null,
        points,
      };
    })
    .filter((row) => row.points.length > 0)
    .sort((a, b) => {
      const deltaDiff =
        Math.abs(b.entryCountDelta ?? 0) - Math.abs(a.entryCountDelta ?? 0);
      if (deltaDiff !== 0) {
        return deltaDiff;
      }
      return a.title.localeCompare(b.title);
    });
}
