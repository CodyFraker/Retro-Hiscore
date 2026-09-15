import type { MemberRaRankHistoryItemDto } from "../../generated/api-client";
import {
  buildRankSnapshotRows,
  summarizeRankHistory,
} from "../member-ra-rank-history";

function snapshot(
  syncedAt: string,
  rank: number,
  totalPoints: number,
): MemberRaRankHistoryItemDto {
  return {
    syncedAt,
    rank,
    totalRanked: 100_000,
    totalPoints,
    totalTruePoints: totalPoints,
    totalSoftcorePoints: 0,
  };
}

describe("buildRankSnapshotRows", () => {
  it("orders newest first and computes deltas vs the previous snapshot", () => {
    const items = [
      snapshot("2026-01-01T12:00:00Z", 50_000, 1_000),
      snapshot("2026-01-02T12:00:00Z", 48_000, 1_150),
    ];

    const rows = buildRankSnapshotRows(items);

    expect(rows).toHaveLength(2);
    expect(rows[0].syncedAt).toBe("2026-01-02T12:00:00Z");
    expect(rows[0].rankDelta).toBe(2_000);
    expect(rows[0].hardcorePointsDelta).toBe(150);
    expect(rows[1].rankDelta).toBeNull();
    expect(rows[1].hardcorePointsDelta).toBeNull();
  });
});

describe("summarizeRankHistory", () => {
  it("summarizes current, best, and period rank change", () => {
    const items = [
      snapshot("2026-01-01T12:00:00Z", 50_000, 1_000),
      snapshot("2026-01-02T12:00:00Z", 48_000, 1_150),
      snapshot("2026-01-03T12:00:00Z", 49_000, 1_200),
    ];

    const summary = summarizeRankHistory(items);

    expect(summary.currentRank).toBe(49_000);
    expect(summary.bestRank).toBe(48_000);
    expect(summary.periodRankDelta).toBe(1_000);
    expect(summary.periodStartSyncedAt).toBe("2026-01-01T12:00:00Z");
    expect(summary.latestHardcorePointsDelta).toBe(50);
    expect(summary.periodHardcorePointsDelta).toBe(200);
  });
});
