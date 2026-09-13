import type { MemberHistoryItemDto } from "@/generated/api-client";
import {
  countDistinctSyncTimestampsForAggregate,
  countDistinctSyncTimestampsForBoards,
  toAggregateSeries,
  toBoardRankSeries,
} from "../member-history-series";

function item(
  overrides: Partial<MemberHistoryItemDto> &
    Pick<
      MemberHistoryItemDto,
      "id" | "raGameId" | "gameTitle" | "raLeaderboardId" | "leaderboardTitle" | "score" | "syncedAt"
    >,
): MemberHistoryItemDto {
  return {
    formattedScore: String(overrides.score),
    globalRank: null,
    friendRank: null,
    format: "VALUE",
    ...overrides,
  };
}

describe("toAggregateSeries", () => {
  it("groups by syncedAt and counts friend rank ones", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        raGameId: 100,
        gameTitle: "Game A",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 1,
      }),
      item({
        id: "2",
        raGameId: 100,
        gameTitle: "Game A",
        raLeaderboardId: 1002,
        leaderboardTitle: "Board B",
        score: 50,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 2,
      }),
      item({
        id: "3",
        raGameId: 100,
        gameTitle: "Game A",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 150,
        syncedAt: "2026-01-02T00:00:00Z",
        friendRank: 1,
      }),
    ];

    // Act
    const series = toAggregateSeries(items);

    // Assert
    expect(series).toHaveLength(2);
    expect(series[0]).toEqual({
      syncedAt: "2026-01-01T00:00:00Z",
      friendRankOnes: 1,
      boardsWithScore: 2,
    });
    expect(series[1]).toEqual({
      syncedAt: "2026-01-02T00:00:00Z",
      friendRankOnes: 1,
      boardsWithScore: 1,
    });
    expect(countDistinctSyncTimestampsForAggregate(series)).toBe(2);
  });
});

describe("toBoardRankSeries", () => {
  it("groups points by board and sorts ascending by time", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        raGameId: 100,
        gameTitle: "Game A",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 200,
        syncedAt: "2026-01-02T00:00:00Z",
        friendRank: 1,
      }),
      item({
        id: "2",
        raGameId: 100,
        gameTitle: "Game A",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 2,
      }),
      item({
        id: "3",
        raGameId: 200,
        gameTitle: "Game B",
        raLeaderboardId: 2001,
        leaderboardTitle: "Board C",
        score: 50,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 1,
      }),
    ];

    // Act
    const series = toBoardRankSeries(items);

    // Assert
    expect(series).toHaveLength(2);
    expect(series[0].leaderboardTitle).toBe("Board A");
    expect(series[0].points.map((p) => p.friendRank)).toEqual([2, 1]);
    expect(series[1].leaderboardTitle).toBe("Board C");
    expect(countDistinctSyncTimestampsForBoards(series)).toBe(2);
  });

  it("excludes boards with no friend rank data", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        raGameId: 100,
        gameTitle: "Game A",
        raLeaderboardId: 1001,
        leaderboardTitle: "Board A",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: null,
      }),
    ];

    // Act
    const series = toBoardRankSeries(items);

    // Assert
    expect(series).toHaveLength(0);
  });
});
