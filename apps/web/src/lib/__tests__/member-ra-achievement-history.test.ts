import type { MemberRaAchievementHistoryItemDto } from "../../generated/api-client";
import { summarizeAchievementActivity } from "../member-ra-achievement-history";

function unlock(
  earnedAt: string,
  pointsEarned: number,
  cumulativeUnlocks: number,
): MemberRaAchievementHistoryItemDto {
  return {
    earnedAt,
    pointsEarned,
    truePointsEarned: pointsEarned,
    cumulativeUnlocks,
    cumulativePoints: cumulativeUnlocks * pointsEarned,
    cumulativeTruePoints: cumulativeUnlocks * pointsEarned,
  };
}

describe("summarizeAchievementActivity", () => {
  it("counts unlocks and points in the last 30 days", () => {
    const now = new Date("2026-09-15T12:00:00Z");
    const items = [
      unlock("2026-08-01T12:00:00Z", 5, 1),
      unlock("2026-09-10T12:00:00Z", 10, 2),
      unlock("2026-09-14T12:00:00Z", 3, 3),
    ];

    const summary = summarizeAchievementActivity(items, now);

    expect(summary.totalUnlocks).toBe(3);
    expect(summary.unlocksLast30Days).toBe(2);
    expect(summary.pointsEarnedLast30Days).toBe(13);
  });
});
