"use client";

import { useMemo, useState } from "react";
import { FormattedSyncTime } from "@/components/formatted-sync-time";
import { Button } from "@/components/ui/button";
import type { MemberRaRankHistoryItemDto } from "@/generated/api-client";
import {
  buildRankSnapshotRows,
  formatPointsDelta,
  formatRankDelta,
} from "@/lib/member-ra-rank-history";
import { formatRankLabel } from "@/lib/ra-member-metrics";

type Props = {
  items: MemberRaRankHistoryItemDto[];
  showSoftcorePoints?: boolean;
};

const initialVisibleRows = 25;

function deltaClass(delta: number | null) {
  if (delta == null || delta === 0) {
    return "text-muted-foreground";
  }

  return delta > 0 ? "text-[var(--accent-retro)]" : "text-muted-foreground";
}

export function MemberRaRankSnapshotTable({ items, showSoftcorePoints = false }: Props) {
  const [showAll, setShowAll] = useState(false);

  const showSoftcore =
    showSoftcorePoints || items.some((item) => (item.totalSoftcorePoints ?? 0) > 0);

  const rows = useMemo(() => buildRankSnapshotRows(items), [items]);
  const visibleRows = showAll ? rows : rows.slice(0, initialVisibleRows);
  const hasMore = rows.length > initialVisibleRows;

  if (rows.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        No rank snapshots yet. They are recorded when score or rank sync runs.
      </p>
    );
  }

  return (
    <div className="space-y-3">
      <div className="md:overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border text-left text-muted-foreground">
              <th className="py-2 pr-4 font-medium">Synced</th>
              <th className="py-2 pr-4 font-medium">Site rank</th>
              <th className="py-2 pr-4 font-medium">Rank change</th>
              <th className="py-2 pr-4 font-medium">HC pts</th>
              <th className="py-2 pr-4 font-medium">HC change</th>
              <th className="py-2 pr-4 font-medium">True pts</th>
              {showSoftcore ? <th className="py-2 font-medium">SC pts</th> : null}
            </tr>
          </thead>
          <tbody>
            {visibleRows.map((row, index) => (
              <tr key={`${row.syncedAt}-${index}`} className="border-b border-border/60">
                <td className="py-2 pr-4 whitespace-nowrap text-muted-foreground">
                  <FormattedSyncTime value={row.syncedAt} />
                </td>
                <td className="py-2 pr-4 font-mono tabular-nums">
                  {formatRankLabel(row.rank, row.totalRanked)}
                </td>
                <td
                  className={`py-2 pr-4 font-mono tabular-nums ${deltaClass(row.rankDelta)}`}
                >
                  {formatRankDelta(row.rankDelta)}
                </td>
                <td className="py-2 pr-4 font-mono tabular-nums">
                  {row.totalPoints?.toLocaleString() ?? "—"}
                </td>
                <td
                  className={`py-2 pr-4 font-mono tabular-nums ${deltaClass(row.hardcorePointsDelta)}`}
                >
                  {formatPointsDelta(row.hardcorePointsDelta)}
                </td>
                <td className="py-2 pr-4 font-mono tabular-nums">
                  {row.totalTruePoints?.toLocaleString() ?? "—"}
                </td>
                {showSoftcore ? (
                  <td className="py-2 font-mono tabular-nums">
                    {row.totalSoftcorePoints?.toLocaleString() ?? "—"}
                  </td>
                ) : null}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {hasMore ? (
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => setShowAll((value) => !value)}
        >
          {showAll ? "Show fewer" : `Show all ${rows.length} snapshots`}
        </Button>
      ) : null}
    </div>
  );
}
