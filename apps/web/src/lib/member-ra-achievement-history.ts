import type { MemberRaAchievementHistoryItemDto } from "@/generated/api-client";

const THIRTY_DAYS_MS = 30 * 24 * 60 * 60 * 1000;

export type MemberRaAchievementActivitySummary = {
  totalUnlocks: number;
  unlocksLast30Days: number;
  pointsEarnedLast30Days: number;
};

export function summarizeAchievementActivity(
  items: MemberRaAchievementHistoryItemDto[],
  now: Date = new Date(),
): MemberRaAchievementActivitySummary {
  if (items.length === 0) {
    return {
      totalUnlocks: 0,
      unlocksLast30Days: 0,
      pointsEarnedLast30Days: 0,
    };
  }

  const windowStart = now.getTime() - THIRTY_DAYS_MS;
  let unlocksLast30Days = 0;
  let pointsEarnedLast30Days = 0;

  for (const item of items) {
    if (new Date(item.earnedAt).getTime() < windowStart) {
      continue;
    }
    unlocksLast30Days += 1;
    pointsEarnedLast30Days += item.pointsEarned ?? 0;
  }

  const last = items[items.length - 1];

  return {
    totalUnlocks: last.cumulativeUnlocks,
    unlocksLast30Days,
    pointsEarnedLast30Days,
  };
}
