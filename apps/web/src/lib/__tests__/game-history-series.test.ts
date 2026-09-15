import type { GameHistoryItemDto } from "@/generated/api-client";
import {
  countDistinctSyncTimestampsForBoardRanks,
  countDistinctSyncTimestampsForGameMemberBoards,
  countDistinctSyncTimestampsForMemberLeads,
  gameDeltaMovementScore,
  isGameDeltaMover,
  memberLeadSeriesHasVariation,
  sortGameDeltasByMovement,
  toBoardRankSeries,
  toGameDeltas,
  toGameMemberBoardRankSeries,
  toMemberLeadSeries,
} from "../game-history-series";

function item(
  overrides: Partial<GameHistoryItemDto> &
    Pick<
      GameHistoryItemDto,
      | "id"
      | "memberId"
      | "raUsername"
      | "displayName"
      | "raLeaderboardId"
      | "leaderboardTitle"
      | "score"
      | "syncedAt"
    >,
): GameHistoryItemDto {
  return {
    formattedScore: String(overrides.score),
    globalRank: null,
    friendRank: null,
    format: "VALUE",
    ...overrides,
  };
}

describe("toMemberLeadSeries", () => {
  it("counts friend rank ones per member at each sync timestamp", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        memberId: "m1",
        raUsername: "alice",
        displayName: "Alice",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 1,
      }),
      item({
        id: "2",
        memberId: "m2",
        raUsername: "bob",
        displayName: "Bob",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 50,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 2,
      }),
      item({
        id: "3",
        memberId: "m2",
        raUsername: "bob",
        displayName: "Bob",
        raLeaderboardId: 1002,
        leaderboardTitle: "Board B",
        score: 200,
        syncedAt: "2026-01-02T00:00:00Z",
        friendRank: 1,
      }),
    ];

    // Act
    const series = toMemberLeadSeries(items);
    const alice = series.find((entry) => entry.memberId === "m1");
    const bob = series.find((entry) => entry.memberId === "m2");

    // Assert
    expect(alice?.points).toEqual([
      { syncedAt: "2026-01-01T00:00:00Z", friendRankOnes: 1 },
      { syncedAt: "2026-01-02T00:00:00Z", friendRankOnes: 0 },
    ]);
    expect(bob?.points).toEqual([
      { syncedAt: "2026-01-01T00:00:00Z", friendRankOnes: 0 },
      { syncedAt: "2026-01-02T00:00:00Z", friendRankOnes: 1 },
    ]);
    expect(countDistinctSyncTimestampsForMemberLeads(series)).toBe(2);
  });
});

describe("toBoardRankSeries", () => {
  it("groups friend ranks by leaderboard over time", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        memberId: "m1",
        raUsername: "alice",
        displayName: "Alice",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 2,
      }),
      item({
        id: "2",
        memberId: "m1",
        raUsername: "alice",
        displayName: "Alice",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 150,
        syncedAt: "2026-01-02T00:00:00Z",
        friendRank: 1,
      }),
    ];

    // Act
    const series = toBoardRankSeries(items);

    // Assert
    expect(series).toHaveLength(1);
    expect(series[0].memberId).toBe("m1");
    expect(series[0].points).toEqual([
      { syncedAt: "2026-01-01T00:00:00Z", friendRank: 2 },
      { syncedAt: "2026-01-02T00:00:00Z", friendRank: 1 },
    ]);
    expect(countDistinctSyncTimestampsForBoardRanks(series)).toBe(2);
  });
});

describe("toGameDeltas", () => {
  it("returns score and rank deltas per board and member", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        memberId: "m1",
        raUsername: "alice",
        displayName: "Alice",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 2,
      }),
      item({
        id: "2",
        memberId: "m1",
        raUsername: "alice",
        displayName: "Alice",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 150,
        syncedAt: "2026-01-02T00:00:00Z",
        friendRank: 1,
      }),
    ];

    // Act
    const deltas = toGameDeltas(items);

    // Assert
    expect(deltas).toEqual([
      {
        memberId: "m1",
        displayName: "Alice",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        scoreDelta: 50,
        friendRankDelta: 1,
        globalRankDelta: null,
        globalEntryCount: null,
        globalEntryCountDelta: null,
      },
    ]);
  });
});

describe("toGameMemberBoardRankSeries", () => {
  it("returns one series per board for the selected member", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        memberId: "m1",
        raUsername: "alice",
        displayName: "Alice",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 2,
      }),
      item({
        id: "2",
        memberId: "m2",
        raUsername: "bob",
        displayName: "Bob",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 50,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 1,
      }),
      item({
        id: "3",
        memberId: "m1",
        raUsername: "alice",
        displayName: "Alice",
        raLeaderboardId: 1002,
        leaderboardTitle: "Board B",
        score: 200,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 1,
      }),
    ];

    // Act
    const series = toGameMemberBoardRankSeries("m1", items);

    // Assert
    expect(series).toHaveLength(2);
    expect(series.map((entry) => entry.leaderboardTitle)).toEqual(["Board A", "Board B"]);
    expect(countDistinctSyncTimestampsForGameMemberBoards(series)).toBe(1);
  });
});

describe("sortGameDeltasByMovement", () => {
  it("orders deltas by largest absolute movement first", () => {
    // Arrange
    const deltas = [
      {
        memberId: "m1",
        displayName: "Alice",
        raLeaderboardId: 1,
        leaderboardTitle: "A",
        scoreDelta: 1,
        friendRankDelta: null,
        globalRankDelta: null,
        globalEntryCount: null,
        globalEntryCountDelta: null,
      },
      {
        memberId: "m2",
        displayName: "Bob",
        raLeaderboardId: 2,
        leaderboardTitle: "B",
        scoreDelta: null,
        friendRankDelta: 3,
        globalRankDelta: null,
        globalEntryCount: null,
        globalEntryCountDelta: null,
      },
    ];

    // Act
    const sorted = sortGameDeltasByMovement(deltas);

    // Assert
    expect(sorted[0].memberId).toBe("m2");
    expect(gameDeltaMovementScore(sorted[0])).toBe(3);
    expect(isGameDeltaMover(sorted[1])).toBe(true);
  });
});

describe("memberLeadSeriesHasVariation", () => {
  it("returns false when every member has the same lead count at each sync", () => {
    // Arrange
    const series = toMemberLeadSeries([
      item({
        id: "1",
        memberId: "m1",
        raUsername: "alice",
        displayName: "Alice",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 1,
      }),
      item({
        id: "2",
        memberId: "m1",
        raUsername: "alice",
        displayName: "Alice",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 100,
        syncedAt: "2026-01-02T00:00:00Z",
        friendRank: 1,
      }),
    ]);

    // Act
    const hasVariation = memberLeadSeriesHasVariation(series);

    // Assert
    expect(hasVariation).toBe(false);
  });
});
