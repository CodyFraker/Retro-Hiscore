import {
  computeMemberDeltas,
  countDistinctSyncTimestamps,
  toChartSeries,
  type MemberSeries,
} from "../history-series";
import type { LeaderboardHistoryItemDto } from "@/generated/api-client";

function item(
  overrides: Partial<LeaderboardHistoryItemDto> &
    Pick<LeaderboardHistoryItemDto, "id" | "memberId" | "displayName" | "score" | "syncedAt">,
): LeaderboardHistoryItemDto {
  return {
    raUsername: overrides.displayName,
    formattedScore: String(overrides.score),
    globalRank: null,
    friendRank: null,
    ...overrides,
  };
}

describe("toChartSeries", () => {
  it("groups points by member and sorts ascending by time", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        memberId: "a",
        displayName: "Alice",
        score: 200,
        syncedAt: "2026-01-02T00:00:00Z",
        friendRank: 1,
      }),
      item({
        id: "2",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 2,
      }),
      item({
        id: "3",
        memberId: "b",
        displayName: "Bob",
        score: 150,
        syncedAt: "2026-01-01T00:00:00Z",
        friendRank: 1,
      }),
    ];

    // Act
    const series = toChartSeries(items);

    // Assert
    expect(series.map((s) => s.displayName)).toEqual(["Alice", "Bob"]);
    expect(series[0].points.map((p) => p.score)).toEqual([100, 200]);
    expect(countDistinctSyncTimestamps(series)).toBe(2);
  });
});

describe("computeMemberDeltas", () => {
  it("returns null deltas until two points exist", () => {
    // Arrange
    const series: MemberSeries[] = [
      {
        memberId: "a",
        displayName: "Alice",
        points: [{ syncedAt: "2026-01-01T00:00:00Z", score: 100, friendRank: 1 }],
      },
    ];

    // Act
    const deltas = computeMemberDeltas(series);

    // Assert
    expect(deltas[0].scoreDelta).toBeNull();
    expect(deltas[0].friendRankDelta).toBeNull();
  });

  it("computes score and rank deltas from last two points", () => {
    // Arrange
    const series: MemberSeries[] = [
      {
        memberId: "a",
        displayName: "Alice",
        points: [
          { syncedAt: "2026-01-01T00:00:00Z", score: 100, friendRank: 2 },
          { syncedAt: "2026-01-02T00:00:00Z", score: 150, friendRank: 1 },
        ],
      },
    ];

    // Act
    const deltas = computeMemberDeltas(series);

    // Assert
    expect(deltas[0].scoreDelta).toBe(50);
    expect(deltas[0].friendRankDelta).toBe(1);
  });
});
