"use client";

import type { DashboardAchievementHistoryItemDto } from "@/generated/api-client";
import { MemberRaMetricTrendChart } from "@/components/members/member-ra-metric-trend-chart";

type Props = {
  items: DashboardAchievementHistoryItemDto[];
};

const emptyMessage = "Group unlock history appears after achievement sync on tracked games.";

export function DashboardAchievementTrends({ items }: Props) {
  const unlockSeries = items.map((item) => ({
    syncedAt: item.earnedAt,
    value: item.cumulativeUnlocks,
  }));

  return (
    <MemberRaMetricTrendChart
      title="Group unlocks (tracked games)"
      emptyMessage={emptyMessage}
      dataKey="cumulativeUnlocks"
      label="Unlocks"
      items={unlockSeries}
      stroke="var(--chart-3)"
      yDomainMode="fromZero"
      tickFormat="date"
    />
  );
}
