import type { MemberStandingDto } from "@/generated/api-client";
import {
  buildMemberLeaderboardsQueryString,
  memberLeaderboardsPath,
  memberStandingsOffset,
  parseMemberStandingsPage,
  parseTrendHistoryPage,
  resolveMemberLeaderboardSelection,
} from "../member-leaderboards-params";

function standing(
  overrides: Partial<MemberStandingDto> & Pick<MemberStandingDto, "raGameId" | "raLeaderboardId">,
): MemberStandingDto {
  return {
    gameTitle: "Game A",
    leaderboardTitle: "Board A",
    format: null,
    friendRank: 1,
    globalRank: null,
    globalEntryCount: null,
    score: 100,
    formattedScore: "100",
    ...overrides,
  };
}

describe("parseMemberStandingsPage", () => {
  it("defaults to page 1", () => {
    expect(parseMemberStandingsPage(undefined)).toBe(1);
    expect(parseMemberStandingsPage("0")).toBe(1);
  });

  it("parses valid pages", () => {
    expect(parseMemberStandingsPage("3")).toBe(3);
  });
});

describe("parseTrendHistoryPage", () => {
  it("defaults to page 1", () => {
    expect(parseTrendHistoryPage(undefined)).toBe(1);
  });
});

describe("memberStandingsOffset", () => {
  it("computes offset from page", () => {
    expect(memberStandingsOffset(1)).toBe(0);
    expect(memberStandingsOffset(2)).toBe(15);
  });
});

describe("resolveMemberLeaderboardSelection", () => {
  const standings = [
    standing({
      raGameId: 2,
      gameTitle: "Beta",
      raLeaderboardId: 200,
      leaderboardTitle: "B Board",
    }),
    standing({
      raGameId: 1,
      gameTitle: "Alpha",
      raLeaderboardId: 100,
      leaderboardTitle: "A Board",
    }),
    standing({
      raGameId: 1,
      gameTitle: "Alpha",
      raLeaderboardId: 101,
      leaderboardTitle: "B Board",
    }),
  ];

  it("returns null when no standings", () => {
    expect(resolveMemberLeaderboardSelection([], undefined, undefined)).toBeNull();
  });

  it("defaults to first game and board alphabetically", () => {
    expect(resolveMemberLeaderboardSelection(standings, undefined, undefined)).toEqual({
      raGameId: 1,
      raLeaderboardId: 100,
    });
  });

  it("falls back when params are invalid", () => {
    expect(resolveMemberLeaderboardSelection(standings, "999", "888")).toEqual({
      raGameId: 1,
      raLeaderboardId: 100,
    });
  });

  it("honors valid game and board", () => {
    expect(resolveMemberLeaderboardSelection(standings, "2", "200")).toEqual({
      raGameId: 2,
      raLeaderboardId: 200,
    });
    expect(resolveMemberLeaderboardSelection(standings, "1", "101")).toEqual({
      raGameId: 1,
      raLeaderboardId: 101,
    });
  });
});

describe("buildMemberLeaderboardsQueryString", () => {
  it("builds leaderboards tab query", () => {
    expect(
      buildMemberLeaderboardsQueryString({
        tab: "leaderboards",
        game: 1,
        board: 100,
      }),
    ).toBe("?tab=leaderboards&game=1&board=100");
  });

  it("omits default page params", () => {
    expect(
      buildMemberLeaderboardsQueryString({
        tab: "leaderboards",
        standingsPage: 1,
        trendHistoryPage: 1,
        game: 1,
        board: 100,
      }),
    ).toBe("?tab=leaderboards&game=1&board=100");
  });
});

describe("memberLeaderboardsPath", () => {
  it("encodes username", () => {
    expect(
      memberLeaderboardsPath("User Name", { tab: "leaderboards", game: 1, board: 2 }),
    ).toBe("/members/User%20Name?tab=leaderboards&game=1&board=2");
  });
});
