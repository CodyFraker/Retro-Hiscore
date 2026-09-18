import type { MemberStandingDto } from "@/generated/api-client";

export const MEMBER_STANDINGS_PAGE_SIZE = 15;

export type MemberLeaderboardsSearchParams = {
  tab?: string;
  standingsPage?: string;
  game?: string;
  board?: string;
  trendHistoryPage?: string;
};

export type MemberLeaderboardSelection = {
  raGameId: number;
  raLeaderboardId: number;
};

export function parseMemberStandingsPage(raw: string | undefined): number {
  const parsed = Number.parseInt(raw ?? "1", 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : 1;
}

export function parseTrendHistoryPage(raw: string | undefined): number {
  const parsed = Number.parseInt(raw ?? "1", 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : 1;
}

export function memberStandingsOffset(page: number): number {
  return (page - 1) * MEMBER_STANDINGS_PAGE_SIZE;
}

function uniqueGamesFromStandings(standings: MemberStandingDto[]): { raGameId: number; gameTitle: string }[] {
  const byId = new Map<number, string>();
  for (const standing of standings) {
    if (!byId.has(standing.raGameId)) {
      byId.set(standing.raGameId, standing.gameTitle);
    }
  }
  return [...byId.entries()]
    .map(([raGameId, gameTitle]) => ({ raGameId, gameTitle }))
    .sort((a, b) => a.gameTitle.localeCompare(b.gameTitle));
}

function boardsForGame(standings: MemberStandingDto[], raGameId: number): MemberStandingDto[] {
  return standings
    .filter((standing) => standing.raGameId === raGameId)
    .sort((a, b) => a.leaderboardTitle.localeCompare(b.leaderboardTitle));
}

export function resolveMemberLeaderboardSelection(
  standings: MemberStandingDto[],
  gameRaw: string | undefined,
  boardRaw: string | undefined,
): MemberLeaderboardSelection | null {
  if (standings.length === 0) {
    return null;
  }

  const games = uniqueGamesFromStandings(standings);
  const parsedGame = Number.parseInt(gameRaw ?? "", 10);
  const defaultGame = games[0]!;
  const raGameId = games.some((g) => g.raGameId === parsedGame) ? parsedGame : defaultGame.raGameId;

  const boards = boardsForGame(standings, raGameId);
  if (boards.length === 0) {
    return null;
  }

  const parsedBoard = Number.parseInt(boardRaw ?? "", 10);
  const defaultBoard = boards[0]!;
  const match = boards.find((b) => b.raLeaderboardId === parsedBoard);
  const raLeaderboardId = match?.raLeaderboardId ?? defaultBoard.raLeaderboardId;

  return { raGameId, raLeaderboardId };
}

export function buildMemberLeaderboardsQueryString(options: {
  tab?: "leaderboards";
  standingsPage?: number;
  game?: number;
  board?: number;
  trendHistoryPage?: number;
}): string {
  const params = new URLSearchParams();
  if (options.tab === "leaderboards") {
    params.set("tab", "leaderboards");
  }
  if (options.standingsPage && options.standingsPage > 1) {
    params.set("standingsPage", String(options.standingsPage));
  }
  if (options.game != null) {
    params.set("game", String(options.game));
  }
  if (options.board != null) {
    params.set("board", String(options.board));
  }
  if (options.trendHistoryPage && options.trendHistoryPage > 1) {
    params.set("trendHistoryPage", String(options.trendHistoryPage));
  }
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export function memberLeaderboardsPath(
  raUsername: string,
  options: Parameters<typeof buildMemberLeaderboardsQueryString>[0],
): string {
  return `/members/${encodeURIComponent(raUsername)}${buildMemberLeaderboardsQueryString(options)}`;
}

export function listGamesFromStandings(
  standings: MemberStandingDto[],
): { raGameId: number; gameTitle: string }[] {
  return uniqueGamesFromStandings(standings);
}

export function listBoardsForGameFromStandings(
  standings: MemberStandingDto[],
  raGameId: number,
): { raLeaderboardId: number; leaderboardTitle: string }[] {
  return boardsForGame(standings, raGameId).map((standing) => ({
    raLeaderboardId: standing.raLeaderboardId,
    leaderboardTitle: standing.leaderboardTitle,
  }));
}
