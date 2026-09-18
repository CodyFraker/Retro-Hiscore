import type { GameLeaderboardPopulationHistoryResponse } from "@/generated/api-client";
import {
  countCompletePopulationSyncs,
  hasAnyPopulationSnapshots,
  hasPartialPopulationSnapshots,
  toBoardPopulationRows,
  toTotalPopulationSeries,
} from "@/lib/game-population-series";

describe("toTotalPopulationSeries", () => {
  it("sums entry counts across boards at each sync time", () => {
    const data: GameLeaderboardPopulationHistoryResponse = {
      boards: [
        {
          raLeaderboardId: 1,
          title: "Fast",
          points: [
            { syncedAt: "2026-01-01T00:00:00Z", entryCount: 100 },
            { syncedAt: "2026-01-02T00:00:00Z", entryCount: 120 },
          ],
        },
        {
          raLeaderboardId: 2,
          title: "Slow",
          points: [
            { syncedAt: "2026-01-01T00:00:00Z", entryCount: 50 },
            { syncedAt: "2026-01-02T00:00:00Z", entryCount: 55 },
          ],
        },
      ],
    };

    const series = toTotalPopulationSeries(data);

    expect(series).toEqual([
      { syncedAt: "2026-01-01T00:00:00Z", totalEntryCount: 150, boardCount: 2 },
      { syncedAt: "2026-01-02T00:00:00Z", totalEntryCount: 175, boardCount: 2 },
    ]);
  });

  it("excludes sync times where not every board was captured", () => {
    const data: GameLeaderboardPopulationHistoryResponse = {
      boards: [
        {
          raLeaderboardId: 1,
          title: "A",
          points: [
            { syncedAt: "2026-01-01T00:00:00Z", entryCount: 100 },
            { syncedAt: "2026-01-02T00:00:00Z", entryCount: 110 },
          ],
        },
        {
          raLeaderboardId: 2,
          title: "B",
          points: [{ syncedAt: "2026-01-01T00:00:00Z", entryCount: 50 }],
        },
      ],
    };

    const series = toTotalPopulationSeries(data);

    expect(series).toEqual([
      { syncedAt: "2026-01-01T00:00:00Z", totalEntryCount: 150, boardCount: 2 },
    ]);
    expect(hasPartialPopulationSnapshots(data)).toBe(true);
    expect(countCompletePopulationSyncs(data)).toBe(1);
  });

  it("returns empty series when there are no boards", () => {
    const data: GameLeaderboardPopulationHistoryResponse = { boards: [] };

    expect(toTotalPopulationSeries(data)).toEqual([]);
    expect(hasAnyPopulationSnapshots(data)).toBe(false);
    expect(countCompletePopulationSyncs(data)).toBe(0);
  });
});

describe("toBoardPopulationRows", () => {
  it("computes latest counts and deltas per board sorted by movement", () => {
    // Arrange
    const data: GameLeaderboardPopulationHistoryResponse = {
      boards: [
        {
          raLeaderboardId: 1,
          title: "Stable",
          points: [
            { syncedAt: "2026-01-01T00:00:00Z", entryCount: 100 },
            { syncedAt: "2026-01-02T00:00:00Z", entryCount: 100 },
          ],
        },
        {
          raLeaderboardId: 2,
          title: "Growing",
          points: [
            { syncedAt: "2026-01-01T00:00:00Z", entryCount: 50 },
            { syncedAt: "2026-01-02T00:00:00Z", entryCount: 80 },
          ],
        },
      ],
    };

    // Act
    const rows = toBoardPopulationRows(data);

    // Assert
    expect(rows[0].title).toBe("Growing");
    expect(rows[0].latestEntryCount).toBe(80);
    expect(rows[0].entryCountDelta).toBe(30);
    expect(rows[1].entryCountDelta).toBe(0);
  });
});
