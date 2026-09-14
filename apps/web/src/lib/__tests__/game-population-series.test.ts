import type { GameLeaderboardPopulationHistoryResponse } from "@/generated/api-client";
import { toTotalPopulationSeries } from "@/lib/game-population-series";

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
});
