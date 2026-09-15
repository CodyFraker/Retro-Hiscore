import type { DashboardGameDto } from "../../generated/api-client";
import {
  filterGamesByQuery,
  parseRaGameIdInput,
  sortDashboardGames,
} from "../dashboard-games";

function game(overrides: Partial<DashboardGameDto> & Pick<DashboardGameDto, "raGameId" | "title">): DashboardGameDto {
  return {
    id: overrides.id ?? "00000000-0000-0000-0000-000000000001",
    raGameId: overrides.raGameId,
    title: overrides.title,
    consoleName: overrides.consoleName ?? null,
    consoleIconUrl: overrides.consoleIconUrl ?? null,
    imageBoxArtUrl: overrides.imageBoxArtUrl ?? null,
    imageIconUrl: overrides.imageIconUrl ?? null,
    imageTitleUrl: overrides.imageTitleUrl ?? null,
    imageIngameUrl: overrides.imageIngameUrl ?? null,
    leaderboardCount: overrides.leaderboardCount ?? 0,
    totalRankedEntriesAcrossBoards: overrides.totalRankedEntriesAcrossBoards ?? null,
    friendRankOneLeader: overrides.friendRankOneLeader ?? null,
    playersWithAvatars: overrides.playersWithAvatars ?? [],
    lastActivityAt: overrides.lastActivityAt ?? null,
    leaderboardScoresSyncedAt: overrides.leaderboardScoresSyncedAt ?? null,
    totalAchievementsInCatalog: overrides.totalAchievementsInCatalog ?? null,
    leaderboardSyncStatus: overrides.leaderboardSyncStatus ?? {
      tier: "Cold",
      groupLastPlayedAt: null,
      leaderboardSyncNextDueAt: null,
      leaderboardSyncIntervalMinutes: 1440,
      leaderboardSyncIsDue: true,
      leaderboardSyncForcedCold: false,
    },
  };
}

describe("filterGamesByQuery", () => {
  const games = [
    game({ raGameId: 1, title: "Sonic Rush", consoleName: "Nintendo DS" }),
    game({ raGameId: 2, title: "Pinball Fantasies", consoleName: "Atari Jaguar" }),
  ];

  it("returns all games when query is empty", () => {
    expect(filterGamesByQuery(games, "")).toHaveLength(2);
  });

  it("matches title case-insensitively", () => {
    expect(filterGamesByQuery(games, "sonic")).toHaveLength(1);
    expect(filterGamesByQuery(games, "sonic")[0].raGameId).toBe(1);
  });

  it("matches console name", () => {
    expect(filterGamesByQuery(games, "jaguar")).toHaveLength(1);
    expect(filterGamesByQuery(games, "jaguar")[0].raGameId).toBe(2);
  });
});

describe("sortDashboardGames", () => {
  const games = [
    game({ raGameId: 1, title: "Alpha", leaderboardCount: 2, lastActivityAt: "2026-01-01T00:00:00Z" }),
    game({ raGameId: 2, title: "Beta", leaderboardCount: 8, lastActivityAt: "2026-02-01T00:00:00Z" }),
    game({ raGameId: 3, title: "Gamma", leaderboardCount: 5 }),
  ];

  it("sorts by recent activity descending", () => {
    const sorted = sortDashboardGames(games, "recent");
    expect(sorted.map((g) => g.raGameId)).toEqual([2, 1, 3]);
  });

  it("sorts by name ascending", () => {
    const sorted = sortDashboardGames(games, "name");
    expect(sorted.map((g) => g.title)).toEqual(["Alpha", "Beta", "Gamma"]);
  });

  it("sorts by board count descending", () => {
    const sorted = sortDashboardGames(games, "boards");
    expect(sorted.map((g) => g.raGameId)).toEqual([2, 3, 1]);
  });

  it("sorts by total ranked entries descending", () => {
    const populationGames = [
      game({ raGameId: 1, title: "Alpha", totalRankedEntriesAcrossBoards: 100 }),
      game({ raGameId: 2, title: "Beta", totalRankedEntriesAcrossBoards: 500 }),
      game({ raGameId: 3, title: "Gamma", totalRankedEntriesAcrossBoards: 200 }),
    ];
    const sorted = sortDashboardGames(populationGames, "population");
    expect(sorted.map((g) => g.raGameId)).toEqual([2, 3, 1]);
  });
});

describe("parseRaGameIdInput", () => {
  it("parses plain numeric ids", () => {
    expect(parseRaGameIdInput("38130")).toBe(38130);
    expect(parseRaGameIdInput("  42  ")).toBe(42);
  });

  it("parses retroachievements game urls", () => {
    expect(parseRaGameIdInput("https://retroachievements.org/game/38130")).toBe(38130);
    expect(parseRaGameIdInput("retroachievements.org/game/999")).toBe(999);
  });

  it("parses query param ids", () => {
    expect(parseRaGameIdInput("https://example.com?id=12345")).toBe(12345);
  });

  it("returns null for invalid input", () => {
    expect(parseRaGameIdInput("")).toBeNull();
    expect(parseRaGameIdInput("abc")).toBeNull();
    expect(parseRaGameIdInput("0")).toBeNull();
    expect(parseRaGameIdInput("-5")).toBeNull();
  });
});
