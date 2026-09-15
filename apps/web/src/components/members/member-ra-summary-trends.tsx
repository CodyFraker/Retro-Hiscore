"use client";

import type { MemberRaRankHistoryItemDto } from "@/generated/api-client";
import { MemberRaDualMetricTrendChart } from "@/components/members/member-ra-dual-metric-trend-chart";
import { MemberRaMetricTrendChart } from "@/components/members/member-ra-metric-trend-chart";
import { MemberRaPointsSyncSummary } from "@/components/members/member-ra-points-sync-summary";
import { MemberRaRankSnapshotSummary } from "@/components/members/member-ra-rank-snapshot-summary";
import { MemberRaRankSnapshotTable } from "@/components/members/member-ra-rank-snapshot-table";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { softcorePointsVaryInHistory } from "@/lib/member-ra-rank-history";

type Props = {
  items: MemberRaRankHistoryItemDto[];
  showSoftcorePoints?: boolean;
};

const syncEmptyMessage =
  "Snapshots are saved on each score sync. Run Refresh scores twice to draw a line. If this stays empty, restart the API so database migrations apply.";

export function MemberRaSummaryTrends({ items, showSoftcorePoints = false }: Props) {
  const showSoftcore =
    showSoftcorePoints || items.some((item) => (item.totalSoftcorePoints ?? 0) > 0);
  const showSoftcoreTile =
    showSoftcore && !softcorePointsVaryInHistory(items) && items.length > 0;

  const rankSeries = items.map((item) => ({ syncedAt: item.syncedAt, value: item.rank }));
  const hardcoreSeries = items.map((item) => ({ syncedAt: item.syncedAt, value: item.totalPoints }));
  const trueSeries = items.map((item) => ({ syncedAt: item.syncedAt, value: item.totalTruePoints }));

  const pointSnapshotsWithValues = items.filter(
    (item) => item.totalPoints != null || item.totalTruePoints != null,
  );

  return (
    <div className="space-y-6">
      <div className="space-y-3">
        <h4 className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
          Site rank
        </h4>
        <MemberRaMetricTrendChart
          title="Site rank trend"
          emptyMessage={syncEmptyMessage}
          dataKey="rank"
          label="Site rank"
          items={rankSeries}
          reversedY
          yDomainMode="tight"
          formatValue={(value) => `#${value.toLocaleString()}`}
        />
        <MemberRaRankSnapshotSummary items={items} />
        <div className="space-y-2">
          <h4 className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
            Rank snapshots
          </h4>
          <MemberRaRankSnapshotTable items={items} showSoftcorePoints={showSoftcorePoints} />
        </div>
      </div>

      <Card size="sm" className="min-w-0">
        <CardHeader>
          <CardTitle>Points from syncs</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <MemberRaPointsSyncSummary items={items} showSoftcoreTile={showSoftcoreTile} />
          {pointSnapshotsWithValues.length >= 2 ? (
            <MemberRaDualMetricTrendChart
              series={[
                {
                  dataKey: "totalPoints",
                  label: "Hardcore points",
                  stroke: "var(--chart-2)",
                  items: hardcoreSeries,
                },
                {
                  dataKey: "totalTruePoints",
                  label: "True points",
                  stroke: "var(--chart-3)",
                  items: trueSeries,
                },
              ]}
            />
          ) : (
            <p className="text-sm text-muted-foreground">
              {pointSnapshotsWithValues.length === 0
                ? "No point snapshots yet. They are recorded when score or rank sync runs."
                : "Run Refresh scores again to chart point trends."}
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
