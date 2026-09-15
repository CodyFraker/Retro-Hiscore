import type { DashboardGameDto } from "@/generated/api-client";

export type DashboardGameSortKey = "recent" | "name" | "boards" | "population";

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
): DashboardGameDto[] {
  const sorted = [...games];

  sorted.sort((a, b) => {
    switch (sortKey) {
      case "name":
        return a.title.localeCompare(b.title);
      case "boards":
        return b.leaderboardCount - a.leaderboardCount || a.title.localeCompare(b.title);
      case "population":
        return (
          (b.totalRankedEntriesAcrossBoards ?? 0) - (a.totalRankedEntriesAcrossBoards ?? 0) ||
          a.title.localeCompare(b.title)
        );
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
