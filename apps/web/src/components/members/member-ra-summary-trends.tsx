"use client";

import type { MemberRaRankHistoryItemDto } from "@/generated/api-client";
import { MemberRaMetricTrendChart } from "@/components/members/member-ra-metric-trend-chart";

type Props = {
  items: MemberRaRankHistoryItemDto[];
  showSoftcorePoints?: boolean;
};

const syncEmptyMessage =
  "Snapshots are saved on each score sync. Run Refresh scores twice to draw a line. If this stays empty, restart the API so database migrations apply.";

export function MemberRaSummaryTrends({ items, showSoftcorePoints = false }: Props) {
  const showSoftcore =
    showSoftcorePoints || items.some((item) => (item.totalSoftcorePoints ?? 0) > 0);

  const rankSeries = items.map((item) => ({ syncedAt: item.syncedAt, value: item.rank }));
  const hardcoreSeries = items.map((item) => ({ syncedAt: item.syncedAt, value: item.totalPoints }));
  const trueSeries = items.map((item) => ({ syncedAt: item.syncedAt, value: item.totalTruePoints }));
  const softcoreSeries = items.map((item) => ({
    syncedAt: item.syncedAt,
    value: item.totalSoftcorePoints,
  }));

  return (
    <div className="grid gap-3 md:grid-cols-2">
      <MemberRaMetricTrendChart
        title="Site rank trend"
        emptyMessage={syncEmptyMessage}
        dataKey="rank"
        label="Site rank"
        items={rankSeries}
        reversedY
        formatValue={(value) => `#${value.toLocaleString()}`}
      />
      <MemberRaMetricTrendChart
        title="Hardcore points trend"
        emptyMessage={syncEmptyMessage}
        dataKey="totalPoints"
        label="Hardcore points"
        items={hardcoreSeries}
        stroke="var(--chart-2)"
      />
      <MemberRaMetricTrendChart
        title="True points trend"
        emptyMessage={syncEmptyMessage}
        dataKey="totalTruePoints"
        label="True points"
        items={trueSeries}
        stroke="var(--chart-3)"
      />
      {showSoftcore ? (
        <MemberRaMetricTrendChart
          title="Softcore points trend"
          emptyMessage={syncEmptyMessage}
          dataKey="totalSoftcorePoints"
          label="Softcore points"
          items={softcoreSeries}
          stroke="var(--chart-4)"
        />
      ) : null}
    </div>
  );
}
