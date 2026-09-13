import type { ActivityItemDto, DashboardGameDto } from "@/generated/api-client";

export type DashboardGameSortKey = "recent" | "name" | "boards" | "changes";

export function countActivityByGameId(
  activity: ActivityItemDto[],
): Map<number, number> {
  const counts = new Map<number, number>();
  for (const item of activity) {
    counts.set(item.raGameId, (counts.get(item.raGameId) ?? 0) + 1);
  }
  return counts;
}

export function activityCountForGame(
  activityCounts: Map<number, number>,
  raGameId: number,
): number {
  return activityCounts.get(raGameId) ?? 0;
}

export function filterGamesByQuery(
  games: DashboardGameDto[],
  query: string,
): DashboardGameDto[] {
  const normalized = query.trim().toLowerCase();
  if (!normalized) {
    return games;
  }

  return games.filter((game) => {
    const title = game.title.toLowerCase();
    const consoleName = game.consoleName?.toLowerCase() ?? "";
    return title.includes(normalized) || consoleName.includes(normalized);
  });
}

export function sortDashboardGames(
  games: DashboardGameDto[],
  sortKey: DashboardGameSortKey,
  activityCounts: Map<number, number>,
): DashboardGameDto[] {
  const sorted = [...games];

  sorted.sort((a, b) => {
    switch (sortKey) {
      case "name":
        return a.title.localeCompare(b.title);
      case "boards":
        return b.leaderboardCount - a.leaderboardCount || a.title.localeCompare(b.title);
      case "changes": {
        const aChanges = activityCounts.get(a.raGameId) ?? 0;
        const bChanges = activityCounts.get(b.raGameId) ?? 0;
        return bChanges - aChanges || a.title.localeCompare(b.title);
      }
      case "recent":
      default: {
        const aTime = a.lastActivityAt ? Date.parse(a.lastActivityAt) : 0;
        const bTime = b.lastActivityAt ? Date.parse(b.lastActivityAt) : 0;
        return bTime - aTime || a.title.localeCompare(b.title);
      }
    }
  });

  return sorted;
}

export function parseRaGameIdInput(raw: string): number | null {
  const trimmed = raw.trim();
  if (!trimmed) {
    return null;
  }

  if (/^\d+$/.test(trimmed)) {
    const parsed = Number.parseInt(trimmed, 10);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
  }

  const urlMatch = trimmed.match(/\/game\/(\d+)(?:\D|$)/i);
  if (urlMatch?.[1]) {
    const parsed = Number.parseInt(urlMatch[1], 10);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
  }

  const queryMatch = trimmed.match(/[?&]id=(\d+)/i);
  if (queryMatch?.[1]) {
    const parsed = Number.parseInt(queryMatch[1], 10);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
  }

  return null;
}
