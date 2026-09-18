import type { MemberStandingDto } from "@/generated/api-client";
import { filterTopLeaderboardStandings } from "../member-top-leaderboards";

function standing(
  overrides: Partial<MemberStandingDto> & Pick<MemberStandingDto, "raLeaderboardId">,
): MemberStandingDto {
  return {
    raGameId: 1,
    gameTitle: "Game",
    leaderboardTitle: "Board",
    format: null,
    friendRank: 1,
    globalRank: 100,
    globalEntryCount: 1000,
    score: 1,
    formattedScore: "1",
    ...overrides,
  };
}

describe("filterTopLeaderboardStandings", () => {
  it("includes standings in global top 20 percent sorted by percentile", () => {
    const result = filterTopLeaderboardStandings([
      standing({ raLeaderboardId: 1, globalRank: 300, globalEntryCount: 1000 }),
      standing({ raLeaderboardId: 2, globalRank: 50, globalEntryCount: 1000 }),
      standing({ raLeaderboardId: 3, globalRank: 200, globalEntryCount: 1000 }),
    ]);

    expect(result.map((row) => row.raLeaderboardId)).toEqual([2, 3]);
  });

  it("excludes rows without global rank data", () => {
    const result = filterTopLeaderboardStandings([
      standing({ raLeaderboardId: 1, globalRank: null, globalEntryCount: 1000 }),
      standing({ raLeaderboardId: 2, globalRank: 10, globalEntryCount: null }),
    ]);

    expect(result).toHaveLength(0);
  });

  it("includes exactly at 20 percent boundary", () => {
    const result = filterTopLeaderboardStandings([
      standing({ raLeaderboardId: 1, globalRank: 200, globalEntryCount: 1000 }),
      standing({ raLeaderboardId: 2, globalRank: 201, globalEntryCount: 1000 }),
    ]);

    expect(result.map((row) => row.raLeaderboardId)).toEqual([1]);
  });
});
