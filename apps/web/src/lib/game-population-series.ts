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

function aggregatePopulationBySync(data: GameLeaderboardPopulationHistoryResponse) {
  const bySync = new Map<string, { total: number; boardIds: Set<number> }>();

  for (const board of data.boards) {
    for (const point of board.points) {
      const row = bySync.get(point.syncedAt) ?? { total: 0, boardIds: new Set<number>() };
      if (!row.boardIds.has(board.raLeaderboardId)) {
        row.boardIds.add(board.raLeaderboardId);
        row.total += point.entryCount;
      }
      bySync.set(point.syncedAt, row);
    }
  }

  return bySync;
}

export function expectedPopulationBoardCount(
  data: GameLeaderboardPopulationHistoryResponse,
): number {
  return data.boards.length;
}

export function hasAnyPopulationSnapshots(data: GameLeaderboardPopulationHistoryResponse): boolean {
  return data.boards.some((board) => board.points.length > 0);
}

export function countCompletePopulationSyncs(data: GameLeaderboardPopulationHistoryResponse): number {
  const expected = expectedPopulationBoardCount(data);
  if (expected === 0) {
    return 0;
  }

  let count = 0;
  for (const row of aggregatePopulationBySync(data).values()) {
    if (row.boardIds.size === expected) {
      count += 1;
    }
  }
  return count;
}

export function hasPartialPopulationSnapshots(
  data: GameLeaderboardPopulationHistoryResponse,
): boolean {
  const expected = expectedPopulationBoardCount(data);
  if (expected === 0) {
    return false;
  }

  for (const row of aggregatePopulationBySync(data).values()) {
    if (row.boardIds.size > 0 && row.boardIds.size < expected) {
      return true;
    }
  }
  return false;
}

export function toTotalPopulationSeries(
  data: GameLeaderboardPopulationHistoryResponse,
): TotalPopulationPoint[] {
  const expectedBoardCount = expectedPopulationBoardCount(data);
  if (expectedBoardCount === 0) {
    return [];
  }

  return [...aggregatePopulationBySync(data).entries()]
    .filter(([, row]) => row.boardIds.size === expectedBoardCount)
    .map(([syncedAt, { total, boardIds }]) => ({
      syncedAt,
      totalEntryCount: total,
      boardCount: boardIds.size,
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
