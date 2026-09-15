import type { DashboardGameSortKey } from "@/lib/dashboard-games";

export const TRACKED_GAMES_PAGE_SIZE = 20;

export type TrackedGamesSearchParams = {
  page?: string;
  q?: string;
  sort?: string;
};

export function parseTrackedGamesSort(raw: string | undefined): DashboardGameSortKey {
  switch (raw) {
    case "name":
    case "boards":
    case "population":
    case "recent":
      return raw;
    default:
      return "recent";
  }
}

export function parseTrackedGamesPage(raw: string | undefined): number {
  const parsed = Number.parseInt(raw ?? "1", 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : 1;
}

export function trackedGamesOffset(page: number): number {
  return (page - 1) * TRACKED_GAMES_PAGE_SIZE;
}

export function buildTrackedGamesQueryString(options: {
  page?: number;
  q?: string;
  sort?: DashboardGameSortKey;
}): string {
  const params = new URLSearchParams();
  if (options.page && options.page > 1) {
    params.set("page", String(options.page));
  }
  const q = options.q?.trim();
  if (q) {
    params.set("q", q);
  }
  if (options.sort && options.sort !== "recent") {
    params.set("sort", options.sort);
  }
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}
