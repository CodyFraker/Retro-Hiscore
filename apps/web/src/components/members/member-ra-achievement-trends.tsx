"use client";

import type { MemberRaAchievementHistoryItemDto } from "@/generated/api-client";
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
  const pointsSeries = items.map((item) => ({
    syncedAt: item.earnedAt,
    value: item.cumulativePoints,
  }));
  const truePointsSeries = items.map((item) => ({
    syncedAt: item.earnedAt,
    value: item.cumulativeTruePoints,
  }));

  return (
    <div className="grid gap-3 md:grid-cols-2">
      <MemberRaMetricTrendChart
        title="Cumulative unlocks"
        emptyMessage={emptyMessage}
        dataKey="cumulativeUnlocks"
        label="Unlocks"
        items={unlockSeries}
        stroke="var(--chart-1)"
      />
      <MemberRaMetricTrendChart
        title="Cumulative points (synced games)"
        emptyMessage={emptyMessage}
        dataKey="cumulativePoints"
        label="Points"
        items={pointsSeries}
        stroke="var(--chart-2)"
      />
      <MemberRaMetricTrendChart
        title="Cumulative true points"
        emptyMessage={emptyMessage}
        dataKey="cumulativeTruePoints"
        label="True points"
        items={truePointsSeries}
        stroke="var(--chart-3)"
      />
    </div>
  );
}
