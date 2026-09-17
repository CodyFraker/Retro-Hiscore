import {
  computeLeaderboardPopulationDelta,
  computeMemberDeltas,
  countDistinctSyncTimestamps,
  dedupeLeaderboardHistoryItems,
  enrichLeaderboardHistoryRows,
  memberSeriesHasGlobalRankVariation,
  parseLeaderboardHistoryPage,
  toChartSeries,
  toLeaderboardPopulationSeries,
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
        points: [
          {
            syncedAt: "2026-01-01T00:00:00Z",
            score: 100,
            formattedScore: "100",
            friendRank: 1,
            globalRank: null,
          },
        ],
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
          {
            syncedAt: "2026-01-01T00:00:00Z",
            score: 100,
            formattedScore: "100",
            friendRank: 2,
            globalRank: null,
          },
          {
            syncedAt: "2026-01-02T00:00:00Z",
            score: 150,
            formattedScore: "150",
            friendRank: 1,
            globalRank: null,
          },
        ],
      },
    ];

    // Act
    const deltas = computeMemberDeltas(series);

    // Assert
    expect(deltas[0].scoreDelta).toBe(50);
    expect(deltas[0].friendRankDelta).toBe(1);
  });

  it("computes global rank delta when score is unchanged", () => {
    // Arrange
    const series: MemberSeries[] = [
      {
        memberId: "a",
        displayName: "Shrimp",
        points: [
          {
            syncedAt: "2026-09-16T22:05:00Z",
            score: 134990,
            formattedScore: "2:14.99",
            friendRank: 1,
            globalRank: 149,
          },
          {
            syncedAt: "2026-09-16T23:25:00Z",
            score: 134990,
            formattedScore: "2:14.99",
            friendRank: 1,
            globalRank: 150,
          },
        ],
      },
    ];

    // Act
    const deltas = computeMemberDeltas(series);

    // Assert
    expect(deltas[0].scoreDelta).toBe(0);
    expect(deltas[0].globalRankDelta).toBe(-1);
  });
});

describe("toLeaderboardPopulationSeries", () => {
  it("dedupes entry counts by sync time and sorts ascending", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-02T00:00:00Z",
        globalEntryCount: 150,
      }),
      item({
        id: "2",
        memberId: "b",
        displayName: "Bob",
        score: 90,
        syncedAt: "2026-01-02T00:00:00Z",
        globalEntryCount: 150,
      }),
      item({
        id: "3",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        globalEntryCount: 149,
      }),
    ];

    // Act
    const series = toLeaderboardPopulationSeries(items);

    // Assert
    expect(series).toEqual([
      { syncedAt: "2026-01-01T00:00:00Z", entryCount: 149 },
      { syncedAt: "2026-01-02T00:00:00Z", entryCount: 150 },
    ]);
  });
});

describe("computeLeaderboardPopulationDelta", () => {
  it("returns population change between the last two sync stamps", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        globalEntryCount: 149,
      }),
      item({
        id: "2",
        memberId: "b",
        displayName: "Bob",
        score: 90,
        syncedAt: "2026-01-01T00:00:00Z",
        globalEntryCount: 149,
      }),
      item({
        id: "3",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-02T00:00:00Z",
        globalEntryCount: 150,
      }),
    ];

    // Act
    const population = computeLeaderboardPopulationDelta(items);

    // Assert
    expect(population?.currentCount).toBe(150);
    expect(population?.delta).toBe(1);
  });
});

describe("dedupeLeaderboardHistoryItems", () => {
  it("drops snapshots when score and global rank are unchanged since the prior sync", () => {
    // Arrange
    const items = [
      item({
        id: "3",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-03T00:00:00Z",
        globalRank: 150,
      }),
      item({
        id: "2",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-02T00:00:00Z",
        globalRank: 150,
      }),
      item({
        id: "1",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        globalRank: 149,
      }),
    ];

    // Act
    const deduped = dedupeLeaderboardHistoryItems(items);

    // Assert
    expect(deduped.map((row) => row.id)).toEqual(["2", "1"]);
  });

  it("keeps the first snapshot for each member", () => {
    // Arrange
    const items = [
      item({
        id: "1",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        globalRank: 10,
      }),
    ];

    // Act
    const deduped = dedupeLeaderboardHistoryItems(items);

    // Assert
    expect(deduped).toHaveLength(1);
  });
});

describe("parseLeaderboardHistoryPage", () => {
  it("clamps invalid and out-of-range page numbers", () => {
    // Arrange
    // Act
    // Assert
    expect(parseLeaderboardHistoryPage(undefined, 3)).toBe(1);
    expect(parseLeaderboardHistoryPage("0", 3)).toBe(1);
    expect(parseLeaderboardHistoryPage("2", 3)).toBe(2);
    expect(parseLeaderboardHistoryPage("9", 3)).toBe(3);
  });
});

describe("enrichLeaderboardHistoryRows", () => {
  it("attaches global rank delta vs the member previous snapshot", () => {
    // Arrange
    const items = [
      item({
        id: "2",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-02T00:00:00Z",
        globalRank: 150,
      }),
      item({
        id: "1",
        memberId: "a",
        displayName: "Alice",
        score: 100,
        syncedAt: "2026-01-01T00:00:00Z",
        globalRank: 149,
      }),
    ];

    // Act
    const rows = enrichLeaderboardHistoryRows(items);

    // Assert
    expect(rows.find((row) => row.id === "2")?.globalRankDelta).toBe(-1);
    expect(rows.find((row) => row.id === "1")?.globalRankDelta).toBeNull();
  });
});

describe("memberSeriesHasGlobalRankVariation", () => {
  it("is true when any member global rank changes across syncs", () => {
    // Arrange
    const series: MemberSeries[] = [
      {
        memberId: "a",
        displayName: "Alice",
        points: [
          {
            syncedAt: "2026-01-01T00:00:00Z",
            score: 100,
            formattedScore: "100",
            friendRank: 1,
            globalRank: 10,
          },
          {
            syncedAt: "2026-01-02T00:00:00Z",
            score: 100,
            formattedScore: "100",
            friendRank: 1,
            globalRank: 11,
          },
        ],
      },
    ];

    // Act
    const hasVariation = memberSeriesHasGlobalRankVariation(series);

    // Assert
    expect(hasVariation).toBe(true);
  });
});
