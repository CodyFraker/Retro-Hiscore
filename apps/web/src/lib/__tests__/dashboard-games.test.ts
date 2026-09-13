import type { ActivityItemDto, DashboardGameDto } from "../../generated/api-client";
import {
  countActivityByGameId,
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
    friendRankOneLeader: overrides.friendRankOneLeader ?? null,
    lastActivityAt: overrides.lastActivityAt ?? null,
  };
}

describe("countActivityByGameId", () => {
  it("groups activity items by game id", () => {
    const activity: ActivityItemDto[] = [
      {
        memberId: "a",
        raUsername: "a",
        displayName: "A",
        raGameId: 1,
        gameTitle: "One",
        raLeaderboardId: 10,
        leaderboardTitle: "Board",
      },
      {
        memberId: "b",
        raUsername: "b",
        displayName: "B",
        raGameId: 1,
        gameTitle: "One",
        raLeaderboardId: 11,
        leaderboardTitle: "Board 2",
      },
      {
        memberId: "c",
        raUsername: "c",
        displayName: "C",
        raGameId: 2,
        gameTitle: "Two",
        raLeaderboardId: 20,
        leaderboardTitle: "Board",
      },
    ];

    const counts = countActivityByGameId(activity);

    expect(counts.get(1)).toBe(2);
    expect(counts.get(2)).toBe(1);
  });
});

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
  const activityCounts = new Map<number, number>([[1, 3], [3, 1]]);

  it("sorts by recent activity descending", () => {
    const sorted = sortDashboardGames(games, "recent", activityCounts);
    expect(sorted.map((g) => g.raGameId)).toEqual([2, 1, 3]);
  });

  it("sorts by name ascending", () => {
    const sorted = sortDashboardGames(games, "name", activityCounts);
    expect(sorted.map((g) => g.title)).toEqual(["Alpha", "Beta", "Gamma"]);
  });

  it("sorts by board count descending", () => {
    const sorted = sortDashboardGames(games, "boards", activityCounts);
    expect(sorted.map((g) => g.raGameId)).toEqual([2, 3, 1]);
  });

  it("sorts by activity change count descending", () => {
    const sorted = sortDashboardGames(games, "changes", activityCounts);
    expect(sorted.map((g) => g.raGameId)).toEqual([1, 3, 2]);
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
