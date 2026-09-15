import type { GameLeaderboardSyncStatusDto } from "../../generated/api-client";
import {
  leaderboardSyncTierDescription,
  leaderboardSyncTierLabel,
} from "../leaderboard-sync-tier";

function status(overrides: Partial<GameLeaderboardSyncStatusDto> = {}): GameLeaderboardSyncStatusDto {
  return {
    tier: "Hot",
    groupLastPlayedAt: "2026-01-15T12:00:00Z",
    leaderboardSyncNextDueAt: "2026-01-15T12:30:00Z",
    leaderboardSyncIntervalMinutes: 15,
    leaderboardSyncIsDue: false,
    leaderboardSyncForcedCold: false,
    ...overrides,
  };
}

describe("leaderboardSyncTierLabel", () => {
  it("maps Hot and Cold tiers", () => {
    expect(leaderboardSyncTierLabel("Hot")).toBe("Hot sync");
    expect(leaderboardSyncTierLabel("Cold")).toBe("Cold sync");
    expect(leaderboardSyncTierLabel("Hot", true)).toBe("Cold sync (pinned)");
  });
});

describe("leaderboardSyncTierDescription", () => {
  it("mentions interval minutes for hot tier", () => {
    const text = leaderboardSyncTierDescription(status());
    expect(text).toContain("Hot");
    expect(text).toContain("15 minute");
  });

  it("notes when sync is due", () => {
    const text = leaderboardSyncTierDescription(status({ leaderboardSyncIsDue: true }));
    expect(text).toContain("due now");
  });

  it("notes admin-pinned cold schedule", () => {
    const text = leaderboardSyncTierDescription(
      status({ leaderboardSyncForcedCold: true, tier: "Cold" }),
    );
    expect(text).toContain("admin pinned");
  });

  it("handles missing group play", () => {
    const text = leaderboardSyncTierDescription(
      status({ tier: "Cold", groupLastPlayedAt: null, leaderboardSyncIntervalMinutes: 1440 }),
    );
    expect(text).toContain("No recent group play");
    expect(text).toContain("Cold");
  });
});
