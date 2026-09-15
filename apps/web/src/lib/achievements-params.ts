export const ACHIEVEMENTS_PAGE_SIZE = 50;

export type AchievementsSearchParams = {
  page?: string;
  game?: string;
  member?: string;
};

export function parseAchievementsPage(raw: string | undefined): number {
  const parsed = Number.parseInt(raw ?? "1", 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : 1;
}

export function parseAchievementsGameId(raw: string | undefined): number | undefined {
  if (!raw?.trim()) {
    return undefined;
  }
  const parsed = Number.parseInt(raw, 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : undefined;
}

export function achievementsOffset(page: number): number {
  return (page - 1) * ACHIEVEMENTS_PAGE_SIZE;
}

export function buildAchievementsQueryString(options: {
  page?: number;
  game?: number;
  member?: string;
}): string {
  const params = new URLSearchParams();
  if (options.page && options.page > 1) {
    params.set("page", String(options.page));
  }
  if (options.game != null) {
    params.set("game", String(options.game));
  }
  const member = options.member?.trim();
  if (member) {
    params.set("member", member);
  }
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}
