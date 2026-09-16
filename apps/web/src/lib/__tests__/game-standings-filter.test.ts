import type { GameLeaderboardDto } from "@/generated/api-client";
import {
  applyLeaderboardFilters,
  filterLeaderboardsByQuery,
  filterLeaderboardsWithFriendScores,
} from "@/lib/game-standings-filter";

function board(
  partial: Partial<GameLeaderboardDto> & Pick<GameLeaderboardDto, "id" | "title">,
): GameLeaderboardDto {
  return {
    raLeaderboardId: 1,
    rankAsc: true,
    standings: [],
    ...partial,
  };
}

describe("filterLeaderboardsByQuery", () => {
  it("returns all boards when query is empty", () => {
    const boards = [board({ id: "a", title: "Fast Lap" }), board({ id: "b", title: "Top Score" })];

    const result = filterLeaderboardsByQuery(boards, "   ");

    expect(result).toEqual(boards);
  });

  it("matches title and description case-insensitively", () => {
    const boards = [
      board({ id: "a", title: "Chicken Challenge", description: "Beat the clock" }),
      board({ id: "b", title: "Other", description: null }),
    ];

    const byTitle = filterLeaderboardsByQuery(boards, "chicken");
    const byDescription = filterLeaderboardsByQuery(boards, "CLOCK");

    expect(byTitle.map((b) => b.id)).toEqual(["a"]);
    expect(byDescription.map((b) => b.id)).toEqual(["a"]);
  });
});

describe("filterLeaderboardsWithFriendScores", () => {
  it("keeps boards where a listed member has a score", () => {
    const boards = [
      board({
        id: "scored",
        title: "A",
        standings: [
          {
            memberId: "m1",
            raUsername: "a",
            displayName: "A",
            score: 100,
            friendRank: 1,
          },
        ],
      }),
      board({
        id: "empty",
        title: "B",
        standings: [
          {
            memberId: "m1",
            raUsername: "a",
            displayName: "A",
            score: null,
            friendRank: null,
          },
        ],
      }),
    ];

    const result = filterLeaderboardsWithFriendScores(boards, ["m1"]);

    expect(result.map((b) => b.id)).toEqual(["scored"]);
  });

  it("returns all boards when member list is empty", () => {
    const boards = [board({ id: "a", title: "A" })];

    const result = filterLeaderboardsWithFriendScores(boards, []);

    expect(result).toEqual(boards);
  });
});

describe("applyLeaderboardFilters", () => {
  it("applies friend filter before search", () => {
    const boards = [
      board({
        id: "1",
        title: "Fast Lap Alpha",
        standings: [
          {
            memberId: "m1",
            raUsername: "a",
            displayName: "A",
            score: 1,
            friendRank: 1,
          },
        ],
      }),
      board({
        id: "2",
        title: "Fast Lap Beta",
        standings: [
          {
            memberId: "m1",
            raUsername: "a",
            displayName: "A",
            score: null,
            friendRank: null,
          },
        ],
      }),
    ];

    const result = applyLeaderboardFilters(boards, {
      query: "alpha",
      friendScoresOnly: true,
      memberIds: ["m1"],
    });

    expect(result.map((b) => b.id)).toEqual(["1"]);
  });
});
