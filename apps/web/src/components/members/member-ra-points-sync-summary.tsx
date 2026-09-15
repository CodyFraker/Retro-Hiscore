"use client";

import { Card, CardContent } from "@/components/ui/card";
import type { MemberRaRankHistoryItemDto } from "@/generated/api-client";
import { formatPointsDelta, summarizeRankHistory } from "@/lib/member-ra-rank-history";

type Props = {
  items: MemberRaRankHistoryItemDto[];
  showSoftcoreTile?: boolean;
};

function StatTile({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <Card size="sm" className="min-w-0 flex-1">
      <CardContent className="space-y-1">
        <p className="text-xs uppercase tracking-wide text-muted-foreground">{label}</p>
        <p className="font-mono text-lg font-medium tabular-nums">{value}</p>
        {hint ? <p className="text-xs text-muted-foreground">{hint}</p> : null}
      </CardContent>
    </Card>
  );
}

export function MemberRaPointsSyncSummary({ items, showSoftcoreTile = false }: Props) {
  const summary = summarizeRankHistory(items);

  if (items.length === 0) {
    return null;
  }

  const periodHint =
    summary.periodStartSyncedAt != null
      ? "since oldest snapshot in this history"
      : undefined;

  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
      <StatTile
        label="Hardcore points"
        value={summary.currentTotalPoints?.toLocaleString() ?? "—"}
      />
      <StatTile
        label="True points"
        value={summary.currentTotalTruePoints?.toLocaleString() ?? "—"}
        hint="Weighted by difficulty"
      />
      <StatTile
        label="HC change in period"
        value={formatPointsDelta(summary.periodHardcorePointsDelta)}
        hint={periodHint}
      />
      {showSoftcoreTile ? (
        <StatTile
          label="Softcore points"
          value={summary.currentTotalSoftcorePoints?.toLocaleString() ?? "—"}
        />
      ) : (
        <StatTile
          label="Latest HC change"
          value={formatPointsDelta(summary.latestHardcorePointsDelta)}
          hint={items.length > 1 ? "vs previous snapshot" : "needs two snapshots"}
        />
      )}
    </div>
  );
}
