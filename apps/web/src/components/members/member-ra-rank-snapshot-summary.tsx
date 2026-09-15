"use client";

import { Card, CardContent } from "@/components/ui/card";
import type { MemberRaRankHistoryItemDto } from "@/generated/api-client";
import {
  formatPointsDelta,
  formatRankDelta,
  summarizeRankHistory,
} from "@/lib/member-ra-rank-history";
import { formatRankLabel } from "@/lib/ra-member-metrics";

type Props = {
  items: MemberRaRankHistoryItemDto[];
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

export function MemberRaRankSnapshotSummary({ items }: Props) {
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
        label="Current site rank"
        value={formatRankLabel(summary.currentRank, summary.currentTotalRanked)}
      />
      <StatTile
        label="Best rank in period"
        value={summary.bestRank != null ? `#${summary.bestRank.toLocaleString()}` : "—"}
        hint={items.length > 1 ? "lowest rank recorded here" : undefined}
      />
      <StatTile
        label="Rank change in period"
        value={formatRankDelta(summary.periodRankDelta)}
        hint={periodHint}
      />
      <StatTile
        label="Latest HC points change"
        value={formatPointsDelta(summary.latestHardcorePointsDelta)}
        hint={items.length > 1 ? "vs previous snapshot" : "needs two snapshots"}
      />
    </div>
  );
}
