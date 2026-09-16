"use client";

import type { DashboardAchievementHistoryItemDto } from "@/generated/api-client";
import { MemberRaMetricTrendChart } from "@/components/members/member-ra-metric-trend-chart";

type Props = {
  items: DashboardAchievementHistoryItemDto[];
  weeklyUnlocks?: number;
};

const emptyMessage = "Group unlock history appears after achievement sync on tracked games.";

export function DashboardAchievementTrends({ items, weeklyUnlocks }: Props) {
  const unlockSeries = items.map((item) => ({
    syncedAt: item.earnedAt,
    value: item.cumulativeUnlocks,
  }));

  const showWeeklyDelta = weeklyUnlocks != null && weeklyUnlocks > 0;

  return (
    <div className="space-y-2">
      {showWeeklyDelta ? (
        <p className="text-sm text-muted-foreground">
          <span className="font-mono font-medium text-foreground">+{weeklyUnlocks}</span> this week
        </p>
      ) : null}
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
    </div>
  );
}
