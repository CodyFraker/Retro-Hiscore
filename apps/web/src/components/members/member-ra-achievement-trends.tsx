"use client";

import type { MemberRaAchievementHistoryItemDto } from "@/generated/api-client";
import { MemberRaAchievementActivitySummary } from "@/components/members/member-ra-achievement-activity-summary";
import { MemberRaMetricTrendChart } from "@/components/members/member-ra-metric-trend-chart";

type Props = {
  items: MemberRaAchievementHistoryItemDto[];
};

const emptyMessage =
  "Unlock history is recorded when achievement sync runs on tracked games (rank sync, manual achievement sync, or game refresh).";

export function MemberRaAchievementTrends({ items }: Props) {
  const unlockSeries = items.map((item) => ({
    syncedAt: item.earnedAt,
    value: item.cumulativeUnlocks,
  }));

  return (
    <div className="space-y-3">
      <MemberRaAchievementActivitySummary items={items} />
      <MemberRaMetricTrendChart
        title="Cumulative unlocks"
        emptyMessage={emptyMessage}
        dataKey="cumulativeUnlocks"
        label="Unlocks"
        items={unlockSeries}
        stroke="var(--chart-1)"
        yDomainMode="fromZero"
        tickFormat="date"
      />
    </div>
  );
}
